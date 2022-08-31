using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Castle.Core.Internal;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class TimeSupportDeductionController:BaseController {
        public ActionResult Index() {
            if(userLogin != null) {
                if(userLogin.Username.ToLower() == "admin") {
                    ViewBag.list = db.TDS_SupportTimeRequest.OrderByDescending(s => s.ID);
                } else {
                    ViewBag.list = db.TDS_SupportTimeRequest.Where(s => (s.SuperintendentMail.ToLower() == userLogin.Email.ToLower() || s.PayrollSupMail.ToLower() == userLogin.Email.ToLower() || s.HRCBMail.ToLower() == userLogin.Email.ToLower() || s.OpsMail.ToLower() == userLogin.Email.ToLower() || s.HRMgrMail.ToLower() == userLogin.Email.ToLower() || s.LMsMail.ToLower() == userLogin.Email.ToLower() || s.TBL_USERS_MST.EMAIL.ToLower() == userLogin.Email.ToLower())).OrderByDescending(s => s.ID).Take(100);
                }

            } else {
                ViewBag.list = "";
            }
            return View();
        }

        public JsonResult LoadLineMgr(string id) {

            //var user= db.TDS_UserApprover.SingleOrDefault(s => s.id == id) ?? new TDS_UserApprover();
            var ds= db.OL_User_Approver.SingleOrDefault(s => s.UserCD == id) ?? new OL_User_Approver();
            var dsOper= db.OL_User_Approver.SingleOrDefault(s => s.EmpEmail.ToLower() == ds.ApproverEmail.ToLower()) ?? new OL_User_Approver();
            //var eDefault = db.TBL_SYSTEM.SingleOrDefault(a => a.id == "eDefault");
            //if(eDefault != null)
            //    ds.ApproverEmail = eDefault.value;
            return Json(new { EmpName = ds.EmpName ,EmpEmail = ds.EmpEmail ,ds.ApproverName ,ds.ApproverEmail ,operName = dsOper.ApproverName ,operMail = dsOper.ApproverEmail } ,JsonRequestBehavior.AllowGet);

        }
        public ActionResult Create(string Superintendent ,string SuperintendentMail ,string approver ,string approverMail ,string opsManager ,string opsManagerMail ,string HRCB ,string HRCBMail ,string HRMgr ,string HRMgrMail ,string payroll ,string payrollMail) {

            var user1 = db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
            ViewData["user"] = user1;
            var dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == user1.DEPT);
            ViewData["dept"] = dept;
            var proSup = db.TDS_UserApprover.Where(a => a.Permission == "Superintendent").ToList();
            var id = proSup.First().id;
            ViewBag.proSup = proSup;
            ViewData["app"] = db.OL_User_Approver.SingleOrDefault(s => s.UserCD == id) ?? new OL_User_Approver();
            ViewBag.hrsup = db.TDS_UserApprover.First(s => s.Permission == "HR_CB");


            ViewBag.hrMgr = db.TDS_UserApprover.First(s => s.Permission == "HRMgr_Support");
            ViewBag.payroll = db.TDS_UserApprover.First(s => s.Permission == "Payroll_Sup");

            if(Request != null && !string.IsNullOrEmpty(SuperintendentMail)) {
                var detail = new TDS_SupportTimeRequest();
                int MesRow = 0;
                try {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                        string nameAndLocation = @"~\log\Upload\" + userLogin.Username + "-TimeSupportDeduction-" + DateTime.Now.Ticks + "-" + Path.GetFileName(file.FileName);
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
                                string mss1 = "", mss2 = "", mss3 = "", mss4 = "";
                                bool valid = true;
                                detail.Superintendent = Superintendent;
                                detail.SuperintendentMail = SuperintendentMail;
                                detail.LMs = approver;
                                detail.LMsMail = approverMail;
                                detail.Ops = opsManager;
                                detail.OpsMail = opsManagerMail;
                                detail.HRCB = HRCB;
                                detail.HRCBMail = HRCBMail;
                                detail.HRMgr = HRMgr;
                                detail.HRMgrMail = HRMgrMail;
                                detail.RequestBy = userLogin.Username;
                                detail.PayrollSup = payroll;
                                detail.PayrollSupMail = payrollMail;
                                detail.Status = 1;
                                detail.RequestDate = Utilities.GetDate_VietNam(DateTime.Now);
                                db.TDS_SupportTimeRequest.Add(detail);
                                db.SaveChanges();
                                var Items = new List<TDS_SupportTimeItems>();
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null)
                                        break;
                                    MesRow = rowIterator;
                                    int DeductionID =int.Parse(workSheet.Cells[rowIterator ,1].Value.ToString());
                                    var empid = workSheet.Cells[rowIterator ,2].Value.ToString() ?? "";
                                    var fromdate = DateTime.FromOADate(long.Parse(workSheet.Cells[rowIterator ,5].Value.ToString()));
                                    var fromdate2 = fromdate.Day;
                                    var fromdate1 = fromdate.Month;
                                    var supportHours = workSheet.Cells[rowIterator ,10].Value ?? "";
                                    var supportEff = workSheet.Cells[rowIterator ,11].Value ?? "";
                                    supportEff = float.Parse(supportEff.ToString().Replace("%" ,"").Trim()) * 100;
                                    empid = Utilities.ValidEmpID(empid.Trim());
                                    var deduct =  db.TD_TimeDeduction_Items.SingleOrDefault(a => a.Employee_ID == empid && a.TD_TimeDeduction_Request.ID == DeductionID && DbFunctions.TruncateTime(a.DateStart) == DbFunctions.TruncateTime(fromdate.Date));
                                    if(deduct == null) {
                                        mss1 = mss1 + (mss1 != "" ? ", " : "") + empid;
                                        valid = false;
                                    } else {
                                        var item1 = new TDS_SupportTimeItems {
                                            RequestID = detail.ID ,
                                            Employee_ID = empid ,
                                            NAME = deduct.NAME ,
                                            Line = deduct.Line ,
                                            DateStart = deduct.DateStart ,
                                            DateEnd = deduct.DateEnd ,
                                            Detail = deduct.Content ,
                                            DeductionID = DeductionID ,
                                            Total = deduct.Total ,
                                            DeductOT = deduct.DeductOT ,
                                            SupportHours = double.Parse(supportHours.ToString().Trim()) ,
                                            SupportEff = double.Parse(supportEff.ToString().Trim()) ,
                                            Payment = double.Parse(supportHours.ToString().Trim()) * double.Parse(supportEff.ToString().Trim()) * 6000 / 100
                                        };
                                        Items.Add(item1);
                                    }
                                }
                                if(!valid) {
                                    if(detail.ID > 0) {
                                        db = new MyContext();
                                        db.TDS_SupportTimeRequest.Remove(db.TDS_SupportTimeRequest.Find(detail.ID));
                                        db.SaveChanges();
                                    }
                                    var  ms1 = "► Nhân viên chưa được duyệt trừ giờ: " + mss1 + "<br/><br/>";
                                    TempData["msg"] = (mss1 != "" ? ms1 : "");

                                } else {
                                    db.TDS_SupportTimeItems.AddRange(Items);
                                    db.SaveChanges();
                                    Utilities.SendEmail(
                                        "Phiếu đăng ký hỗ trợ tạm thời #" + detail.ID + "/Support time request No#" +
                                        detail.ID ,userLogin.Email ,detail.SuperintendentMail ,userLogin.Email ,
                                        "Dear " + detail.Superintendent +
                                        ",<br/><br/>Vui lòng phê duyệt phiếu đăng ký hỗ trợ tạm thời số#" +
                                        detail.ID + ".<br/>" +
                                        " <span style='color:#0070c0;font-style: italic;'>Please approve or reject supporting time request#" +
                                        detail.ID + ".</span> ");
                                    TempData["msg"] = "<script> alert('Thành công/Success.')</script>";
                                    return RedirectToAction("Index");
                                }
                            }

                        }

                    }
                } catch(Exception e) {
                    if(detail.ID > 0) {
                        db = new MyContext();
                        db.TDS_SupportTimeRequest.Remove(db.TDS_SupportTimeRequest.Find(detail.ID));
                        db.SaveChanges();
                    }
                    TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ", Row " + Convert.ToString(MesRow) + "')</script>";
                    Utilities.WriteLogException(e);
                }
            }
            //else {
            //    TempData["msg"] = "<script> alert('Bạn chưa nhập đủ thông tin.')</script>";
            //}
            return View();
        }

        public ActionResult Edit(int ID ,FormCollection fr) {
            var request = db.TDS_SupportTimeRequest.SingleOrDefault(a => a.ID == ID);
            var requestGroup = request.TDS_SupportTimeItems.GroupBy(a => a.Line).Select(a => new TdsTimeSupportGroup() {
                Line = a.Key ,
                HC = a.Count() ,
                Total = a.Sum(b => b.Total) ,
                SupportHours = a.Sum(b => b.SupportHours) ,
                SupportEff = a.Sum(b => b.SupportEff) / a.Count() ,
                Payment = a.Sum(b => b.Payment) ,



            }).ToList();
            ViewBag.requestGroup = requestGroup;


            if(HttpContext.Request.HttpMethod.ToUpper() == HttpMethod.Post.Method) {
                try {
                    //string approver ,string approverMail ,string opsManager ,string opsManagerMail ,string HRCB ,string HRCBMail ,string HRMgr ,string HRMgrMail) 
                    if(request != null) {
                        if(fr["tacdong"] == "Superintendent" && request.Status == 1 && userLogin.Email.ToLower() == request.SuperintendentMail.ToLower()) {
                            // if(fr["tacdong"] == "LmApprove" && request.Status == 1) {

                            request.SuperintendentDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.Status = 2;
                            db.SaveChanges();
                            var body = "Dear " + request.LMs + ", <br/><br/> Vui lòng phê duyệt phiếu đăng ký hỗ trợ tạm thời số#" +
                                       request.ID + ".<br/>" +
                                       " <span style='color:#0070c0;font-style: italic;'>Please approve or reject supporting time request#" +
                                       request.ID + ".</span> ";
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.LMsMail ,request.TBL_USERS_MST.EMAIL + ";" + userLogin.Email ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        if(fr["tacdong"] == "supReject" && request.Status == 1 && userLogin.Email.ToLower() == request.SuperintendentMail.ToLower()) {
                            // if(fr["tacdong"] == "LMReject" && request.Status == 1) {
                            request.LMsDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.Status = -2;
                            request.ReasonReject = fr["body"];
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu không được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been Rejected.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,userLogin.Email ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }

                        if(fr["tacdong"] == "LmApprove" && request.Status == 2 && userLogin.Email.ToLower() == request.LMsMail.ToLower()) {
                            // if(fr["tacdong"] == "LmApprove" && request.Status == 1) {

                            request.LMsDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.Status = 3;
                            db.SaveChanges();
                            var body = "Dear " + request.Ops + ", <br/><br/> Vui lòng phê duyệt phiếu đăng ký hỗ trợ tạm thời số#" +
                                       request.ID + ".<br/>" +
                                       " <span style='color:#0070c0;font-style: italic;'>Please approve or reject supporting time request#" +
                                       request.ID + ".</span> ";
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.OpsMail ,request.TBL_USERS_MST.EMAIL + ";" + userLogin.Email ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }

                        if(fr["tacdong"] == "LMReject" && request.Status == 2 && userLogin.Email.ToLower() == request.LMsMail.ToLower()) {
                            // if(fr["tacdong"] == "LMReject" && request.Status == 1) {
                            request.LMsDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.Status = -3;
                            request.ReasonReject = fr["body"];
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu không được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been Rejected.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,userLogin.Email ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        //OPS

                        if(fr["tacdong"] == "OpsApprove" && request.Status == 3 && userLogin.Email.ToLower() == request.OpsMail.ToLower()) {
                            // if(fr["tacdong"] == "OpsApprove" && request.Status == 2 ) {
                            request.Status = 4;
                            request.OpsDate = Utilities.GetDate_VietNam(DateTime.Now);
                            db.SaveChanges();
                            var body = "Dear " + request.HRCB + ", <br/><br/> yêu cầu đã được phê duyệt, vui lòng xử lý. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been approved. Please process this request.</span>";
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.HRCBMail ,request.LMsMail + ";" + userLogin.Email + ";" + request.TBL_USERS_MST.EMAIL ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        if(fr["tacdong"] == "OpsReject" && request.Status == 3 && userLogin.Email.ToLower() == request.OpsMail.ToLower()) {
                            // if(fr["tacdong"] == "OpsReject" && request.Status == 2 ) {
                            request.Status = -4;
                            request.ReasonReject = fr["body"];
                            request.OpsDate = Utilities.GetDate_VietNam(DateTime.Now);
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu không được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been Rejected.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,request.LMsMail + ";" + userLogin.Email ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        //HR SUP
                        if(fr["tacdong"] == "HRCBApprove" && request.Status == 4 && userLogin.Email.ToLower() == request.HRCBMail.ToLower()) {
                            //if(fr["tacdong"] == "HRCBApprove" && request.Status == 3 ) {
                            request.Status = 5;
                            request.HRCBDate = Utilities.GetDate_VietNam(DateTime.Now);
                            db.SaveChanges();
                            var body = "Dear " + request.HRMgr + ", <br/><br/> yêu cầu đã được phê duyệt, vui lòng xử lý. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been approved. Please process this request.</span>";
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.HRMgrMail ,request.LMsMail + ";" + request.OpsMail + ";" + userLogin.Email + ";" + request.TBL_USERS_MST.EMAIL ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        if(fr["tacdong"] == "HRCBReject" && request.Status == 4 && userLogin.Email.ToLower() == request.HRCBMail.ToLower()) {
                            // if(fr["tacdong"] == "HRCBReject" && request.Status == 3 ) {
                            request.Status = -5;
                            request.HRCBDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.ReasonReject = fr["body"];
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu không được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been Rejected.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,request.LMsMail + ";" + request.OpsMail + ";" + userLogin.Email ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        //HR Manager
                        if(fr["tacdong"] == "HRMApprove" && request.Status == 5 && userLogin.Email.ToLower() == request.HRMgrMail.ToLower()) {
                            // if(fr["tacdong"] == "HRMApprove" && request.Status == 4) {
                            request.Status = 6;
                            request.HRMgrDate = Utilities.GetDate_VietNam(DateTime.Now);
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu đã được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been approved.</span>";
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,request.LMsMail + ";" + request.OpsMail + ";" + userLogin.Email + ";" + request.HRCBMail ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }

                        if(fr["tacdong"] == "HRMReject" && request.Status == 5 && userLogin.Email.ToLower() == request.HRMgrMail.ToLower()) {
                            // if(fr["tacdong"] == "HRMReject" && request.Status == 4 ) {
                            request.Status = -6;
                            request.HRMgrDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.ReasonReject = fr["body"];
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu không được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Request has been Rejected.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL + ";" + request.PayrollSupMail ,request.LMsMail + ";" + request.OpsMail + ";" + userLogin.Email + ";" + request.HRCBMail ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }

                        //PayrollSup
                        if(fr["tacdong"] == "PayrollSupApprove" && request.Status == 6 && userLogin.Email.ToLower() == request.PayrollSupMail.ToLower()) {
                            // if(fr["tacdong"] == "PayrollSupApprove" && request.Status == 5) {
                            request.Status = 7;
                            request.PayrollSupDate = Utilities.GetDate_VietNam(DateTime.Now);
                            db.SaveChanges();
                            //var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu đã được phê duyệt. <br/> " +
                            //           "<span style='color:#0070c0;font-style: italic;'>Request has been approved.</span>";
                            //Utilities.SendEmail("Phiếu đăng ký hỗ trợ tạm thời #" + request.ID + "/Support time request No#" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,request.LMsMail + ";" + request.OpsMail + ";" + userLogin.Email + ";" + request.HRCBMail ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        //  if(fr["tacdong"] == "DownloadPayroll" && request.Status >= 6 && userLogin.Email.ToLower() == request.PayrollSupMail.ToLower())
                        if(fr["tacdong"] == "DownloadPayroll" && request.Status >= 6) {

                            var data =request.TDS_SupportTimeItems.GroupBy(a => new { a.DateStart ,a.TDS_SupportTimeRequest.HRMgrDate ,a.TDS_SupportTimeRequest.TBL_USERS_MST.FULLNAME }).Select(a => new {
                                DateStart = a.Key.DateStart.Value.ToString("dd/MM/yyyy") ,
                                HC = a.Count() ,
                                SumMoney = a.Sum(b => b.Payment) ,
                                a.Key.FULLNAME ,
                                Superintendent = "Approved" ,
                                Superintendent2 = "Approved" ,
                                Superintendent3 = "Approved" ,
                                Superintendent4 = "Approved" ,
                                Superintendent5 = "Approved" ,
                                HRMgrDate = a.Key.HRMgrDate.Value.ToString("dd/MM/yyyy")
                            });


                            //var requestGroup = request.TDS_SupportTimeItems.GroupBy(a => a.Line).Select(a => new TdsTimeSupportGroup() {
                            //    Line = a.Key ,
                            //    HC = a.Count() ,
                            //    Total = a.Sum(b => b.Total) ,
                            //    SupportHours = a.Sum(b => b.SupportHours) ,
                            //    SupportEff = a.Sum(b => b.SupportEff) / a.Count() ,
                            //    Payment = a.Sum(b => b.Payment) ,



                            //}).ToList();
                            var excel = new ExcelPackage();
                            var workSheet = excel.Workbook.Worksheets.Add("payment");
                            workSheet.Row(1).Height = 30;
                            var c = 1;
                            var r = 1;
                            var fc = c;
                            workSheet.Cells[r ,c++].Value = "Ngày hỗ trợ";
                            workSheet.Column(c - 1).Style.Numberformat.Format = "d/m/yyyy";
                            workSheet.Cells[r ,c++].Value = "Số lượt đề nghị hỗ trợ";
                            workSheet.Cells[r ,c++].Value = "Tổng tiền (VND)";
                            workSheet.Cells[r ,c++].Value = "Prepared by";
                            workSheet.Cells[r ,c++].Value = "Superintendent";
                            workSheet.Cells[r ,c++].Value = "Line Manager";
                            workSheet.Cells[r ,c++].Value = "Operation Manager";
                            workSheet.Cells[r ,c++].Value = "HR C&B Team";
                            workSheet.Cells[r ,c++].Value = "Authorized by HR Manager";
                            workSheet.Cells[r ,c].Value = "Date of approval";
                            workSheet.Column(c).Style.Numberformat.Format = "d/m/yyyy";


                            workSheet.Cells[r ,fc ,r ,c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            workSheet.Cells[r ,fc ,r ,c].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                            workSheet.Cells[r ,fc ,r ,c].Style.Font.Bold = true;

                            workSheet.Cells[r + 1 ,fc].LoadFromCollection(data ,false);
                            using(var col = workSheet.Cells[r ,fc ,data.Count() + r ,c]) {
                                col.AutoFitColumns();
                                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            ExcelToPdf pdf= new ExcelToPdf();
                            pdf.CreateExcelToPdf(excel ,userLogin);
                            using(var memoryStream = new MemoryStream(pdf.SaveToClient())) {
                                Response.ContentType = "application/pdf";
                                Response.AddHeader("content-disposition" ,"attachment;  filename=Payment-TimeSupportDeduction#" + request.ID + ".pdf");
                                memoryStream.WriteTo(Response.OutputStream);
                                Response.Flush();
                                Response.End();
                            }

                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        }
                        TempData["msg"] = "<script>alert('Bạn không thể thực hiện hành động này / Access denied ');</script>";

                    }

                } catch(Exception e) {
                    Utilities.WriteLogException(e);
                    TempData["msg"] = "<script>alert('System error, Please retry or contact to IT team!');</script>";
                }
            }


            return View(request);
        }


        public ActionResult ExportSupport(DateTime date ,DateTime date1) {
            var employeeList1 = (from a in db.TDS_SupportTimeRequest
                                 join b in db.TDS_SupportTimeItems on a.ID equals b.RequestID
                                 where a.RequestDate >= date & a.RequestDate <= DbFunctions.AddDays(date1 ,1) && a.Status >= 1
                                 select new {
                                     EmployeeID = b.Employee_ID ,
                                     FullName = b.NAME ,
                                     Line = b.Line ,
                                     DateStart = b.DateStart ,
                                     DateEnd = b.DateEnd ,
                                     Reason = b.Detail ,
                                     Total = b.Total ,
                                     DeductOT = b.DeductOT ,
                                     SupportHours = b.SupportHours ,
                                     SupportEff = b.SupportEff + "%" ,
                                     Payment = b.Payment

                                 }).ToList();

            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("TimeSupportDeduction");
            workSheet.Row(1).Height = 30;
            var c = 1;
            var r = 1;
            var fc = c;
            workSheet.Cells[r ,c++].Value = "Mã NV";
            workSheet.Cells[r ,c++].Value = "Họ và tên";
            workSheet.Cells[r ,c++].Value = "Tổ";
            workSheet.Cells[r ,c++].Value = "Từ ngày";
            workSheet.Column(c - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,c++].Value = "Tới ngày";
            workSheet.Column(c - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,c++].Value = "Công việc hỗ trợ";
            workSheet.Cells[r ,c++].Value = "Tổng số giờ trừ";
            workSheet.Cells[r ,c++].Value = "Trừ Overtime";
            workSheet.Cells[r ,c++].Value = "Số giờ hỗ trợ";
            workSheet.Cells[r ,c++].Value = "Hiệu suất hỗ trợ";
            workSheet.Cells[r ,c].Value = "Thành tiền";


            workSheet.Cells[r ,fc ,r ,c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[r ,fc ,r ,c].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells[r ,fc ,r ,c].Style.Font.Bold = true;

            workSheet.Cells[r + 1 ,fc].LoadFromCollection(employeeList1 ,false);
            using(var col = workSheet.Cells[r ,fc ,employeeList1.Count() + r ,c]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=HR-TimeSupportDeduction.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Index");
        }
        public ActionResult ExportSupportByID(int RequestID) {
            var employeeList1 = (from a in db.TDS_SupportTimeRequest
                                 join b in db.TDS_SupportTimeItems on a.ID equals b.RequestID
                                 where a.ID == RequestID
                                 select new {
                                     EmployeeID = b.Employee_ID ,
                                     FullName = b.NAME ,
                                     Line = b.Line ,
                                     DateStart = b.DateStart ,
                                     DateEnd = b.DateEnd ,
                                     Reason = b.Detail ,
                                     Total = b.Total ,
                                     DeductOT = b.DeductOT ,
                                     SupportHours = b.SupportHours ,
                                     SupportEff = b.SupportEff + "%" ,
                                     Payment = b.Payment

                                 }).ToList();

            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("#" + RequestID);
            workSheet.Row(1).Height = 30;
            var c = 1;
            var r = 1;
            var fc = c;
            workSheet.Cells[r ,c++].Value = "Mã NV";
            workSheet.Cells[r ,c++].Value = "Họ và tên";
            workSheet.Cells[r ,c++].Value = "Tổ";
            workSheet.Cells[r ,c++].Value = "Từ ngày";
            workSheet.Column(c - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,c++].Value = "Tới ngày";
            workSheet.Column(c - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,c++].Value = "Công việc hỗ trợ";
            workSheet.Cells[r ,c++].Value = "Tổng số giờ trừ";
            workSheet.Cells[r ,c++].Value = "Trừ Overtime";
            workSheet.Cells[r ,c++].Value = "Số giờ hỗ trợ";
            workSheet.Cells[r ,c++].Value = "Hiệu suất hỗ trợ";
            workSheet.Cells[r ,c].Value = "Thành tiền";


            workSheet.Cells[r ,fc ,r ,c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[r ,fc ,r ,c].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells[r ,fc ,r ,c].Style.Font.Bold = true;
            workSheet.Cells[r + 1 ,fc].LoadFromCollection(employeeList1 ,false);
            using(var col = workSheet.Cells[r ,fc ,employeeList1.Count() + r ,c]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=HR-TimeSupportDeduction#" + RequestID + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Index");
        }
    }
}