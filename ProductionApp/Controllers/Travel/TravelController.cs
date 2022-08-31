using ProductionApp.Helpers;
using ProductionApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProductionApp.Controllers.Travel
{
    public class TravelController : BaseController
    {
        private const string TravelConfig = "TravelConfig";

        private string Domain()
        {
            return db.TBL_SYSTEM.Single(a => a.id == "website").value;
        }

        private TBL_SYSTEM HR_TEAM_Email()
        {
            return db.TBL_SYSTEM.Single(a => a.id == "HRADM");
        }

        private TBL_SYSTEM PM_Email()
        {
            return db.TBL_SYSTEM.Single(a => a.value3 == "PM");
        }

        private TBL_SYSTEM HRM_Email()
        {
            return db.TBL_SYSTEM.Single(a => a.value3 == "HRM");
        }

        public ActionResult Index()
        {
            var objSystem = db.TBL_SYSTEM.FirstOrDefault(x => x.id == TravelConfig);
            ViewData["Delegate"] = objSystem == null ? false : bool.Parse(objSystem.value);
            ViewData["User"] = userLogin;
            ViewData["Destination"] = db.TBL_Travel_Destination.ToList();
            ViewData["PlantManager"] = PM_Email().value;

            var listTravel = new List<HR_Travel_Request>();
            if (userLogin.Username.ToLower() == "admin") // user là admin
            {
                listTravel = db.HR_Travel_Request.OrderByDescending(x => x.Id).Take(500).ToList();
            }
            else
            {
                var hrTeam = db.TBL_SYSTEM.Where(x => x.value.ToLower() == userLogin.Email.ToLower() & x.value3 == "HR_TEAM").ToList();
                if (hrTeam.Count > 0)
                {
                    listTravel = db.HR_Travel_Request
                        .Where(x => x.ManagerApproved == (int)EnumHelper.Manager_Action.Approve)
                        .Where(x => x.SManagerApproved == (int)EnumHelper.Manager_Action.Approve)
                        .Where(x => x.HRApproved == (int)EnumHelper.HR_Action.None)
                        .OrderByDescending(x => x.Id)
                        .ToList();
                }
                else
                {
                    listTravel = db.HR_Travel_Request
                        .Where(x => (x.EmpId == userLogin.EmpID || x.ManagerMail.ToLower() == userLogin.Email.ToLower() || x.SManagerMail.ToLower() == userLogin.Email.ToLower()))
                        .OrderByDescending(x => x.Id)
                        .Take(100)
                        .ToList();
                }
            }
            return View(listTravel);
        }

        public ActionResult UploadFile()
        {
            string msg = "";
            int type = int.Parse(Request.Form["type"]);
            var dt = DateTime.Now;
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    foreach (string file in Request.Files)
                    {
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            string path = Server.MapPath("~/Uploads/Travel/");
                            string extension = Path.GetExtension(path + fileContent.FileName);
                            if (extension != ".xls" && extension != ".xlsx" && extension != ".doc" && extension != ".docx" && extension != ".pdf")
                                return Json(new { rs = false, msg = "File không đúng định dạng, Hỗ trợ định dạng .xls,.xlsx, .doc, .docx, .pdf" });

                            // add time prefix to avoid override file has the same name :D so brilliant =))
                            string fileName = string.Format("{0}{1}{2}{3}{4}{5}{6}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Ticks) + "_" + fileContent.FileName;
                            string fullPath = path + fileName;
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            foreach (string key in Request.Files)
                            {
                                HttpPostedFileBase postedFile = Request.Files[key];
                                postedFile.SaveAs(fullPath);
                            }

                            switch (type)
                            {
                                case 1: { CacheHelper.Set("HotelFilePath", fileName); } break;
                                case 2: { CacheHelper.Set("TicketFilePath", fileName); } break;
                            }

                            if (msg.Length > 0) return Json(new { rs = false, msg = msg });
                            return Json(new { rs = true, fileName = fileContent.FileName, msg = "Tệp dữ liệu đã tải lên thành công." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    // Returns message that successfully uploaded  
                    msg = "File Uploaded Successfully!";
                    return Json(new { rs = true, msg = msg });
                }
                catch (Exception ex)
                {
                    Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                    msg = "Error occurred. Error details: " + ex.Message;
                    return Json(new { rs = false, msg = msg });
                }
            }
            else
            {
                msg = "No files selected.";
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult AddNewRequest(HR_Travel_Request request)
        {
            string msg = string.Empty;
            try
            {
                var position = db.TBL_Positions_MST.ToList();
                var objSystem = db.TBL_SYSTEM.FirstOrDefault(x => x.id == TravelConfig);
                var isDelegate = objSystem == null ? false : bool.Parse(objSystem.value);
                var group = db.TBL_DEPARTMENT_MST.Where(x => x.DEPT_ID == userLogin.DeptID).FirstOrDefault().GROUP;
                var sManagerEmail = group == 1 ? PM_Email().value : (isDelegate ? HRM_Email().value : PM_Email().value);
                var employee = db.OL_User_Approver.Where(x => x.UserCD == userLogin.Username).FirstOrDefault();

                request.Position = userLogin.POSID;
                request.Department = userLogin.DeptID;
                request.EmpId = employee == null ? string.Empty : employee.EmpID;
                request.Name = userLogin.Fullname;
                request.EmpEmail = userLogin.Email;
                request.RequestDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                request.ManagerMail = userLogin.ApproverEmail;
                request.ManagerApproved = (int)EnumHelper.Action.None;

                request.SManagerApproved = (int)EnumHelper.Action.None;
                request.SManagerMail = sManagerEmail;

                request.HREmail = HR_TEAM_Email().value;
                request.HRApproved = (int)EnumHelper.HR_Action.None;
                request.Active = true;

                db.HR_Travel_Request.Add(request);
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Send email to manager
                    var subject = string.Format("Yêu cầu phê duyệt công tác số #{0} / Business trip request need your approval", request.Id);
                    var body = string.Format("Dear {0},", userLogin.ApproverName)
                        + string.Format("<br> Vui lòng phê duyệt đề nghị <a href='{0}/Travel/Index#travel-{1}'>#{1} tại đây</a>", Domain(), request.Id)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Please review the request <a href='{0}/Travel/Index#travel-{1}'>#{1} click here</a></span> <br>", Domain(), request.Id);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, request.ManagerMail, "", body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "Tuyen.Nguyen@hanes.com", "Tuyen.Nguyen@hanes.com", "", body);
                });

                var objPos = position.FirstOrDefault(x => x.ID == request.Position);
                var result = new
                {
                    id = request.Id,
                    name = request.Name,
                    position = objPos == null ? "" : objPos.NAME,
                    department = request.TBL_DEPARTMENT_MST == null ? "" : request.TBL_DEPARTMENT_MST.NAME,
                    purpose = request.Purpose,
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

        public ActionResult ManagerApprove(int requestId)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Travel_Request.Where(x => x.Id == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != "admin" && userLogin.Email.ToLower() != objRequest.ManagerMail.ToLower())
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to approve this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.SManagerApproved != (int)EnumHelper.Manager_Action.None)
                {
                    msg = "Request đã được quản lý cấp cao duyệt/ The request has been approved by senior manager.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.ManagerApproved == (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request đã được duyệt/ The request has been approved.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.Note = "";
                objRequest.ManagerApproved = (int)EnumHelper.Manager_Action.Approve;
                objRequest.ManagerApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var sManagerName = db.TBL_SYSTEM.Where(x => x.value == objRequest.SManagerMail).FirstOrDefault().fullname;
                    var subject = string.Format("Đề nghị phê duyệt công tác số #{0} đã được duyệt/ Business trip request #{0} has been approved", objRequest.Id);
                    var body = string.Format("Dear {0},", sManagerName)
                        + string.Format("<br> Đề nghị công tác số <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> đã được duyệt, vui lòng xử lý.", Domain(), objRequest.Id)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> has been approved. Please process this request.</span> <br>", Domain(), objRequest.Id);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.SManagerMail, objRequest.EmpEmail, body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });
                
                msg = "Bạn đã duyệt thành công request/ The request has been approved.";
                return Json(new { rs = true, msg = msg, data = objRequest.Id });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult ManagerReject(int requestId, string note)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Travel_Request.Where(x => x.Id == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != "admin" && userLogin.Email.ToLower() != objRequest.ManagerMail.ToLower())
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to approve this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.SManagerApproved != (int)EnumHelper.Manager_Action.None)
                {
                    msg = "Request đã được quản lý cấp cao duyệt/ The request has been approved by senior manager.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.ManagerApproved == (int)EnumHelper.Manager_Action.Reject)
                {
                    msg = "Yêu cầu đã bị từ chối/ The request has been rejected.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.HRApproved == (int)EnumHelper.HR_Action.Approve)
                {
                    msg = "Yêu cầu này đã được HR xử lý/ The request has been processed by HR.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.ManagerApproved = (int)EnumHelper.Manager_Action.Reject;
                objRequest.ManagerApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objRequest.Note = note;
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt công tác số #{0} đã bị từ chối/ Business trip request #{0} has been rejected.", objRequest.Id);
                    var body = string.Format("Hi {0},", objRequest.Name)
                        + string.Format("<br> Đề nghị công tác số <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> đã bị từ chối..", Domain(), objRequest.Id)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> has been rejected.</span> <br>", Domain(), objRequest.Id)
                        + string.Format("<br><br> Lý do: {0}", objRequest.Note)
                        + string.Format("<br><span style='color:#0070c0;font-style: italic;'>Reason: {0} </span>", objRequest.Note);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.EmpEmail, "", body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });

                msg = "Bạn đã từ chối duyệt request/ The request has been rejected.";
                return Json(new { rs = true, msg = msg, data = objRequest.Id });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult SManagerApprove(int requestId)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Travel_Request.Where(x => x.Id == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != "admin" && userLogin.Email.ToLower() != objRequest.SManagerMail.ToLower())
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to approve this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.ManagerApproved != (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request chưa được cấp quản lý duyệt/ The request has not been approved by line manager.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.SManagerApproved == (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request đã được duyệt/ The request has been approved.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.HRApproved == (int)EnumHelper.HR_Action.Approve)
                {
                    msg = "Yêu cầu này đã được HR xử lý/ The request has been processed by HR.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.Note = "";
                objRequest.SManagerApproved = (int)EnumHelper.Manager_Action.Approve;
                objRequest.SManagerApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                db.SaveChanges();

                Task.Run(() => {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt công tác số #{0} đã được duyệt/ Business trip request #{0} has been approved", objRequest.Id);
                    var body = string.Format("Hi HR teams,")
                        + string.Format("<br> Đề nghị công tác số <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> đã được duyệt, vui lòng xử lý.", Domain(), objRequest.Id)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> has been approved. Please process this request.</span> <br>", Domain(), objRequest.Id);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.HREmail, objRequest.EmpEmail, body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });

                msg = "Bạn đã duyệt thành công request/ The request has been approved.";
                return Json(new { rs = true, msg = msg, data = objRequest.Id });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult SManagerReject(int requestId, string note)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Travel_Request.Where(x => x.Id == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (userLogin.Username.ToLower() != "admin" && userLogin.Email.ToLower() != objRequest.SManagerMail.ToLower())
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have permission to approve this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.ManagerApproved != (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request chưa được cấp quản lý duyệt/ The request has not been approved by line manager.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.SManagerApproved == (int)EnumHelper.Manager_Action.Reject)
                {
                    msg = "Yêu cầu đã bị từ chối/ The request has been rejected.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.HRApproved == (int)EnumHelper.HR_Action.Approve)
                {
                    msg = "Yêu cầu này đã được HR xử lý/ The request has been processed by HR.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.SManagerApproved = (int)EnumHelper.Manager_Action.Reject;
                objRequest.SManagerApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objRequest.Note = note;
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt công tác số #{0} đã bị từ chối/ Business trip request #{0} has been rejected.", objRequest.Id);
                    var body = string.Format("Hi {0},", objRequest.Name)
                        + string.Format("<br> Đề nghị phương tiện số <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> đã bị từ chối.", Domain(), objRequest.Id)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> has been rejected.</span> <br>", Domain(), objRequest.Id)
                        + string.Format("<br><br> Lý do: {0}", objRequest.Note)
                        + string.Format("<br><span style='color:#0070c0;font-style: italic;'>Reason: {0} </span>", objRequest.Note);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.EmpEmail, objRequest.ManagerMail, body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });

                msg = "Bạn đã từ chối duyệt request/ The request has been rejected.";
                return Json(new { rs = true, msg = msg, data = objRequest.Id });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Process(int requestId)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Travel_Request.Where(x => x.Id == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.ManagerApproved != (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request này chưa được cấp quản lý duyệt/ The request is not approve by line manager.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.SManagerApproved != (int)EnumHelper.Manager_Action.Approve)
                {
                    msg = "Request này chưa được cấp quản lý cấp cao duyệt/ The request is not approve by senior manager.";
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
                    msg = "Request đã được xử lý/ The request has been processed.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.HotelLink = CacheHelper.Get("HotelFilePath") == null ? string.Empty : CacheHelper.Get("HotelFilePath").ToString();
                objRequest.AirTicketLink = CacheHelper.Get("TicketFilePath") == null ? string.Empty : CacheHelper.Get("HotelFilePath").ToString();
                objRequest.HRApproved = (int)EnumHelper.HR_Action.Approve;
                objRequest.HREmail = userLogin.Email;
                objRequest.HRApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objRequest.Active = false;
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt phương tiện số #{0} đã được xử lý/ Business trip request #{0} has been processed.", objRequest.Id);
                    var body = string.Format("Hi {0},", objRequest.Name)
                        + string.Format("<br> Đề nghị công tác số <a href='{0}/Travel/Index#travel-{1}'>#{1}</a> đã được xử lý.", Domain(), objRequest.Id)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/Travel/Index#trans-{1}'>#{1}</a> has been processed.</span> <br>", Domain(), objRequest.Id);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, objRequest.EmpEmail, "", body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", "tuyen.nguyen@hanes.com", body);
                });
                 
                msg = "Bạn đã duyệt thành công/ The request has been processed.";
                return Json(new { rs = true, msg = msg, data = objRequest.Id });
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
                var request = db.HR_Travel_Request.Where(x => x.Id == requestId).FirstOrDefault();
                if (request == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                var listUsers = db.TBL_USERS_MST.ToList();
                var managerName = listUsers.Where(x => (x.EMAIL == null ? "" : x.EMAIL).ToLower() == request.ManagerMail).FirstOrDefault();
                var seniorManager = listUsers.Where(x => x.EMAIL == request.SManagerMail).FirstOrDefault();
                var hrName = listUsers.Where(x => x.EMAIL == request.HREmail).FirstOrDefault();

                var result = new
                {
                     id = request.Id,
                     empId = request.EmpId,
                     name = request.Name,
                     position = request.TBL_Positions_MST.NAME,
                     dept = request.TBL_DEPARTMENT_MST.NAME,
                     purpose = request.Purpose,
                     manager = managerName == null ? "" : managerName.FULLNAME,
                     managerMail = request.ManagerMail,
                     seniorManager = seniorManager == null ? "" : seniorManager.FULLNAME,
                     seniorManagerMail = request.SManagerMail,
                     hrName = hrName == null ? "" : hrName.FULLNAME,
                     destination = request.Destination,
                     deptDate = request.DepartureDate,
                     deptFrom = request.DepartureFrom,
                     deptTo = request.DepartureTo,
                     returnDate = request.ReturnDate,
                     returnFrom = request.ReturnFrom,
                     returnTo = request.ReturnTo,
                     hotelLink = request.HotelLink,
                     ticketLink = request.AirTicketLink,
                     note = request.Note
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

        public ActionResult AddDestination(TBL_Travel_Destination destination)
        {
            string msg = string.Empty;
            try
            {
                var objDes = db.TBL_Travel_Destination.ToList().Where(x => x.Name.ToLower().NonUnicode() == destination.Name.ToLower().NonUnicode()).FirstOrDefault();
                if (objDes != null)
                {
                    msg = "Địa điểm này đã tồn tại / The destination has been existed.";
                   return Json(new { rs = false, msg = msg});
                }

                db.TBL_Travel_Destination.Add(destination);
                db.SaveChanges();
                var result = new
                {
                    id = destination.Id,
                    name = destination.Name                    
                };
                msg = "Thêm mới thông tin địa điểm thành công/ Add new destination successful.";
                return Json(new { rs = true, msg = msg, data = result });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult GetListDestination()
        {
            string msg = string.Empty;
            try
            {
                var result = db.TBL_Travel_Destination
                            .Select(x => new
                            {
                                id = x.Id,
                                name = x.Name,
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

        public ActionResult Delegate(bool isChecked)
        {
            string msg = string.Empty;
            try
            {
                var objSystem = db.TBL_SYSTEM.FirstOrDefault(x => x.id == TravelConfig);
                if (objSystem == null)
                {
                    db.TBL_SYSTEM.Add(new TBL_SYSTEM()
                    {
                        id = TravelConfig,
                        value = isChecked.ToString()
                    });
                }
                else
                {
                    objSystem.value = isChecked.ToString();
                }
                
                db.SaveChanges();

                msg = isChecked ? "Ủy quyền thành công / Delegate succesful." : "Hủy bỏ ủy quyền thành công / Undelegate succesful.";
                return Json(new { rs = true, msg = msg });
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
                var listDepartments = db.TBL_DEPARTMENT_MST.ToList();
                var listPositions = db.TBL_Positions_MST.ToList();
                var result = new List<PROC_GET_TRAVEL_REQUEST_Result>();
                result = db.PROC_GET_TRAVEL_REQUEST(fromDate, toDate).ToList();

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("#No");
                dtReport.Columns.Add("Fullname");
                dtReport.Columns.Add("Department");
                dtReport.Columns.Add("Position");
                dtReport.Columns.Add("Purpose");
                dtReport.Columns.Add("Destination");
                dtReport.Columns.Add("Departure date");
                dtReport.Columns.Add("Departure from");
                dtReport.Columns.Add("Departure to");

                dtReport.Columns.Add("Return date");
                dtReport.Columns.Add("Return from");
                dtReport.Columns.Add("Return to");

                dtReport.Columns.Add("Request date");
                dtReport.Columns.Add("Manager Approve date");
                dtReport.Columns.Add("Senior Manager Approve date");
                dtReport.Columns.Add("HR Process date");
                dtReport.Columns.Add("Note");

                // Add data row
                foreach (var row in result)
                {
                    DataRow dr = dtReport.NewRow();
                    dr["#No"] = row.Id;
                    dr["Fullname"] = row.Name;
                    dr["Department"] = listDepartments.FirstOrDefault(x => x.DEPT_ID == row.Department).NAME;
                    dr["Position"] = listPositions.FirstOrDefault(x => x.ID == row.Position).NAME;
                    dr["Purpose"] = row.Purpose;
                    dr["Destination"] = row.Destination;
                    dr["Departure date"] = row.DepartureDate;
                    dr["Departure from"] = row.DepartureFrom;
                    dr["Departure to"] = row.DepartureTo;
                    dr["Return date"] = row.ReturnDate;
                    dr["Return from"] = row.ReturnFrom;
                    dr["Return to"] = row.ReturnTo;

                    dr["Request date"] = row.RequestDate;
                    dr["Manager Approve date"] = row.ManagerApprovedDate;
                    dr["Senior Manager Approve date"] = row.SManagerApprovedDate;
                    dr["HR Process date"] = row.HRApprovedDate;
                    dr["Note"] = row.Note;

                    dtReport.Rows.Add(dr);
                }

                // FileName
                string strFileName = "Travel_Request" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
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