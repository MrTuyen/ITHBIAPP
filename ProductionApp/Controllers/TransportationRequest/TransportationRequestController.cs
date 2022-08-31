using ProductionApp.Helpers;
using ProductionApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProductionApp.Controllers.TransportationRequest
{
    public class TransportationRequestController : BaseController
    {
        private string Domain()
        {
            return db.TBL_SYSTEM.Single(a => a.id == "website").value;
        }

        private TBL_SYSTEM HR_TEAM_EMAIL()
        {
            return db.TBL_SYSTEM.Single(a => a.id == "HRADM");
        }

        // GET: TransportationRequest
        public ActionResult Index()
        {
            if (userLogin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var listRequest = new List<HR_Tran_Request>();

            if (userLogin.Username.ToLower() == "admin") // user là admin
            {
                listRequest = db.HR_Tran_Request.OrderByDescending(x => x.ID).Take(500).ToList();
            }
            else if(userLogin.Username.ToLower() == "baove")
            {
                listRequest = db.HR_Tran_Request.ToList()
                    .Where(x => DateTime.Parse(x.UsageDate).Date == DateTime.Now.Date)
                    .Where(x => x.HRApproved == (int)EnumHelper.HR_Action.Approve)
                    .OrderByDescending(x => x.ID).ToList();
            }
            else
            {
                //
                var hrTeam = db.TBL_SYSTEM.Where(x => x.value.ToLower() == userLogin.Email.ToLower() & x.value3 == "HR_TEAM").ToList();
                if (hrTeam.Count > 0)
                {
                    listRequest = db.HR_Tran_Request
                        //.Where(x => x.MgrApproved == (int)EnumHelper.Manager_Action.Approve && x.HRApproved == (int)EnumHelper.HR_Action.None)
                        .Where(x => x.HRApproved == (int)EnumHelper.HR_Action.None)
                        .OrderByDescending(x => x.ID)
                        .ToList();
                }
                else
                {
                    listRequest = db.HR_Tran_Request
                        .Where(x => (x.EmpID == userLogin.EmpID.ToLower() || x.MgrEmail.ToLower() == userLogin.Email.ToLower()))
                        .OrderByDescending(x => x.ID)
                        .Take(100)
                        .ToList();
                }
            }

            ViewData["User"] = userLogin;
            ViewData["ListVan"] = db.HR_Tran_Van_MST.ToList();
            return View(listRequest);
        }

        /// <summary>
        /// Add new transportation request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult AddNewRequest(HR_Tran_Request request)
        {
            string msg = string.Empty;
            try
            {
                var employee = db.OL_User_Approver.Where(x => x.UserCD == userLogin.Username).FirstOrDefault();

                request.Dept = userLogin.DeptID;
                request.EmpID = employee == null ? string.Empty : employee.EmpID;
                request.FullName = userLogin.Fullname;
                request.RequestEmail = userLogin.Email;
                request.RequestDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                request.MrgName = userLogin.ApproverName;
                request.MgrEmail = userLogin.ApproverEmail;
                request.MgrApproved = (int)EnumHelper.Manager_Action.None;
                request.HRName = HR_TEAM_EMAIL().fullname;
                request.HREmail = HR_TEAM_EMAIL().value;
                request.HRApproved = (int)EnumHelper.HR_Action.None;
                request.Active = true;

                db.HR_Tran_Request.Add(request);
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send email here
                    var subject = string.Format("Yêu cầu phê duyệt phương tiện số #{0} / Transportation request need your approval", request.ID);
                    var body = string.Format("Dear {0},", userLogin.ApproverName)
                        + string.Format("<br> Vui lòng phê duyệt đề nghị <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1} tại đây</a>", Domain(), request.ID)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Please review the request <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1} click here</a></span> <br>", Domain(), request.ID);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, userLogin.ApproverEmail, "", body);
                });

                // Return to client message
                var result = new
                {
                    id = request.ID,
                    empId = request.EmpID,
                    fullName = request.FullName,
                    dept = employee.Dept,
                    usageDate = request.UsageDate,
                    purpose = request.Purposes,
                    time = request.DepartureTime,
                    departure = request.Departure,
                    arrival = request.Arrival,
                    mrgApproved = (int)EnumHelper.Manager_Action.None,
                    hrApproved = (int)EnumHelper.HR_Action.None
                };

                msg = "Thêm mới yêu cầu thành công/ Add new request successfully.";
                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Manager approve for the request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public ActionResult Approve(int requestId)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Tran_Request.Where(x => x.ID == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != "admin" && userLogin.Email.ToLower() != objRequest.MgrEmail.ToLower())
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to approve this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.MgrApproved == (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request đã được duyệt/ The request has been approved.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.Reason = null;
                objRequest.MgrApproved = (int)EnumHelper.Manager_Action.Approve;
                objRequest.MgrApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt phương tiện số #{0} đã được duyệt/ Transportation request #{0} has been approved", objRequest.ID);
                    var body = string.Format("Hi HR teams,")
                        + string.Format("<br> Đề nghị phương tiện số <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1}</a> đã được duyệt, vui lòng xử lý.", Domain(), objRequest.ID)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1}</a> has been approved. Please process this request.</span> <br>", Domain(), objRequest.ID);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.HREmail, objRequest.RequestEmail, body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });

                msg = "Bạn đã duyệt thành công request/ The request has been approved.";
                return Json(new { rs = true, msg = msg, data = objRequest.ID });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Manager reject the request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public ActionResult Reject(int requestId, string reason)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Tran_Request.Where(x => x.ID == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != "admin" && userLogin.Email.ToLower() != objRequest.MgrEmail.ToLower())
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to approve this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.MgrApproved == (int)EnumHelper.Manager_Action.Reject)
                {
                    msg = "Yêu cầu đã bị từ chối/ The request has been rejected.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.HRApproved == (int)EnumHelper.HR_Action.Approve)
                {
                    msg = "Yêu cầu này đã được HR xử lý/ The request has been processed by HR.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.MgrApproved = (int)EnumHelper.Manager_Action.Reject;
                objRequest.MgrApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objRequest.Reason = reason;
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt phương tiện số #{0} đã bị từ chối/ Transportation request #{0} has been rejected.", objRequest.ID);
                    var body = string.Format("Hi {0},", objRequest.FullName)
                        + string.Format("<br> Đề nghị phương tiện số <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1}</a> đã bị từ chối..", Domain(), objRequest.ID)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1}</a> has been rejected.</span> <br>", Domain(), objRequest.ID)
                        + string.Format("<br><br> Lý do: {0}", objRequest.Reason)
                        + string.Format("<br>Reason: {0}", objRequest.Reason);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.RequestEmail, "", body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });

                msg = "Bạn đã từ chối duyệt request/ The request has been rejected.";
                return Json(new { rs = true, msg = msg, data = objRequest.ID });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Human Resource (HR_TEAM) process the request has been approved by manager
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="vanId"></param>
        /// <returns></returns>
        public ActionResult Process(int requestId, int vanId)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Tran_Request.Where(x => x.ID == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.MgrApproved != (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request này chưa được cấp quản lý duyệt/ The request is not approve by line manager.";
                    return Json(new { rs = false, msg = msg });
                }

                var hrTeam = db.TBL_SYSTEM.Where(x => x.value.ToLower() == userLogin.Email.ToLower() & x.value3 == "HR_TEAM").ToList();
                if (hrTeam.Count < 0)
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to process this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.HRApproved == (int)EnumHelper.HR_Action.Approve)
                {
                    msg = "Request đã được duyệt/ The request has been approved.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.Van = vanId;
                objRequest.HRApproved = (int)EnumHelper.HR_Action.Approve;
                objRequest.HREmail = userLogin.Email;
                objRequest.HRApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objRequest.Active = false;
                db.SaveChanges();

                var objVan = db.HR_Tran_Van_MST.Where(x => x.ID == vanId).FirstOrDefault();
                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt phương tiện số #{0} đã được xử lý/ Transportation request #{0} has been processed.", objRequest.ID);
                    var body = string.Format("Hi {0},", db.TBL_USERS_MST.Where(x => x.EMAIL == objRequest.RequestEmail).FirstOrDefault().FULLNAME)
                        + string.Format("<br> Đề nghị phương tiện số <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1}</a> đã được xử lý.", Domain(), objRequest.ID)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/TransportationRequest/Index#trans-{1}'>#{1}</a> has been processed.</span> <br>", Domain(), objRequest.ID)
                        + "<br><br>"
                        + string.Format("Thông tin xe: Biển số: {0}. Tài xế: {1}. Số điện thoại: {2}", objVan.NumberPlate, objVan.DriverName, objVan.DriverPhone)
                        + string.Format("<br>Van's info: Number plate: {0}. Driver name: {1}. Drive phone number: {2}", objVan.NumberPlate, objVan.DriverName, objVan.DriverPhone);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.RequestEmail, "", body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "", body);
                });

                msg = "Bạn đã duyệt thành công/ The request has been processed.";
                return Json(new { rs = true, msg = msg, data = objRequest.ID });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult GetRequestDetail(int requestId)
        {
            string msg = string.Empty;
            try
            {
                var request = db.HR_Tran_Request.Where(x => x.ID == requestId).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                var result = new
                {
                    id = request.ID,
                    empId = request.EmpID,
                    fullName = request.FullName,
                    dept = request.TBL_DEPARTMENT_MST.NAME,
                    usageDate = request.UsageDate,
                    purpose = request.Purposes,
                    time = request.DepartureTime,
                    departure = request.Departure,
                    arrival = request.Arrival,
                    mgrApproved = request.MgrApproved,
                    hrApproved = request.HRApproved,
                    mgrEmail = request.MgrEmail,
                    mgrName = request.MrgName,
                    hrName = request.HRName,
                    van = request.Van,
                    reason = request.Reason
                };

                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult AddVan(HR_Tran_Van_MST van)
        {
            string msg = string.Empty;
            try
            {
                db.HR_Tran_Van_MST.Add(van);
                db.SaveChanges();
                var result = new
                {
                    id = van.ID,
                    numberPlate = van.NumberPlate,
                    driverName = van.DriverName,
                    phone = van.DriverPhone,
                    active = van.Active
                };
                msg = "Thêm mới thông tin xe thành công/ Add new van's information successful.";
                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult UpdateVan(HR_Tran_Van_MST van)
        {
            string msg = string.Empty;
            try
            {
                var objVan = db.HR_Tran_Van_MST.Where(x => x.ID == van.ID).FirstOrDefault();
                if (objVan == null)
                {
                    msg = "Không tồn tại thông tin xe/ The van is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                objVan.DriverName = van.DriverName;
                objVan.DriverPhone = van.DriverPhone;
                db.SaveChanges();

                var result = new
                {
                    id = objVan.ID,
                    numberPlate = objVan.NumberPlate,
                    driverName = objVan.DriverName,
                    phone = objVan.DriverPhone,
                    active = objVan.Active
                };
                msg = "Cập nhật thông tin xe thành công/ Updated successful.";
                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult DeleteVan(int vanId)
        {
            string msg = string.Empty;
            try
            {
                var objVan = db.HR_Tran_Van_MST.Where(x => x.ID == vanId).FirstOrDefault();
                if (objVan == null)
                {
                    msg = "Không tồn tại thông tin xe/ The van is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                db.HR_Tran_Van_MST.Remove(objVan);
                db.SaveChanges();
                msg = "Xóa thông tin xe thành công/ Delete van's info successful.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }


        public ActionResult GetVanInfo(int vanId)
        {
            string msg = string.Empty;
            try
            {
                var objVan = db.HR_Tran_Van_MST.Where(x => x.ID == vanId).FirstOrDefault();
                if (objVan == null)
                {
                    msg = "Không tồn tại thông tin xe/ The van is not exist.";
                    return Json(new { rs = false, msg = msg });
                }
                var result = new
                {
                    id = objVan.ID,
                    numberPlate = objVan.NumberPlate,
                    driverName = objVan.DriverName,
                    phone = objVan.DriverPhone,
                    active = objVan.Active
                };
                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult GetListVan()
        {
            string msg = string.Empty;
            try
            {
                var result = db.HR_Tran_Van_MST
                            .Select(x => new
                            {
                                id = x.ID,
                                numberPlate = x.NumberPlate,
                                driverName = x.DriverName,
                                phone = x.DriverPhone,
                                active = x.Active
                            }).ToList();

                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Export request data to excel filter by request date
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public ActionResult Report(string fromDate, string toDate)
        {
            string msg = "";
            try
            {
                var listDepartments = db.TBL_DEPARTMENT_MST.ToList();
                var listVans = db.HR_Tran_Van_MST.ToList();
                var result = new List<PROC_GET_TRAN_REQUEST_Result>();
                result = db.PROC_GET_TRAN_REQUEST(fromDate, toDate).ToList();

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("#No");
                dtReport.Columns.Add("EmployeeId");
                dtReport.Columns.Add("Fullname");
                dtReport.Columns.Add("Department");
                dtReport.Columns.Add("Usage date");
                dtReport.Columns.Add("Purpose");
                dtReport.Columns.Add("Departure time");
                dtReport.Columns.Add("Departure");
                dtReport.Columns.Add("Arrival");
                dtReport.Columns.Add("Request date");
                dtReport.Columns.Add("Manager Approve date");
                dtReport.Columns.Add("HR Process date");
                dtReport.Columns.Add("Van Information");
                dtReport.Columns.Add("Reason");

                // Add data row
                foreach (var row in result)
                {
                    var van = listVans.Where(x => x.ID == row.Van).FirstOrDefault();
                    DataRow dr = dtReport.NewRow();
                    dr["#No"] = row.ID;
                    dr["EmployeeId"] = row.EmpID;
                    dr["Fullname"] = row.FullName;
                    dr["Department"] = listDepartments.Where(x => x.DEPT_ID == row.Dept).FirstOrDefault().NAME;
                    dr["Usage date"] = row.UsageDate;
                    dr["Purpose"] = row.Purposes;
                    dr["Departure time"] = row.DepartureTime;
                    dr["Departure"] = row.Departure;
                    dr["Arrival"] = row.Arrival;
                    dr["Request date"] = row.RequestDate;
                    dr["Manager Approve date"] = row.MgrApprovedDate;
                    dr["HR Process date"] = row.HRApprovedDate;
                    dr["Van Information"] = string.Format("Biển số: {0}. Tài xế: {1}. Số điện thoại: {2}", van.NumberPlate, van.DriverName, van.DriverPhone);
                    dr["Reason"] = row.Reason;

                    dtReport.Rows.Add(dr);
                }

                // FileName
                string strFileName = "Transportation_Request" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
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
                