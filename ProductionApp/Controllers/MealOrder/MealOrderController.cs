using Newtonsoft.Json;
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

namespace ProductionApp.Controllers.MealOrder
{
    public class MealOrderController : BaseController
    {

        private string Domain()
        {
            return db.TBL_SYSTEM.Single(a => a.id == "website").value;
        }

        private TBL_SYSTEM HR_TEAM_EMAIL()
        {
            return db.TBL_SYSTEM.Single(a => a.id == "HRADM");
        }

        // GET: MealOrder
        public ActionResult Index()
        {
            try
            {


                if (userLogin == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var listMealOrders = new List<HR_Meals_Order>();

                if (userLogin.Username.ToLower() == "admin") // user là admin
                {
                    listMealOrders = db.HR_Meals_Order.OrderByDescending(x => x.ID).Take(500).ToList();
                }
                else
                {
                    var hrTeam = db.TBL_SYSTEM
                        .Where(x => x.value.ToLower() == userLogin.Email.ToLower() & x.value3 == "HR_TEAM").ToList();
                    if (hrTeam.Count > 0)
                    {
                        listMealOrders = db.HR_Meals_Order
                            .Where(x => x.HRApproved == (int) EnumHelper.HR_Action.None)
                            .OrderByDescending(x => x.ID)
                            .ToList();
                    }
                    else
                    {
                        listMealOrders = db.HR_Meals_Order
                            .Where(x => (x.EmpID == userLogin.EmpID.ToLower()))
                            .OrderByDescending(x => x.ID)
                            .Take(100)
                            .ToList();
                    }
                }

                ViewData["User"] = userLogin;
                ViewData["Shift"] = db.TBL_SHIFT_MST.ToList();
                return View(listMealOrders);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
           
                return View();
            }
        }

        /// <summary>
        /// Upload file excel from client
        /// </summary>
        /// <returns></returns>
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
                            string path = Server.MapPath("~/Uploads/MealOrder/");
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
                                case 1: { CacheHelper.Set("MilkFilePath", fileName); } break;
                                case 2: { CacheHelper.Set("WaterFilePath", fileName); } break;
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

        /// <summary>
        /// Add new transportation request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult AddNewRequest(HR_Meals_Order request)
        {
            string msg = string.Empty;
            try
            {
                var employee = db.OL_User_Approver.Where(x => x.UserCD == userLogin.Username).FirstOrDefault();

                request.Dept = userLogin.DeptID;
                request.EmpID = employee == null ? string.Empty : employee.EmpID;
                request.FullName = userLogin.Fullname;
                request.RequestEmail = userLogin.Email;
                request.RequestDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss" ,CultureInfo.InvariantCulture);
                request.HRName = HR_TEAM_EMAIL().fullname;
                request.HREmail = HR_TEAM_EMAIL().value;
                request.HRApproved = (int)EnumHelper.HR_Action.None;
                request.Active = true;

                // Get file path
                request.MilkFilePath = CacheHelper.Get("MilkFilePath") == null ? string.Empty : CacheHelper.Get("MilkFilePath").ToString();
                request.WaterFilePath = CacheHelper.Get("WaterFilePath") == null ? string.Empty : CacheHelper.Get("WaterFilePath").ToString();

                db.HR_Meals_Order.Add(request);
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send email here
                    var subject = string.Format("Yêu cầu phê duyệt đề nghị suất ăn số #{0} / Meal order request need your approval", request.ID);
                    var body = string.Format("Dear HR Teams,")
                        + string.Format("<br> Vui lòng phê duyệt đề nghị suất ăn <a href='{0}/MealOrder/Index#meal-{1}'>#{1} tại đây</a>", Domain(), request.ID)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Please review the request <a href='{0}/MealOrder/Index#meal-{1}'>#{1} click here</a></span> <br>", Domain(), request.ID);
                    var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, HR_TEAM_EMAIL().value, "", body);
                    //var isSendMailSuccess = Utilities.SendEmail(subject, userLogin.Email, userLogin.Email, "", body);
                });

                // Return to client message
                var result = new
                {
                    id = request.ID,
                    empId = request.EmpID,
                    fullName = request.FullName,
                    dept = employee.Dept,
                    shift = db.TBL_SHIFT_MST.FirstOrDefault(x => x.SHIFT_ID == request.Shift).NAME,
                    qtyMeal = request.QtyMeal,
                    qtyMilk = request.QtyMilk,
                    qtyWater = request.QtyWater,
                    total = request.QtyMeal + request.QtyMilk + request.QtyWater,
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
            finally
            {
                CacheHelper.Remove("MilkFilePath");
                CacheHelper.Remove("WaterFilePath");
            }

        }

        /// <summary>
        /// Human Resource (HR_TEAM) process the request has been approved by manager
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="vanId"></param>
        /// <returns></returns>
        public ActionResult Process(int requestId, string comment)
        {
            string msg = string.Empty;
            try
            {
                var objRequest = db.HR_Meals_Order.Where(x => x.ID == requestId).FirstOrDefault();
                if (objRequest == null)
                {
                    msg = "Không tồn tại request / The request is not exist.";
                    return Json(new { rs = false, msg = msg });
                }

                var hrTeam = db.TBL_SYSTEM.Where(x => x.value.ToLower() == userLogin.Email.ToLower() & x.value3 == "HR_TEAM").ToList();
                if (hrTeam.Count < 0)
                {
                    msg = "Bạn không có quyền duyệt request này/ You do not have right to process this request.";
                    return Json(new { rs = false, msg = msg });
                }

                if (objRequest.HRApproved == (int)EnumHelper.HR_Action.Approve)
                {
                    msg = "Request đã được duyệt/ The request has been approved.";
                    return Json(new { rs = false, msg = msg });
                }

                objRequest.Comment = comment;
                objRequest.HRApproved = (int)EnumHelper.HR_Action.Approve;
                objRequest.HREmail = userLogin.Email;
                objRequest.HRApprovedDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss" ,CultureInfo.InvariantCulture);
                objRequest.Active = false;
                db.SaveChanges();

                Task.Run(() =>
                {
                    // Code send mail here
                    var subject = string.Format("Đề nghị phê duyệt phương tiện số #{0} đã được xử lý/ Transportation request #{0} has been processed.", objRequest.ID);
                    var body = string.Format("Hi {0},", db.TBL_USERS_MST.Where(x => x.EMAIL == objRequest.RequestEmail).FirstOrDefault().FULLNAME)
                        + string.Format("<br> Đề nghị suất ăn số <a href='{0}/MealOrder/Index#meal-{1}'>#{1}</a> đã được xử lý.", Domain(), objRequest.ID)
                        + string.Format("<br> <span style='color:#0070c0;font-style: italic;'>Request <a href='{0}/MealOrder/Index#meal-{1}'>#{1}</a> has been processed.</span> <br>", Domain(), objRequest.ID)
                        + "<br><br>"
                        + string.Format("Ghi chú/ Comment: {0}", objRequest.Comment);
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
                var request = db.HR_Meals_Order.Where(x => x.ID == requestId).FirstOrDefault();
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
                    shift = request.Shift,
                    qtyMeal = request.QtyMeal,
                    qtyMilk = request.QtyMilk,
                    milkFilePath = request.MilkFilePath,
                    waterFilePath = request.WaterFilePath,
                    qtyWater = request.QtyWater,
                    total = request.QtyMeal + request.QtyMilk + request.QtyWater,
                    comment = request.Comment,
                    hrApproved = request.HRApproved,
                    hrName = request.HRName,
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
                var listShift = db.TBL_SHIFT_MST.ToList();
                var result = new List<PROC_GET_MEAL_ORDER_Result>();
                result = db.PROC_GET_MEAL_ORDER(fromDate, toDate).ToList();

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("#No");
                dtReport.Columns.Add("EmployeeId");
                dtReport.Columns.Add("Fullname");
                dtReport.Columns.Add("Department");
                dtReport.Columns.Add("Shift");
                dtReport.Columns.Add("Quantity of Meals");
                dtReport.Columns.Add("Quantity of Milk cake");
                dtReport.Columns.Add("Quantity of Water");
                dtReport.Columns.Add("Total");
                dtReport.Columns.Add("Comment");
                dtReport.Columns.Add("Request date");
                dtReport.Columns.Add("HR Process date");

                // Add data row
                foreach (var row in result)
                {
                    DataRow dr = dtReport.NewRow();
                    dr["#No"] = row.ID;
                    dr["EmployeeId"] = row.EmpID;
                    dr["Fullname"] = row.FullName;
                    dr["Department"] = listDepartments.Where(x => x.DEPT_ID == row.Dept).FirstOrDefault().NAME;
                    dr["Shift"] = listShift.FirstOrDefault(x => x.SHIFT_ID == row.Shift).NAME;
                    dr["Quantity of Meals"] = row.QtyMeal;
                    dr["Quantity of Milk cake"] = row.QtyMilk;
                    dr["Quantity of Water"] = row.QtyWater;
                    dr["Total"] = row.QtyMeal + row.QtyMilk + row.QtyWater;
                    dr["Comment"] = row.Comment;
                    dr["Request date"] = row.RequestDate;
                    dr["HR Process date"] = row.HRApprovedDate;

                    dtReport.Rows.Add(dr);
                }

                // FileName
                string strFileName = "Meal_Order_s" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
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