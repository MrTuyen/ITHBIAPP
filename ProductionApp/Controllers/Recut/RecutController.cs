using Microsoft.AspNet.SignalR;
using ProductionApp.Common;
using ProductionApp.Helpers;
using ProductionApp.Models;
using ProductionApp.Models.Recut;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProductionApp.Controllers.Recut
{
    public class RecutController : BaseController
    {
        // GET: Recut
        public ActionResult Index()
        {
            var listRecut = db.TBL_RC_Request.ToList();

            return View(listRecut);
        }

        public ActionResult Add()
        {
            ViewData["Manager"] = db.WH_User_Approver.Where(s => s.value3 == "Prd_Approval").ToList();


            return View();
        }

        public ActionResult Detail(string id)
        {
            var data = id.Split('-');

            var requestId = int.Parse(data[0]);
            var request = db.TBL_RC_Request.Where(x => x.ID == requestId).FirstOrDefault();
            if (request == null)
            {
                return RedirectToAction("Index", "Recut");
            }

            if (data[1] == "issue")
            {
                ViewData["issue"] = true;
            }
            else
            {
                ViewData["issue"] = false;
            }
           
            ViewData["User"] = userLogin;
            return View(request);
        }

        public ActionResult DataDetail(string id)
        {
            var data = id.Split('-');

            var requestId = int.Parse(data[0]);
            var request = db.TBL_RC_Request_Data.Where(x => x.ID == requestId).FirstOrDefault();
            if (request == null)
            {
                return RedirectToAction("Index", "Recut");
            }

            if (data[1] == "issue")
            {
                ViewData["issue"] = true;
            }
            else
            {
                ViewData["issue"] = false;
            }

            ViewData["User"] = userLogin;
            return View(request);
        }

        public ActionResult WOIssues()
        {
            string msg = "";
            try
            {
                // Get records from TBL_RC_Request
                var list1 = db.TBL_RC_Request_Detail.Where(x => string.IsNullOrEmpty(x.WORecut)).Select(x => x.TBL_RC_Request).Distinct().ToList();
                ViewData["ListRCRequest"] = list1;

                // Get records from TBL_RC_Request_Data
                var list2 = db.TBL_RC_Request_Data_Detail.Where(x => string.IsNullOrEmpty(x.WORecut)).Select(x => x.TBL_RC_Request_Data).Distinct().ToList();
                ViewData["ListRCRequestData"] = list2;

                return View();
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult AddRequest(TBL_RC_Request request, List<TBL_RC_Request_Detail> detail)
        {
            string msg = "";
            try
            {
                request.RequestDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.RequestBy = userLogin.Username;

                var totalError = detail.Sum(x => x.CuttingError) + detail.Sum(x => x.FabricError);
                if (totalError <= 0)
                {
                   request.QCSewStatus = (int)EnumHelper.Action.None;
                   request.RequestManagerStatus = (int)EnumHelper.Action.Processing;
                }
                else
                {
                    request.QCSewStatus = (int)EnumHelper.Action.Processing;
                    request.RequestManagerStatus = (int)EnumHelper.Action.None;
                }

                request.PlanStatus = (int)EnumHelper.Action.None;
                request.CCDRequesStatus = (int)EnumHelper.Action.None;
                request.WHStatus = (int)EnumHelper.Action.None;
                request.CCDApproveStatus = (int)EnumHelper.Action.None;
                request.QCCutStatus = (int)EnumHelper.Action.None;
                request.ProductStatus = (int)EnumHelper.Action.None;

                db.TBL_RC_Request.Add(request);
                db.SaveChanges();

                detail.ForEach(x => x.RcID = request.ID);
                db.TBL_RC_Request_Detail.AddRange(detail);

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ActionType = 0,
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Thêm request thành công/ Add request successfully.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult QCSewClick(int id, int status)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "qasew")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                var totalError = request.TBL_RC_Request_Detail.Sum(x => x.CuttingError) + request.TBL_RC_Request_Detail.Sum(x => x.FabricError);
                if (totalError <= 0)
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                if (status == request.QCSewStatus)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.QCSewStatus = status;
                request.QCSewDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.QCSewBy = userLogin.Username;

                if (status == (int)EnumHelper.Action.Approve)
                {
                    request.RequestManagerStatus = (int)EnumHelper.Action.Processing;
                }
                else
                {
                    request.RequestManagerStatus = (int)EnumHelper.Action.None;
                }

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    ActionType = (int)EnumHelper.Recut_Action_Click.QASew,
                    Status = status,
                    Date = request.QCSewDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult ManagerClick(int id, int status)
        {
            string msg = "";
            try
            {
                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.RequestManager.ToLower() != userLogin.Email.ToLower())
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var totalError = request.TBL_RC_Request_Detail.Sum(x => x.CuttingError) + request.TBL_RC_Request_Detail.Sum(x => x.FabricError);
                if (totalError > 0)
                {
                    if (request.QCSewStatus != (int)EnumHelper.Action.Approve)
                    {
                        msg = "Request chưa được QA may duyệt/ Request is not approved by Sew QA";
                        return Json(new { rs = false, msg = msg });
                    }
                }

                if (status == request.RequestManagerStatus)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.RequestManagerStatus = status;
                request.ManagerDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                if (status == (int)EnumHelper.Action.Approve)
                {
                    request.PlanStatus = (int)EnumHelper.Action.Processing;
                }
                else
                {
                    request.PlanStatus = (int)EnumHelper.Action.None;
                }

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    ActionType = (int)EnumHelper.Recut_Action_Click.Manager,
                    Status = status,
                    Date = request.ManagerDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult PlanClick(int id, List<TBL_RC_Request_Detail> details)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "khsx")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.RequestManagerStatus != (int)EnumHelper.Action.Approve)
                {
                    msg = "Request chưa được quản lý sản xuất duyệt/ Request is not approved by Manager.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.PlanStatus == (int)EnumHelper.Action.Approve)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.PlanStatus = (int)EnumHelper.Action.Approve;
                request.PlanDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.PlanBy = userLogin.Username;

                foreach (var item in details)
                {
                    var detail = db.TBL_RC_Request_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                    detail.AltMaterialCode = item.AltMaterialCode;
                    detail.WORecut = item.WORecut;
                }

                request.CCDRequesStatus = (int)EnumHelper.Action.Processing;

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    Status = (int)EnumHelper.Action.Approve,
                    ActionType = (int)EnumHelper.Recut_Action_Click.Plan,
                    Date = request.PlanDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult PlanUpdate(int id, List<TBL_RC_Request_Detail> details, int isData)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "khsx")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                if (isData == 1) // = 1 then take request from tbl_rc_request else from tbl_rc_request_data
                {
                    var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                    if (request == null)
                    {
                        msg = "Không tồn tại request/ Request does not exits.";
                        return Json(new { rs = false, msg = msg });
                    }

                    if (request.PlanStatus != (int)EnumHelper.Action.Approve)
                    {
                        msg = "Request chưa được phòng kế hoạch duyệt/ The request has not been approved by Plan Dept.";
                        return Json(new { rs = false, msg = msg });
                    }

                    foreach (var item in details)
                    {
                        var detail = db.TBL_RC_Request_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                        detail.AltMaterialCode = item.AltMaterialCode;
                        detail.WORecut = item.WORecut;
                    }
                }
                else
                {
                    var request = db.TBL_RC_Request_Data.Where(x => x.ID == id).FirstOrDefault();
                    if (request == null)
                    {
                        msg = "Không tồn tại request/ Request does not exits.";
                        return Json(new { rs = false, msg = msg });
                    }

                    foreach (var item in details)
                    {
                        var detail = db.TBL_RC_Request_Data_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                        detail.AltMaterialCode = item.AltMaterialCode;
                        detail.WORecut = item.WORecut;
                    }
                }

                db.SaveChanges();

                msg = "Cập nhật thành công/ Update successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult CCDRequestClick(int id, List<TBL_RC_Request_Detail> details)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "ccdfabric")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.PlanStatus != (int)EnumHelper.Action.Approve)
                {
                    msg = "Request chưa được kế hoạch sản xuất duyệt/ Request is not approved by plan.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.CCDRequesStatus == (int)EnumHelper.Action.Approve)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.CCDRequesStatus = (int)EnumHelper.Action.Approve;
                request.CCDRequesDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.CCDRequestBy = userLogin.Username;

                foreach (var item in details)
                {
                    var detail = db.TBL_RC_Request_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                    detail.CcdQty = item.CcdQty;
                }

                request.WHStatus = (int)EnumHelper.Action.Processing;

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    Status = (int)EnumHelper.Action.Approve,
                    ActionType = (int)EnumHelper.Recut_Action_Click.CCDFabric,
                    Date = request.CCDRequesDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult WarehouseClick(int id, int status, List<TBL_RC_Request_Detail> details)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "whfabric")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.CCDRequesStatus != (int)EnumHelper.Action.Approve)
                {
                    msg = "Request chưa được CCD vải duyệt/ Request is not approved by CCD Fabric.";
                    return Json(new { rs = false, msg = msg });
                }

                if (status == request.WHStatus)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.WHStatus = status;
                request.WHDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.WHBy = userLogin.Username;

                if (status == (int)EnumHelper.Action.Approve)
                {
                    request.CCDApproveStatus = (int)EnumHelper.Action.Processing;

                    foreach (var item in details)
                    {
                        var detail = db.TBL_RC_Request_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                        detail.WHQty = item.WHQty;
                    }
                }
                else
                {
                    request.CCDApproveStatus = (int)EnumHelper.Action.None;
                    foreach (var item in details)
                    {
                        var detail = db.TBL_RC_Request_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                        detail.WHQty = 0;
                    }
                }

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    Status = status,
                    ActionType = (int)EnumHelper.Recut_Action_Click.Warehouse,
                    Date = request.WHDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult CCDApproveClick(int id, List<TBL_RC_Request_Detail> details)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "ccd")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.WHStatus != (int)EnumHelper.Action.Approve)
                {
                    msg = "Request chưa được warehouse duyệt/ Request is not approved by Warehouse.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.CCDApproveStatus == (int)EnumHelper.Action.Approve)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.CCDApproveStatus = (int)EnumHelper.Action.Approve;
                request.CCDApproveDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.CCDABy = userLogin.Username;

                foreach (var item in details)
                {
                    var detail = db.TBL_RC_Request_Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                    detail.CcdConfirm = item.CcdConfirm;
                }

                request.QCCutStatus = (int)EnumHelper.Action.Processing;
                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    Status = (int)EnumHelper.Action.Approve,
                    ActionType = (int)EnumHelper.Recut_Action_Click.CCD,
                    Date = request.CCDApproveDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult QCCutClick(int id, int status)
        {
            string msg = "";
            try
            {
                if (userLogin.Username.ToLower() != "qacut")
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.CCDApproveStatus != (int)EnumHelper.Action.Approve)
                {
                    msg = "Request chưa được CCD duyệt/ Request is not approved by CCD.";
                    return Json(new { rs = false, msg = msg });
                }

                if (status == request.QCCutStatus)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.QCCutStatus = status;
                request.QCCutDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.QCCutBy = userLogin.Username;

                if (status == (int)EnumHelper.Action.Approve)
                {
                    request.ProductStatus = (int)EnumHelper.Action.Processing;
                }
                else
                {
                    request.ProductStatus = (int)EnumHelper.Action.None;
                }

                db.SaveChanges();

                var signalR = new RecutModel()
                {
                    ID = request.ID,
                    Status = status,
                    ActionType = (int)EnumHelper.Recut_Action_Click.QACut,
                    Date = request.QCCutDate
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceivedRecut(signalR);

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult ProductionClick(int id)
        {
            string msg = "";
            try
            {
                var request = db.TBL_RC_Request.Where(x => x.ID == id).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request/ Request does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != request.RequestBy.ToLower())
                {
                    msg = "Bạn không có quyền thực hiện hành động này/ You do not have permisson to do this action.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.QCCutStatus != (int)EnumHelper.Action.Approve)
                {
                    msg = "Request chưa được QA Cut duyệt/ Request is not approved by Cut QA.";
                    return Json(new { rs = false, msg = msg });
                }

                if (request.ProductStatus == (int)EnumHelper.Action.Approve)
                {
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                request.ProductStatus = (int)EnumHelper.Action.Approve;
                request.ProductDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // Add to TBL_RC_Request_Data
                TBL_RC_Request_Data data = new TBL_RC_Request_Data()
                {
                    ID = request.ID,
                    WO = request.WO,
                    SellingStyle = request.SellingStyle,
                    Size = request.Size,
                    MnfStyle = request.MnfStyle,
                    MnfColor = request.MnfColor,
                    RequestDate = request.RequestDate,
                    RequestBy = request.RequestBy,
                    QCSewBy = request.QCSewBy,
                    QCSewStatus = request.QCSewStatus,
                    QCSewDate = request.QCSewDate,
                    RequestManager = request.RequestManager,
                    RequestManagerStatus = request.RequestManagerStatus,
                    ManagerDate = request.ManagerDate,
                    PlanBy = request.PlanBy,
                    PlanStatus = request.PlanStatus,
                    PlanDate = request.PlanDate,
                    CCDRequestBy = request.CCDRequestBy,
                    CCDRequesStatus = request.CCDRequesStatus,
                    CCDRequesDate = request.CCDRequesDate,
                    WHBy = request.WHBy,
                    WHStatus = request.WHStatus,
                    WHDate = request.WHDate,
                    CCDABy = request.CCDABy,
                    CCDApproveStatus = request.CCDApproveStatus,
                    CCDApproveDate  = request.CCDApproveDate,
                    QCCutBy = request.QCCutBy,
                    QCCutStatus = request.QCCutStatus,
                    QCCutDate = request.QCCutDate,
                    ProductStatus = request.ProductStatus,
                    ProductDate = request.ProductDate
                };
                db.TBL_RC_Request_Data.Add(data);

                List<TBL_RC_Request_Data_Detail> listDataDetail = new List<TBL_RC_Request_Data_Detail>();
                foreach (var item in request.TBL_RC_Request_Detail)
                {
                    TBL_RC_Request_Data_Detail detail = new TBL_RC_Request_Data_Detail()
                    {
                        ID = item.ID,
                        RcID = item.RcID,
                        MaterialCode = item.MaterialCode,
                        SewingError = item.SewingError,
                        CuttingError = item.CuttingError,
                        SewingLack = item.SewingLack,
                        CuttingLack = item.CuttingLack,
                        FabricError = item.FabricError,
                        SewingTest = item.SewingTest,
                        LeaderName = item.LeaderName,
                        AltMaterialCode = item.AltMaterialCode,
                        WORecut = item.WORecut,
                        CcdQty = item.CcdQty,
                        WHQty = item.WHQty,
                        CcdConfirm = item.CcdConfirm
                    };
                    listDataDetail.Add(detail);
                }
                db.TBL_RC_Request_Data_Detail.AddRange(listDataDetail);

                db.TBL_RC_Request.Remove(request);

                db.SaveChanges();

                msg = "Xử lý thành công/ Successful.";
                return Json(new { rs = true, msg = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Filter(FormSearchRecut formSearch)
        {
            var listData = new ConcurrentBag<TBL_RC_Request>();
            var tempData = db.PROC_GET_RECUT_REQUEST(formSearch.WO, formSearch.From, formSearch.To).ToList();

            int taskNum = 20;
            int qtyperTask = 0;
            CalTaskNumber(tempData.Count, ref qtyperTask, ref taskNum);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < taskNum; i++)
            {
                int tempI = i;
                var items = tempData.Skip(tempI * qtyperTask).Take(qtyperTask).ToList();
                var task = Task.Run(() =>
                {
                    foreach (var x in items)
                    {
                        var item = new TBL_RC_Request()
                        {
                             ID = x.ID,
                             WO = x.WO,
                             SellingStyle = x.SellingStyle,
                             Size = x.Size,
                             MnfColor = x.MnfColor,
                             MnfStyle = x.MnfStyle,
                             RequestDate = x.RequestDate,
                             RequestBy = x.RequestBy,
                             QCSewDate = x.QCSewDate,
                             QCSewStatus = x.QCSewStatus,
                             RequestManagerStatus = x.RequestManagerStatus,
                             ManagerDate = x.ManagerDate,
                             PlanStatus = x.PlanStatus,
                             PlanDate = x.PlanDate,
                             CCDRequesDate = x.CCDRequesDate,
                             CCDRequesStatus = x.CCDRequesStatus,
                             WHStatus = x.WHStatus,
                             WHDate = x.WHDate,
                             CCDApproveDate = x.CCDApproveDate,
                             CCDApproveStatus = x.CCDApproveStatus,
                             QCCutStatus = x.QCCutStatus,
                             QCCutDate = x.QCCutDate,
                             ProductStatus = x.ProductStatus,
                             ProductDate = x.ProductDate
                        };
                        listData.Add(item);
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            ViewData["Tags"] = formSearch;

            return View("Index", listData.ToList());
        }

        public ActionResult GetWOInfo(string wo)
        {          
            string msg = "";
            try
            {
                var objWo = db.TBL_KANBAN_DATA.Where(x => x.WLChild == wo).FirstOrDefault();

                if (objWo == null)
                {
                    msg = "Không tồn tại work order. / Work order does not exits.";
                    return Json(new { rs = false, msg = msg });
                }
                var rcComparts = db.TBL_RC_ComParts.Where(x => x.MnfStyle == objWo.MnfStyle).DistinctBy(x => new { x.MnfStyle , x.FabricCD }).ToList();

                var result = new
                {
                    sellingStyle = objWo.SellingStyle,
                    size = objWo.Size,
                    color = objWo.MnfColor,
                    mnfStyle = objWo.MnfStyle,
                    listMaterialCode = string.Join(",", rcComparts.Select(x => x.FabricCD))
                };

                return Json(new { rs = true, msg = msg, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Report(string fromDate, string toDate)
        {
            string msg = "";
            try
            {
                var listDataDetail = db.TBL_RC_Request_Data_Detail.ToList();
                var result = new List<PROC_GET_RECUT_REQUEST_DATA_Result>();
                result = db.PROC_GET_RECUT_REQUEST_DATA(fromDate, toDate).ToList();

                // Create header row
                DataTable dtReport = new DataTable();
                // Request
                dtReport.Columns.Add("ID");
                dtReport.Columns.Add("WO");
                dtReport.Columns.Add("SellingStyle");
                dtReport.Columns.Add("Size");
                dtReport.Columns.Add("MnfStyle");
                dtReport.Columns.Add("MnfColor");
                dtReport.Columns.Add("RequestDate");
                dtReport.Columns.Add("RequestBy");

                dtReport.Columns.Add("QCSewBy");
                dtReport.Columns.Add("QCSewDate");

                dtReport.Columns.Add("RequestManager");
                dtReport.Columns.Add("ManagerDate");

                dtReport.Columns.Add("PlanBy");
                dtReport.Columns.Add("PlanDate");

                dtReport.Columns.Add("CCDFabricBy");
                dtReport.Columns.Add("CCDFabricDate");

                dtReport.Columns.Add("WHBy");
                dtReport.Columns.Add("WHDate");

                dtReport.Columns.Add("CCDBy");
                dtReport.Columns.Add("CCDDate");

                dtReport.Columns.Add("QCCutBy");
                dtReport.Columns.Add("QCCutDate");

                dtReport.Columns.Add("ProductDate");

                //Detail
                dtReport.Columns.Add("MaterialCode");
                dtReport.Columns.Add("SewingError");
                dtReport.Columns.Add("CuttingError");
                dtReport.Columns.Add("SewingLack");
                dtReport.Columns.Add("CuttingLack");
                dtReport.Columns.Add("FabricError");
                dtReport.Columns.Add("SewingTest");
                dtReport.Columns.Add("LeaderName");
                dtReport.Columns.Add("AltMaterialCode");
                dtReport.Columns.Add("WORecut");
                dtReport.Columns.Add("CcdQty");
                dtReport.Columns.Add("WHQty");
                dtReport.Columns.Add("CcdConfirm");

                // Add data row
                foreach (var row in result)
                {
                    var detail = listDataDetail.Where(x => x.RcID == row.ID).ToList();
                    var isSameRow = false;
                    for (var i = 0; i < detail.Count; i++)
                    {
                        var itemDetail = detail[i];
                        DataRow dr = dtReport.NewRow();
                        if (i >= 1)
                            isSameRow = true;

                        dr["ID"] = isSameRow ? "" : row.ID.ToString();
                        dr["WO"] = isSameRow ? "" : row.WO;
                        dr["SellingStyle"] = isSameRow ? "" : row.SellingStyle;
                        dr["Size"] = isSameRow ? "" : row.Size;
                        dr["Size"] = isSameRow ? "" : row.MnfStyle;
                        dr["MnfColor"] = isSameRow ? "" : row.MnfColor;
                        dr["RequestDate"] = isSameRow ? "" : row.RequestDate;
                        dr["RequestBy"] = isSameRow ? "" : row.RequestBy;

                        dr["QCSewBy"] = isSameRow ? "" : row.QCSewBy;
                        dr["QCSewDate"] = isSameRow ? "" : row.QCSewDate;

                        dr["RequestManager"] = isSameRow ? "" : row.RequestManager;
                        dr["ManagerDate"] = isSameRow ? "" : row.ManagerDate;

                        dr["PlanBy"] = isSameRow ? "" : row.PlanBy;
                        dr["PlanDate"] = isSameRow ? "" : row.PlanDate;

                        dr["CCDFabricBy"] = isSameRow ? "" : row.CCDRequestBy;
                        dr["CCDFabricDate"] = isSameRow ? "" : row.CCDRequesDate;

                        dr["WHBy"] = isSameRow ? "" : row.WHBy;
                        dr["WHDate"] = isSameRow ? "" : row.WHDate;

                        dr["CCDBy"] = isSameRow ? "" : row.CCDABy;
                        dr["CCDDate"] = isSameRow ? "" : row.CCDApproveDate;

                        dr["QCCutBy"] = isSameRow ? "" : row.QCCutBy;
                        dr["QCCutDate"] = isSameRow ? "" : row.QCCutDate;

                        dr["ProductDate"] = isSameRow ? "" : row.ProductDate;

                        dr["MaterialCode"] = itemDetail.MaterialCode;
                        dr["SewingError"] = itemDetail.SewingError;
                        dr["CuttingError"] = itemDetail.CuttingError;
                        dr["SewingLack"] = itemDetail.SewingLack;
                        dr["CuttingLack"] = itemDetail.CuttingLack;
                        dr["FabricError"] = itemDetail.FabricError;
                        dr["SewingTest"] = itemDetail.SewingTest;
                        dr["LeaderName"] = itemDetail.LeaderName;
                        dr["AltMaterialCode"] = itemDetail.AltMaterialCode;
                        dr["WORecut"] = itemDetail.WORecut;
                        dr["CcdQty"] = itemDetail.CcdQty;
                        dr["WHQty"] = itemDetail.WHQty;
                        dr["CcdConfirm"] = itemDetail.CcdConfirm;

                        dtReport.Rows.Add(dr);
                    }
                }

                // FileName
                string strFileName = "Recut_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
                Response.AppendHeader("Set-Cookie", "fileDownload=true; path=/");
                return PushFile(dtReport, strFileName);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }
    }
}