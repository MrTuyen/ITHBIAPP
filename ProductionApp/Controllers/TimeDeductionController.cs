using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;
using System.Net;
using System.Net.Mail;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data;
using System.Linq.Dynamic;
using ProductionApp.Helpers;
using System.Globalization;
using System.Data.Entity;
using OfficeOpenXml.Style;

namespace ProductionApp.Controllers {
    public class TimeDeductionController:BaseController {
        //
        // GET: /TimeDeduction/
        ProductionAppEntities db = new ProductionAppEntities();
        public ActionResult Index() {
            if((UserModels)Session["SignedInUser"] == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("PR_Request");
        }
        public ActionResult PR_Request() {
            if(userLogin != null) {
                //if(userLogin.Username.ToLower() == "admin") {
                //    ViewBag.list = db.TD_TimeDeduction_Request.OrderByDescending(s => s.ID);
                //    ViewBag.per = 3;
                //} else {
                //    var hr_team = db.TBL_SYSTEM.Where(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB").ToList();

                //    if(hr_team.Count > 0) {

                //        ViewBag.list = db.TD_TimeDeduction_Request.Where(s => s.Status > 0).OrderByDescending(s => s.ID);
                //        ViewBag.per = 2;
                //    } else {
                ViewBag.list = db.TD_TimeDeduction_Request.Where(s => (s.HRProcessMail.ToLower() == userLogin.Email.ToLower() || s.HRCBSupMail.ToLower() == userLogin.Email.ToLower() || s.LMsMail.ToLower() == userLogin.Email.ToLower() || s.TBL_USERS_MST.EMAIL.ToLower() == userLogin.Email.ToLower())).OrderByDescending(s => s.ID).Take(100);
                ViewBag.per = 1;
                //    }
                //}

            } else {
                ViewBag.list = "";
            }
            return View();
        }
        public ActionResult TimeDeduction() {


            var user1 = db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
            ViewData["user"] = user1;
            var dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == user1.DEPT);
            ViewData["dept"] = dept;
            ViewData["app"] = db.OL_User_Approver.SingleOrDefault(s => s.UserCD == user1.USERNAME) ?? new OL_User_Approver();
            ViewBag.hrsup = db.TDS_UserApprover.Where(s => s.Permission == "HRCB_Sup").ToList();
            ViewBag.hr = db.TDS_UserApprover.Where(s => s.Permission == "HR_CB").ToList();
            return View();

        }
        public ActionResult Edit_TimeDeduction(int id) {
            try {
                var request = db.TD_TimeDeduction_Request.Find(id);
                //  ViewData["dept"] = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == request.DeptID);
                ViewData["request"] = db.TD_TimeDeduction_Request.Single(s => s.ID == id);
                ViewData["item"] = db.TD_TimeDeduction_Items.Where(s => s.RequestID == id).ToList();
                ViewData["manager"] = db.OL_User_Approver.First(s => s.UserCD == request.RequestBy);
                ViewData["hrteam"] = db.TDS_UserApprover.Single(s => s.Permission == "HR_CB");
                ViewData["hrsup"] = db.TDS_UserApprover.Single(s => s.Permission == "HRCB_Sup");
            } catch(Exception e) {
                ViewBag.mss = "need contact to IT";
                Utilities.WriteLogException(e ,ViewBag.Status);
                Session["Uploaditems"] = "NONE";
            }
            return View();
        }
        [HttpPost]
        public ActionResult Upload_TimeDeduction(string approver ,
           string approverMail ,string nameHR ,string mailHR ,string CBSup ,string CBSupMail) {

            if(Request != null) {
                var detail = new Models.TD_TimeDeduction_Request();
                int MesRow = 0;
                try {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                        string nameAndLocation = @"~\log\Upload\" + userLogin.Username + "-TimeDeduction-" + DateTime.Now.Ticks + "-" + Path.GetFileName(file.FileName);
                        file.SaveAs(Server.MapPath(nameAndLocation));
                        var mss = "";
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                        using(var package = new ExcelPackage(file.InputStream)) {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            if(workSheet.Cells[3 ,1].Value != null) {

                                detail.LMs = approver;
                                detail.LMsMail = approverMail;
                                detail.HRProcess = nameHR;
                                detail.HRProcessMail = mailHR;
                                detail.HRCBSup = CBSup;
                                detail.HRCBSupMail = CBSupMail;
                                detail.RequestBy = userLogin.Username;
                                detail.Status = 1;
                                detail.RequestDate = Utilities.GetDate_VietNam(DateTime.Now);
                                db.TD_TimeDeduction_Request.Add(detail);
                                db.SaveChanges();
                                var oneLeave = new List<TD_TimeDeduction_Items>();
                                //string mss1 = "", mss2 = "", mss3 = "", mss4 = "";
                                var valid = true;
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null)
                                        break;
                                    MesRow = rowIterator;
                                    var empid = workSheet.Cells[rowIterator ,1].Value ?? "";
                                    var line = workSheet.Cells[rowIterator ,3].Value ?? "";
                                    var datestart = workSheet.Cells[rowIterator ,4].Value ?? "";
                                    var dateend = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var content = workSheet.Cells[rowIterator ,6].Value ?? "";
                                    var total = workSheet.Cells[rowIterator ,7].Value ?? 0;
                                    var deductot = workSheet.Cells[rowIterator ,8].Value ?? 0;
                                    empid = Utilities.ValidEmpID(empid.ToString().Trim());
                                    var user = db.OL_User_Approver.SingleOrDefault(a => a.EmpID == empid.ToString());
                                    if(user == null) {
                                        valid = false;
                                        mss += mss == "" ? "Mã nhân viên không đúng: " + empid : ", " + empid;

                                    } else {
                                        var item1 = new TD_TimeDeduction_Items {
                                            RequestID = detail.ID ,
                                            NAME = user.EmpName ,
                                            Line = line.ToString().Trim() ,
                                            DateStart = DateTime.Parse(datestart.ToString().Trim()) ,
                                            DateEnd = DateTime.Parse(dateend.ToString().Trim()) ,
                                            Content = content.ToString().Trim() ,
                                            Total = double.Parse(total.ToString().Trim()) ,
                                            DeductOT = int.Parse(deductot.ToString().Trim()) ,
                                            Employee_ID = empid.ToString().Trim()
                                        };
                                        oneLeave.Add(item1);
                                    }

                                }

                                if(valid) {


                                    db.TD_TimeDeduction_Items.AddRange(oneLeave);
                                    db.SaveChanges();
                                    mss = "Thành công/Success.";
                                    Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + detail.ID + "/Deduction request No#" + detail.ID ,userLogin.Email ,detail.LMsMail ,userLogin.Email ,
                                        "Dear " + detail.LMs + ",<br/><br/>Vui lòng phê duyệt phiếu Đăng ký tách giờ làm việc #" + detail.ID + ".<br/>" +
                                        " <span style='color:#0070c0;font-style: italic;'>Please approve or reject working hour deduction request#" + detail.ID + ".</span> ");
                                }
                            }
                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    }
                } catch(Exception e) {
                    if(detail.ID > 0) {
                        db = new MyContext();
                        db.TD_TimeDeduction_Request.Remove(db.TD_TimeDeduction_Request.Find(detail.ID));
                        db.SaveChanges();
                    }
                    TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ", Row " + Convert.ToString(MesRow) + "')</script>";
                    Utilities.WriteLogException(e ,"TimeDeduction/AddRequest");
                }
            }
            return RedirectToAction("PR_Request");
        }

        public ActionResult Upload_TimeDeduction2(int id) {


            if(Request != null && db.TD_TimeDeduction_Request.SingleOrDefault(a => a.HRProcessMail.ToLower() == userLogin.Email.ToLower() && a.ID == id) != null) {
                int MesRow = 0;
                try {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {

                        var mss = "";
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                        using(var package = new ExcelPackage(file.InputStream)) {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            if(workSheet.Cells[3 ,1].Value != null) {
                                var oneLeave = new List<TD_TimeDeduction_Items>();
                                //db.Database.ExecuteSqlCommand(" DELETE TD_TimeDeduction_Request where ID=" + id);
                                db.TD_TimeDeduction_Items.RemoveRange(db.TD_TimeDeduction_Items.Where(a => a.RequestID == id));
                                var valid = true;
                                for(var rowIterator = 2; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null)
                                        break;
                                    MesRow = rowIterator;

                                    var empid = workSheet.Cells[rowIterator ,1].Value ?? "";
                                    var name = workSheet.Cells[rowIterator ,2].Value ?? "";
                                    var line = workSheet.Cells[rowIterator ,3].Value ?? "";
                                    var datestart = workSheet.Cells[rowIterator ,4].Value;
                                    var dateend = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var content = workSheet.Cells[rowIterator ,6].Value ?? "";
                                    var total = workSheet.Cells[rowIterator ,7].Value ?? 0;
                                    var deductot = workSheet.Cells[rowIterator ,8].Value ?? "";

                                    empid = Utilities.ValidEmpID(empid.ToString());
                                    var user = db.OL_User_Approver.SingleOrDefault(a => a.EmpID == empid.ToString());

                                    if(user == null) {
                                        valid = false;
                                        mss += mss == "" ? "Mã nhân viên không đúng: " + empid : ", " + empid;

                                    } else {


                                        var item1 = new TD_TimeDeduction_Items {
                                            RequestID = id ,
                                            NAME = name.ToString().Trim() ,
                                            Line = line.ToString().Trim() ,
                                            DateStart = DateTime.FromOADate(long.Parse(datestart.ToString().Trim())) ,
                                            DateEnd = DateTime.FromOADate(long.Parse(dateend.ToString().Trim())) ,
                                            Content = content.ToString().Trim() ,
                                            Total = double.Parse(total.ToString()) ,
                                            DeductOT = int.Parse(deductot.ToString().Trim()) ,
                                            Employee_ID = empid.ToString().Trim()
                                        };
                                        oneLeave.Add(item1);
                                    }
                                }
                                if(valid) {
                                    db.TD_TimeDeduction_Items.AddRange(oneLeave);
                                    db.SaveChanges();
                                    mss = "Thành công/Success.";
                                }
                                TempData["msg"] = "<script> alert('" + mss + "')</script>";
                                return RedirectToAction("Edit_TimeDeduction" ,new { id });
                            }

                        }
                    }
                } catch(Exception e) {

                    TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ", Row " + Convert.ToString(MesRow) + "')</script>";
                    Utilities.WriteLogException(e ,"TimeDeduction/ Re Upload_TimeDeduction2");
                }
            }
            return RedirectToAction("PR_Request");
        }
        [HttpPost]
        public ActionResult Approve(int id ,string dept) {
            try {


                var request = db.TD_TimeDeduction_Request.Find(id);
                //if(request.LMsMail.ToLower() == userLogin.Email.ToLower() && request.Status == 1) {
                if(userLogin != null && request.LMsMail.ToLower() == userLogin.Email.ToLower() && request.Status == 1) {
                    try {
                        request.Status = 2;
                        request.LMsDate = Utilities.GetDate_VietNam(DateTime.Now);
                        db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + "/Deduction request No#" + request.ID ,userLogin.Email ,request.HRProcessMail ,request.TBL_USERS_MST.EMAIL ,

                            "Dear C&B Team,<br/><br/>Yêu cầu khấu trừ giờ làm đã được phê duyệt bạn hãy xử lý tiếp.<br/>" +
                            "<span style='color:#0070c0;font-style: italic;'>Request #" + request.ID + " has been approved. Please process this request.</span>");
                        TempData["msg"] = "<script>alert('Thành công');</script>";

                    } catch {
                        TempData["msg"] = "<script>alert('Phê duyệt thất bại');</script>";
                    }
                } else {
                    TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
                }
            } catch(Exception e) {
                TempData["msg"] = "<script>alert('Error, need contact to IT.)</script>";
                Utilities.WriteLogException(e ,"TimeDeduction/AddRequest");
            }
            return RedirectToAction("PR_Request");
        }

        [HttpPost]

        public ActionResult HRteam_Approve(int id) {
            var request = db.TD_TimeDeduction_Request.Find(id);
            // if(userLogin != null && userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 2) {
            if(userLogin != null && userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 2) {
                request.Status = 3;
                request.HRProcessDate = Utilities.GetDate_VietNam(DateTime.Now);
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + "/Deduction request No#" + request.ID ,userLogin.Email ,request.HRCBSupMail ,"" ,
                    "Dear " + request.HRCBSup + ",<br/><br/>Yêu cầu khấu trừ giờ làm đã được phê duyệt bạn hãy xử lý tiếp.<br/>" +
                    "<span style='color:#0070c0;font-style: italic;'>Request #" + request.ID + " has been approved. Please process this request.</span>");
                TempData["msg"] = "<script>alert('Thành công');</script>";

            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");
        }
        [HttpPost]

        public ActionResult HRteam_Process(int id) {
            var request = db.TD_TimeDeduction_Request.Find(id);
            // if(userLogin != null && userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 2) {
            if(userLogin != null && userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && (request.Status == 4.5 || request.Status == 4)) {
                request.Status = 5;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                request.HRProcessDate = Utilities.GetDate_VietNam(DateTime.Now);
                db.SaveChanges();
                Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + "/Deduction request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,
                    "Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Yêu cầu khấu trừ giờ làm đã hoàn thành.<br/>" +
                    "<span style='color:#0070c0;font-style: italic;'>Request #" + request.ID + " has been finished,</span>");
                TempData["msg"] = "<script>alert('Thành công');</script>";

            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");
        }

        [HttpPost]
        //hr sup Approve
        public ActionResult HrSupApprove(int id ,string body) {

            var request = db.TD_TimeDeduction_Request.Find(id);
            //if(userLogin != null && userLogin.Email.ToLower() == request.HRCBSupMail.ToLower() && request.Status == 3) {
            if(userLogin != null && userLogin.Email.ToLower() == request.HRCBSupMail.ToLower() && request.Status == 3) {

                request.Status = 4;
                request.ReasonReject = body;
                request.HRCBSupDate = Utilities.GetDate_VietNam(DateTime.Now);
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('thành công/Success');</script>";
                Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + "/Deduction request No#" + request.ID ,userLogin.Email ,request.HRProcessMail ,"" ,
                    "Dear C&B Team,<br/><br/>Yêu cầu khấu trừ giờ làm đã được phê duyệt bạn hãy xử lý tiếp.<br/>" +
                    "<span style='color:#0070c0;font-style: italic;'>Request #" + request.ID + " has been approved. Please process this request.</span>");
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");

        }
        [HttpPost]
        //hr sup Approve
        public ActionResult HrSupChecking(int id ,string body) {

            var request = db.TD_TimeDeduction_Request.Find(id);
            //if(userLogin != null && userLogin.Email.ToLower() == request.HRCBSupMail.ToLower() && request.Status == 3) {
            if(userLogin != null && userLogin.Email.ToLower() == request.HRCBSupMail.ToLower() && request.Status == 3) {

                request.Status = 4.5;
                request.ReasonReject = body;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('thành công/Success');</script>";
                Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + "/Deduction request No#" + request.ID ,userLogin.Email ,request.HRProcessMail ,"" ,
                    "Dear C&B Team,<br/><br/>Yêu cầu khấu trừ giờ làm đã được phê duyệt bạn hãy xử lý tiếp.<br/>" +
                    "<span style='color:#0070c0;font-style: italic;'>Request #" + request.ID + " has been approved. Please process this request.</span><br/><br/>" + body);
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");

        }
        [HttpPost]
        public ActionResult LMReject(int id ,string body) {

            //  UserModels user = (UserModels)Session["SignedInUser"];
            var request = db.TD_TimeDeduction_Request.Find(id);
            //if(userLogin != null && userLogin.Email.ToLower() == request.LMsMail.ToLower() && request.Status == 1) {
            if(userLogin != null && userLogin.Email.ToLower() == request.LMsMail.ToLower() && request.Status == 1) {

                request.Status = -2;
                request.ReasonReject = body;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Từ chối thành công');</script>";
                Helpers.Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + " đã bị từ chối/ Time deduction request has been rejected" ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Yêu cầu bị từ chối.<br/><span style='color:#0070c0;font-style: italic;'>Request has been rejected.</span><br/><br/>" + body);
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");

        }
        [HttpPost]
        public ActionResult HrTeamReject(int id ,string body) {

            var request = db.TD_TimeDeduction_Request.Find(id);
            //if(userLogin != null && userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 2) {
            if(userLogin != null && userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 2) {

                request.Status = -3;
                request.ReasonReject = body;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Từ chối thành công');</script>";
                Utilities.SendEmail("Phiếu Đăng ký tách giờ làm việc #" + request.ID + " đã bị từ chối/ Time deduction request has been rejected" ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Yêu cầu bị từ chối.<br/><span style='color:#0070c0;font-style: italic;'>Request has been rejected.</span><br/><br/>" + body);
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");

        }

        public ActionResult ExportForEdit(int id) {
            var employeeList1 = (from a in db.TD_TimeDeduction_Request
                                 join b in db.TD_TimeDeduction_Items on a.ID equals b.RequestID
                                 where a.ID == id
                                 select new {
                                     EmployeeID = b.Employee_ID ,
                                     FullName = b.NAME ,
                                     Line = b.Line ,
                                     DateStart = b.DateStart ,
                                     DateEnd = b.DateEnd ,
                                     Reason = b.Content ,
                                     Total = b.Total ,
                                     DeductOT = b.DeductOT ,
                                 }).ToList();


            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("TimeDeduction");
            workSheet.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells["A1:H1"].Style.Font.Bold = true;
            workSheet.Cells["A1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A1:H1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Height = 30;


            var i = 0;
            workSheet.Cells[1 ,++i].Value = "Mã NV";
            workSheet.Cells[1 ,++i].Value = "Họ và tên";
            workSheet.Cells[1 ,++i].Value = "Tổ";
            workSheet.Cells[1 ,++i].Value = "Từ ngày";
            workSheet.Column(i).Style.Numberformat.Format = "MM/dd/yyyy";
            workSheet.Cells[1 ,++i].Value = "Đến ngày";
            workSheet.Column(i).Style.Numberformat.Format = "MM/dd/yyyy";
            workSheet.Cells[1 ,++i].Value = "Công việc hỗ trợ";
            workSheet.Cells[1 ,++i].Value = "Tổng số giờ trừ";
            workSheet.Cells[1 ,++i].Value = "Trừ Overtime";


            workSheet.Cells[2 ,1].LoadFromCollection(employeeList1 ,false);
            using(var col = workSheet.Cells[1 ,1 ,employeeList1.Count() + 1 ,i]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=HR-TimeDeduction-No#" + id + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }


            return RedirectToAction("PR_Request");

        }
        public ActionResult ExportForSupportDeduction(int RequestID) {
            var employeeList1 = (from a in db.TD_TimeDeduction_Request
                                 join b in db.TD_TimeDeduction_Items on a.ID equals b.RequestID
                                 where a.ID == RequestID
                                 select new {
                                     RequestID = a.ID ,
                                     EmployeeID = b.Employee_ID ,
                                     FullName = b.NAME ,
                                     Line = b.Line ,
                                     DateStart = b.DateStart ,
                                     DateEnd = b.DateEnd ,
                                     Reason = b.Content ,
                                     Total = b.Total ,
                                     DeductOT = b.DeductOT ,
                                     SupportHours = 0 ,
                                     SupportEff = "0%" ,

                                 }).ToList();

            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("SupportDeduction");

            var c = 0;
            var r = 2;
            var fc = c + 1;
            workSheet.Cells[1 ,1].Value = "Mẫu đăng ký thời gian hỗ trợ ";
            workSheet.Cells[r ,++c].Value = "Trừ giờ số";
            workSheet.Cells[r ,++c].Value = "Mã NV";
            workSheet.Cells[r ,++c].Value = "Họ và tên";
            workSheet.Cells[r ,++c].Value = "Line";
            workSheet.Cells[r ,++c].Value = "Từ ngày";
            workSheet.Column(c).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,++c].Value = "Tới ngày";
            workSheet.Column(c).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,++c].Value = "Công việc hỗ trợ";
            workSheet.Cells[r ,++c].Value = "Tổng số giờ trừ";
            workSheet.Cells[r ,++c].Value = "Trừ Overtime";
            workSheet.Cells[r ,++c].Value = "Số giờ hỗ trợ";
            workSheet.Cells[r ,++c].Value = "Hiệu suất hỗ trợ(%)";
            workSheet.Cells[r + 1 ,1].LoadFromCollection(employeeList1 ,false);
            //header
            using(var col = workSheet.Cells[r ,fc ,r ,c]) {
                col.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                col.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                col.Style.Font.Bold = true;

            }
            workSheet.Cells[1 ,1 ,1 ,c].Merge = true;
            workSheet.Row(r).Height = 40;
            //all data
            using(var col = workSheet.Cells[r ,fc ,employeeList1.Count() + r ,c]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            using(var col = workSheet.Cells[r + 1 ,10 ,employeeList1.Count() + r ,11]) {
                col.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                col.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFFF00"));

            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=TemplateSupportDeduction#" + RequestID + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }


            return RedirectToAction("PR_Request");


        }
        public ActionResult Export(DateTime date ,DateTime date1) {

            var employeeList1 = (from a in db.TD_TimeDeduction_Request
                                 join b in db.TD_TimeDeduction_Items on a.ID equals b.RequestID
                                 where a.RequestDate >= date & a.RequestDate <= DbFunctions.AddDays(date1 ,1)
                                 select new {
                                     RequestID = a.ID ,
                                     EmployeeID = b.Employee_ID ,
                                     FullName = b.NAME ,
                                     Line = b.Line ,
                                     DateStart = b.DateStart ,
                                     DateEnd = b.DateEnd ,
                                     Reason = b.Content ,
                                     Total = b.Total ,
                                     DeductOT = b.DeductOT ,
                                     SupportHours = 0 ,
                                     SupportEff = "0%" ,

                                 }).ToList();

            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("SupportDeduction");

            var c = 0;
            var r = 2;
            var fc = c + 1;
            workSheet.Cells[1 ,1].Value = "Mẫu đăng ký thời gian hỗ trợ ";
            workSheet.Cells[r ,++c].Value = "Trừ giờ số";
            workSheet.Cells[r ,++c].Value = "Mã NV";
            workSheet.Cells[r ,++c].Value = "Họ và tên";
            workSheet.Cells[r ,++c].Value = "Line";
            workSheet.Cells[r ,++c].Value = "Từ ngày";
            workSheet.Column(c).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,++c].Value = "Tới ngày";
            workSheet.Column(c).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,++c].Value = "Công việc hỗ trợ";
            workSheet.Cells[r ,++c].Value = "Tổng số giờ trừ";
            workSheet.Cells[r ,++c].Value = "Trừ Overtime";
            workSheet.Cells[r ,++c].Value = "Số giờ hỗ trợ";
            workSheet.Cells[r ,++c].Value = "Hiệu suất hỗ trợ(%)";
            workSheet.Cells[r + 1 ,1].LoadFromCollection(employeeList1 ,false);
            //header
            using(var col = workSheet.Cells[r ,fc ,r ,c]) {
                col.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                col.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                col.Style.Font.Bold = true;

            }
            workSheet.Cells[1 ,1 ,1 ,c].Merge = true;
            workSheet.Row(r).Height = 40;
            //all data
            using(var col = workSheet.Cells[r ,fc ,employeeList1.Count() + r ,c]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            using(var col = workSheet.Cells[r + 1 ,10 ,employeeList1.Count() + r ,11]) {
                col.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                col.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFFF00"));

            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=TemplateSupportDeduction#Date.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("PR_Request");


        }

    }
}