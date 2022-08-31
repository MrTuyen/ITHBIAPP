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
    public class ShiftChangeController:BaseController {
        public ActionResult Index() {
            if(userLogin == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("PR_Request");
        }
        public ActionResult PR_Request() {


            if(userLogin != null && userLogin.Username.ToLower() == "admin") {
                ViewBag.list = db.HR_CS_ShiftChange_Request.OrderByDescending(s => s.ID);
                ViewBag.per = 3;
            } else {
                var hr_team = db.TBL_SYSTEM.Where(s => s.value == userLogin.Email & s.value3 == "HR_CB").ToList();

                if(hr_team.Count > 0) {

                    ViewBag.list = db.HR_CS_ShiftChange_Request.Where(s => s.Status > 0 && s.Status < 3).OrderByDescending(s => s.ID);
                    ViewBag.per = 2;
                } else {

                    ViewBag.list = db.HR_CS_ShiftChange_Request.Where(s => (s.ManagerMail.ToLower() == userLogin.Email.ToLower() || s.TBL_USERS_MST.EMAIL.ToLower() == userLogin.Email.ToLower())).OrderByDescending(s => s.ID).Take(100);

                    ViewBag.per = 1;
                }
            }




            return View();
        }
        public ActionResult ShiftChange() {


            var user1 = db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
            ViewData["user"] = user1;
            var dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == user1.DEPT);
            ViewData["dept"] = dept;
            var emp = new OL_User_Approver();
            try {
                emp = db.OL_User_Approver.Single(s => s.UserCD == user1.USERNAME);
                ViewData["app"] = emp;
                ViewBag.user_list = db.OL_User_Approver.Where(s => s.Section == dept.DEPT_ID).ToList();
            } catch(Exception e) {
                ViewData["app"] = new OL_User_Approver();
                ViewBag.user_list = new List<OL_User_Approver>();
                TempData["msg"] = "<script>alert('Your info not exist, Please contact to HR team!');</script>";
            }


            ViewBag.hr = db.TBL_SYSTEM.Where(s => s.value3 == "HR_CB").ToList();
            return View();

        }
        public ActionResult Load_ShiftChange() {
            ViewBag.total = 0;
            List<HR_CS_ShiftChange_Items> List = null;
            if(Session["shiftchange"] != null) {
                List = (List<HR_CS_ShiftChange_Items>)Session["shiftchange"];
                return Json(new {
                    ds = List.Select(a => new {
                        Employee_ID = a.Employee_ID ,
                        NAME = a.NAME ,
                        OldShift = a.OldShift ,
                        NewShift = a.NewShift ,
                        DateStart = Utilities.GetDDMMYYYY(a.DateStart.ToString()) ,
                        DateEnd = Utilities.GetDDMMYYYY(a.DateEnd.ToString()) ,
                        Content = a.Detail
                    })
                } ,JsonRequestBehavior.AllowGet);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }

        public ActionResult Modal_Shiftchange(string employee_id ,int group_id ,string oldshift ,string fullname ,string newshift ,string newtime ,DateTime datestart ,DateTime dateend ,string content) {
            if(Session["shiftchange"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["shiftchange"] = new List<HR_CS_ShiftChange_Items>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            //  var name = db.HR_CS_ShiftChange_Mst.SingleOrDefault(s => s.ID == group_id);
            var ShiftChange = Session["ShiftChange"] as List<HR_CS_ShiftChange_Items>;
            if(!ShiftChange.Any(m => m.Employee_ID == employee_id)) {
                var newItem = new HR_CS_ShiftChange_Items() {
                    NewShift = newshift ,
                    NAME = fullname ,
                    OldShift = oldshift ,
                    Employee_ID = employee_id ,
                    DateStart = datestart ,
                    DateEnd = dateend ,
                    Detail = content ,

                };
                ShiftChange.Add(newItem);
            } else {
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit_ShiftChange(int id) {
            try {


                var request = db.HR_CS_ShiftChange_Request.Find(id);
                ViewData["dept"] = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == request.DeptID);
                ViewData["request"] = db.HR_CS_ShiftChange_Request.Single(s => s.ID == id);
                ViewData["item"] = db.HR_CS_ShiftChange_Items.Where(s => s.RequestID == id).ToList();
                ViewData["manager"] = db.OL_User_Approver.First(s => s.UserCD == request.RequestBy);
                ViewData["hrteam"] = db.TBL_SYSTEM.Single(s => s.value == request.HRProcessMail && s.value3 == "HR_CB");
                return View(request);
            } catch(Exception e) {
                ViewBag.mss = "need contact to IT";
                Utilities.WriteLogException(e ,ViewBag.Status);
                Session["Uploaditems"] = "NONE";
            }
            return View(new HR_CS_ShiftChange_Request());
        }


        [HttpPost]
        public ActionResult Upload_ShiftChange(string section ,int dept ,string approver ,
           string approverMail ,string nameHR ,string mailHR ,string note) {

            if(Request != null) {
                var detail = new Models.HR_CS_ShiftChange_Request();
                int MesRow = 0;
                try {

                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {

                        string nameAndLocation = @"~\log\Upload\" + userLogin.Username + "-ShiftChange-" + DateTime.Now.Ticks + "-" + Path.GetFileName(file.FileName);
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
                            //var hrteammail = db.TBL_SYSTEM.Single(s => s.value3 == "HR_CB").value;

                            if(workSheet.Cells[3 ,1].Value != null) {
                                var reason = workSheet.Cells[3 ,8].Value ?? "";

                                detail.DeptID = userLogin.DeptID;
                                detail.Approver = approver;
                                detail.ManagerMail = approverMail;
                                detail.RequestBy = userLogin.Username;
                                detail.HRProcess = nameHR;
                                detail.HRProcessMail = mailHR;
                                detail.Status = 1;
                                detail.RequestDate = DateTime.Now;
                                db.HR_CS_ShiftChange_Request.Add(detail);
                                db.SaveChanges();
                                var oneLeave = new List<HR_CS_ShiftChange_Items>();
                                string mss1 = "", mss2 = "", mss3 = "", mss4 = "";
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    MesRow = rowIterator;

                                    var empid = workSheet.Cells[rowIterator ,1].Value ?? "";
                                    var name = workSheet.Cells[rowIterator ,2].Value ?? "";
                                    var newshift = workSheet.Cells[rowIterator ,3].Value ?? "";
                                    var oldshift = workSheet.Cells[rowIterator ,4].Value ?? "";
                                    var datestart = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var dateend = workSheet.Cells[rowIterator ,6].Value ?? "";
                                    var content = workSheet.Cells[rowIterator ,7].Value ?? "";
                                    empid = Utilities.ValidEmpID(empid.ToString().Trim());

                                    var item1 = new HR_CS_ShiftChange_Items {
                                        RequestID = detail.ID ,
                                        NewShift = newshift.ToString().Trim() ,
                                        NAME = name.ToString().Trim() ,
                                        DateStart = DateTime.Parse(datestart.ToString().Trim()) ,
                                        DateEnd = DateTime.Parse(dateend.ToString().Trim()) ,
                                        Detail = content.ToString().Trim() ,
                                        OldShift = oldshift.ToString().Trim() ,
                                        Employee_ID = empid.ToString().Trim()
                                    };
                                    oneLeave.Add(item1);

                                }
                                db.HR_CS_ShiftChange_Items.AddRange(oneLeave);
                                db.SaveChanges();
                                mss = "Upload thành công/Success.";
                                Utilities.SendEmail("Phiếu đăng ký đổi ca #" + detail.ID + "/Shift change request need your approval" ,userLogin.Email ,detail.ManagerMail ,userLogin.Email ,"Dear " + detail.Approver + ",<br/>Vui lòng phê duyệt phiếu đăng ký đổi ca #" + detail.ID + " / Please approve or reject Shift change request#" + detail.ID + ". ");

                            }
                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    }
                } catch(Exception e) {
                    if(detail.ID > 0) {
                        db = new MyContext();
                        db.HR_CS_ShiftChange_Request.Remove(db.HR_CS_ShiftChange_Request.Find(detail.ID));
                        db.SaveChanges();
                    }
                    TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ", Row " + Convert.ToString(MesRow) + "')</script>";
                    Utilities.WriteLogException(e ,"ShiftChange/AddRequest");
                }
            }
            return RedirectToAction("PR_Request");
        }





      
        [HttpPost]
        public ActionResult Approve(int id) {

            var request = db.HR_CS_ShiftChange_Request.Find(id);
           if(request.ManagerMail.ToLower() == userLogin.Email.ToLower() &&request.Status==1) {
            //if(request.Status==1) {
                try {
                    request.Status = 2;
                    request.ManagerDate = DateTime.Now;
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Helpers.Utilities.SendEmail("Phiếu đăng ký đổi ca #" + request.ID + "/Shift change request need your process" ,userLogin.Email ,request.HRProcessMail ,request.TBL_USERS_MST.EMAIL ,"Dear " + request.HRProcess + ", <br/><br/> Bạn nhận được một yêu cầu phê duyệt đổi ca");
                    TempData["msg"] = "<script>alert('Thành công');</script>";

                } catch {
                    TempData["msg"] = "<script>alert('Phê duyệt thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }
            return RedirectToAction("PR_Request");
        }

        [HttpPost]

        public ActionResult HRteam_Approve(int id) {
            var request = db.HR_CS_ShiftChange_Request.Find(id);
            // var requestby = db.TBL_USERS_MST.Find(request.RequestBy);


            if(userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 2) {
           // if( request.Status == 2) {
               
                    request.Status = 3;
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Helpers.Utilities.SendEmail("Yêu cầu xuất phê duyệt đổi ca số #" + request.ID ,request.HRProcessMail ,request.TBL_USERS_MST.EMAIL ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu đổi ca đã được xử lý");
                    TempData["msg"] = "<script>alert('Thành công');</script>";
              
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt');</script>";
            }


            return RedirectToAction("PR_Request");
        }

        [HttpPost]
        public ActionResult ManagerReject(int id ,string body) {

            var request = db.HR_CS_ShiftChange_Request.Find(id);
            if(userLogin.Email.ToLower() == request.ManagerMail.ToLower() && request.Status == 1|| userLogin.Username.ToLower() == "admin") {
            //if(request.Status == 1) {
                request.Status = -2;
                request.ReasonReject = body;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Từ chối thành công');</script>";
                Helpers.Utilities.SendEmail("Yêu cầu phê duyệt đổi ca số #" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu của bạn đã bị từ chối: " + body);
            }

            return RedirectToAction("PR_Request");
        }
        [HttpPost]
        public ActionResult ExportRequest(DateTime date ,DateTime date1) {
            date1 = date1.AddDays(+1);
            var employeeList1 = (from a in db.HR_CS_ShiftChange_Request
                                 join b in db.HR_CS_ShiftChange_Items on a.ID equals b.RequestID
                                 join d in db.TBL_DEPARTMENT_MST on a.DeptID equals d.DEPT_ID
                                 where a.RequestDate >= date & a.RequestDate <= date1 && a.Status >= 1
                                 select new {
                                     EmployeeID = b.Employee_ID ,
                                     FullName = b.NAME ,
                                     Start = b.DateStart ,
                                     End = b.DateEnd ,
                                     NewShift = b.NewShift ,
                                     OldShift = b.OldShift ,
                                     a.RequestDate ,
                                     Detail = b.Detail ,
                                 }).ToList();


            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells["A1:H1"].Style.Font.Bold = true;
            var i = 1;
            workSheet.Cells[1 ,i++].Value = "Employee ID";
            workSheet.Cells[1 ,i++].Value = "FullName";
            workSheet.Cells[1 ,i++].Value = "Start";
            workSheet.Column(i - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[1 ,i++].Value = "End";
            workSheet.Column(i - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[1 ,i++].Value = "NewShift";
            workSheet.Cells[1 ,i++].Value = "OldShift";
            workSheet.Cells[1 ,i++].Value = "RequestDate";
            workSheet.Cells[1 ,i].Value = "Reason";

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
                Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Shiftchange-Request.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return RedirectToAction("PR_Request");

        }
        //[HttpPost]
        //public ActionResult Export2(DateTime date ,DateTime date1) {
        //    date1 = date1.AddDays(+1);
        //    var employeeList1 = (from a in db.HR_CS_ShiftChange_Request
        //                         join b in db.HR_CS_ShiftChange_Items on a.ID equals b.RequestID
        //                         join d in db.TBL_DEPARTMENT_MST on a.DeptID equals d.DEPT_ID
        //                         where a.RequestDate >= date & a.RequestDate <= date1 && a.Status == 1
        //                         select new {
        //                             EmployeeID = b.Employee_ID ,
        //                             FullName = b.NAME ,
        //                             Start = b.DateStart ,
        //                             End = b.DateEnd ,
        //                             NewShift = b.NewShift ,
        //                             OldShift = b.OldShift ,
        //                             a.RequestDate ,
        //                             Detail = b.Detail ,
        //                         }).ToList();


        //    var excel = new ExcelPackage();
        //    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
        //    workSheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //    workSheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
        //    workSheet.Cells["A1:G1"].Style.Font.Bold = true;
        //    var i = 1;
        //    workSheet.Cells[1 ,i++].Value = "Employee ID";
        //    workSheet.Cells[1 ,i++].Value = "FullName";
        //    workSheet.Cells[1 ,i++].Value = "Start";
        //    workSheet.Cells[1 ,i++].Value = "End";
        //    workSheet.Cells[1 ,i++].Value = "NewShift";
        //    workSheet.Cells[1 ,i++].Value = "OldShift";
        //    workSheet.Cells[1 ,i++].Value = "RequestDate";
        //    workSheet.Cells[1 ,i].Value = "Reason";

        //    workSheet.Cells[2 ,1].LoadFromCollection(employeeList1 ,false);
        //    using(var col = workSheet.Cells[1 ,1 ,employeeList1.Count() + 1 ,i]) {
        //        col.AutoFitColumns();
        //        col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        //        col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //    }

        //    using(var memoryStream = new MemoryStream()) {
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Shiftchange-Pending.xlsx");
        //        excel.SaveAs(memoryStream);
        //        memoryStream.WriteTo(Response.OutputStream);
        //        Response.Flush();
        //        Response.End();
        //    }

        //    return RedirectToAction("PR_Request");

        //}

    }
}