using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class AbnormalController:BaseController {
        // GET: Abnormal
        public ActionResult Index(FormCollection fr) {
            try {


                var mesRow = 0;
                if(fr["control"] == "upload") {
                    try {
                        var file = Request.Files["UploadedFile"];
                        if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                            var fileName = file.FileName;
                            var fileContentType = file.ContentType;
                            var fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                            using(var package = new ExcelPackage(file.InputStream)) {
                                db.Database.ExecuteSqlCommand("Delete from HR_ABN_Mst");
                                var currentSheet = package.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;
                                var ls= new List<HR_ABN_Mst>();
                                for(var rowIterator = 4; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null ||(string) workSheet.Cells[rowIterator ,1].Value == "")
                                        break;
                                    mesRow = rowIterator;

                                    var division = workSheet.Cells[rowIterator ,1].Value;
                                    var deptID = workSheet.Cells[rowIterator ,2].Value;
                                    var department = workSheet.Cells[rowIterator ,3].Value;
                                    var shift = workSheet.Cells[rowIterator ,4].Value;
                                    var line = workSheet.Cells[rowIterator ,5].Value;
                                    var empId = workSheet.Cells[rowIterator ,6].Value;
                                    var name = workSheet.Cells[rowIterator ,7].Value;
                                    var operation = workSheet.Cells[rowIterator ,8].Value;
                                    var shiftStart = workSheet.Cells[rowIterator ,9].Value;
                                    var actualStart = workSheet.Cells[rowIterator ,10].Value;
                                    var shiftEnd = workSheet.Cells[rowIterator ,11].Value;
                                    var actualEnd = workSheet.Cells[rowIterator ,12].Value;
                                    var late = workSheet.Cells[rowIterator ,13].Value.ToString().Trim();
                                    var soon = workSheet.Cells[rowIterator ,14].Value.ToString().Trim();
                                    var abnormal = workSheet.Cells[rowIterator ,15].Value;
                                    var uploaddate = workSheet.Cells[rowIterator ,19].Value;
                                    var MailAdmin = workSheet.Cells[rowIterator ,20].Value;
                                    empId = Utilities.ValidEmpID((string)empId);
                                    ls.Add(new HR_ABN_Mst() {
                                        Division = division == null ? "" : division.ToString().Trim() ,
                                        DeptID = deptID == null ? 0 : int.Parse(deptID.ToString().Trim()) ,
                                        Department = department == null ? "" : department.ToString().Trim() ,
                                        Shift = shift == null ? "" : shift.ToString().Trim() ,
                                        Line = line == null ? "" : line.ToString().Trim() ,
                                        EmpID = empId == null ? "" : empId.ToString().Trim() ,
                                        Name = name == null ? "" : name.ToString().Trim() ,
                                        Operation = operation == null ? "" : operation.ToString().Trim() ,
                                        ShiftStart = shiftStart == null ? "" : shiftStart.ToString().Trim() ,
                                        ActualStart = actualStart == null ? "" : actualStart.ToString().Trim() ,
                                        ShiftEnd = shiftEnd == null ? "" : shiftEnd.ToString().Trim() ,
                                        ActualEnd = actualEnd == null ? "" : actualEnd.ToString().Trim() ,
                                        Late = late.Length > 4 ? late.Substring(0 ,4) : late ,
                                        Soon = soon.Length > 4 ? soon.Substring(0 ,4) : soon ,
                                        Abnormal = abnormal == null ? "" : abnormal.ToString().Trim() ,
                                        MailAdmin = MailAdmin == null ? "" : MailAdmin.ToString().Trim() ,
                                        uploaddate = Convert.ToDateTime(uploaddate) ,

                                    });


                                }

                                db.HR_ABN_Mst.AddRange(ls);
                                db.SaveChanges();
                                ViewBag.mss = "Success!";
                            }
                        } else {
                            ViewBag.mss = "Fail, Bạn hãy chọn ngày ";

                        }
                    } catch(Exception e) {
                        ViewBag.mss = "need contact to IT. " + e.Message + ",  row " + Convert.ToString(mesRow);
                        Utilities.WriteLogException(e ,ViewBag.Status);
                        Session["Uploaditems"] = "NONE";
                    }
                } else if(fr["control"] == "download") {
                    var ffromDate = fr["txtFromDate"];
                    var ftoDate = fr["txtToDate"];
                    if(!string.IsNullOrEmpty(ffromDate) && !string.IsNullOrEmpty(ftoDate)) {
                        var excel = new ExcelPackage();
                        var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                        var fromDate =Utilities.GetDate_VietNam(Convert.ToDateTime(ffromDate));
                        var toDate = Utilities.GetDate_VietNam(Convert.ToDateTime(ftoDate + " 23:59:59"));
                        var data = db.HR_ABN_Items.Where(a => a.HR_ABN_Request.Status >= 1 && a.HR_ABN_Request.CreateDate >= fromDate && a.HR_ABN_Request.CreateDate <= toDate)
                            .Select(a => new {
                                a.Division ,
                                a.TBL_DEPARTMENT_MST.NAME ,
                                a.Shift ,
                                a.Line ,
                                a.EmpID ,
                                a.Name ,
                                a.Operation ,
                                a.ShiftStart ,
                                a.ActualStart ,
                                a.ShiftEnd ,
                                a.ActualEnd ,
                                a.Late ,
                                a.Soon ,
                                a.Abnormal ,
                                a.RequestChangeStart ,
                                a.RequestChangeEnd ,
                                a.Reason ,
                                a.uploaddate
                            })
                            .ToList();


                        workSheet.Cells[2 ,1].Value = "Xưởng /Division";
                        workSheet.Cells["A2:A3"].Merge = true;
                        workSheet.Cells[2 ,2].Value = "Phòng /Department";
                        workSheet.Cells["B2:B3"].Merge = true;
                        workSheet.Cells[2 ,3].Value = "Ca làm việc /Shift";
                        workSheet.Cells["C2:C3"].Merge = true;
                        workSheet.Cells[2 ,4].Value = "Tổ/ Bộ phận /Line";
                        workSheet.Cells["D2:D3"].Merge = true;
                        workSheet.Cells[2 ,5].Value = "Mã NV /ID";
                        workSheet.Cells["E2:E3"].Merge = true;
                        workSheet.Cells[2 ,6].Value = "Họ và tên /Name";
                        workSheet.Cells["F2:F3"].Merge = true;
                        workSheet.Cells[2 ,7].Value = "Vị trí/Công đoạn /Operation";
                        workSheet.Cells["G2:G3"].Merge = true;
                        workSheet.Cells[2 ,8].Value = "Giờ bắt đầu ca làm việc /Shift start";
                        workSheet.Cells["H2:H3"].Merge = true;
                        workSheet.Cells[2 ,9].Value = "Giờ bắt đầu ca làm việc thực tế /Actual start";
                        workSheet.Cells["I2:I3"].Merge = true;
                        workSheet.Cells[2 ,10].Value = "Giờ kết thúc ca làm việc /Shift end";
                        workSheet.Cells["J2:J3"].Merge = true;
                        workSheet.Cells[2 ,11].Value = "Giờ kết thúc thực tế /Actual end";
                        workSheet.Cells["K2:K3"].Merge = true;
                        workSheet.Cells[2 ,12].Value = "Đi muộn /Late";
                        workSheet.Cells["L2:L3"].Merge = true;
                        workSheet.Cells[2 ,13].Value = "Về sớm /Soon";
                        workSheet.Cells["M2:M3"].Merge = true;
                        workSheet.Cells[2 ,14].Value = "Bất thường /Abnormal";
                        workSheet.Cells["N2:N3"].Merge = true;
                        workSheet.Cells[2 ,15].Value = "Đề nghị được sửa /Request to edit";
                        workSheet.Cells["O2:P2"].Merge = true;
                        workSheet.Cells[2 ,17].Value = "Lý do /Reason";
                        workSheet.Cells["Q2:Q3"].Merge = true;
                        workSheet.Cells[2 ,18].Value = "Ngày bất thường /Abnormal date";
                        workSheet.Column(18).Style.Numberformat.Format = "m/d/yyyy";
                        workSheet.Cells["R2:R3"].Merge = true;

                        workSheet.Cells[3 ,15].Value = "Giờ bắt đầu /Start";
                        workSheet.Cells[3 ,16].Value = "Giờ kết thúc /End";
                        workSheet.Column(5).Style.Numberformat.Format = "@";
                        workSheet.Column(8).Style.Numberformat.Format = "@";
                        workSheet.Column(9).Style.Numberformat.Format = "@";
                        workSheet.Column(10).Style.Numberformat.Format = "@";
                        workSheet.Column(11).Style.Numberformat.Format = "@";
                        workSheet.Column(15).Style.Numberformat.Format = "@";
                        workSheet.Column(16).Style.Numberformat.Format = "@";

                        workSheet.Cells[4 ,1].LoadFromCollection(data ,false);
                        using(var col = workSheet.Cells[1 ,1 ,data.Count() + 3 ,18]) {
                            col.AutoFitColumns();
                            col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }
                        workSheet.Row(2).Style.WrapText = true;
                        workSheet.Cells["A2:R2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells["A2:R2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                        workSheet.Cells["A2:R2"].Style.Font.Bold = true;
                        workSheet.Cells["A2:R2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells["A2:R2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        workSheet.Cells["A3:R3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells["A3:R3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                        workSheet.Cells["A3:R3"].Style.Font.Bold = true;
                        workSheet.Cells["A3:R3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells["A3:R3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        using(var memoryStream = new MemoryStream()) {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Abnormal-pending.xlsx");
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    } else {
                        ViewBag.mss = "Fail, Bạn hãy chọn ngày ";

                    }
                }
                var listABN_Mst= new List<HR_ABN_Mst>();
                if(userLogin.Username.ToLower() == "admin") {
                    ViewBag.list = db.HR_ABN_Request.OrderByDescending(s => s.ID).ToList();
                    listABN_Mst = db.HR_ABN_Mst.OrderByDescending(s => s.Department).ToList();
                    ViewBag.per = 3;
                } else {
                    var hr_team = db.TBL_SYSTEM.Where(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB");
                    if(hr_team.Any()) {
                        ViewBag.list = db.HR_ABN_Request.Where(s => s.Status > 0 && s.Status < 3).OrderByDescending(s => s.ID).ToList();
                        ViewBag.per = 2;
                        listABN_Mst = db.HR_ABN_Mst.OrderByDescending(s => s.Department).ToList();
                    } else {
                        ViewBag.list = db.HR_ABN_Request.Where(s => s.ApproveMail.ToLower() == userLogin.Email.ToLower() || s.RequestBy == userLogin.Username).OrderByDescending(s => s.ID).Take(100).ToList();
                        ViewBag.per = 1;
                        listABN_Mst = db.HR_ABN_Mst.Where(s => s.MailAdmin.ToLower().Contains(userLogin.Email.ToLower()) || s.DeptID == userLogin.DeptID).OrderByDescending(s => s.EmpID).ToList();
                    }
                }
                return View(listABN_Mst);

            } catch(Exception e) {
                Utilities.WriteLogException(e ,"abn/Index");
                TempData["msg"] = "<script>alert('System error, Please retry or contact to IT team!');</script>";
            }
            return View();

        }



        public JsonResult SendMail() {

            var lsMail = db.HR_ABN_Mst.Select(a => a.MailAdmin).Distinct();
            var mailTo = "";
            foreach(var item in lsMail) {
                mailTo += (mailTo == "" ? "" : ";") + item;
            }
            if(mailTo != "") {
                var body = "Hi all, <br/>" +
                           "Bộ phận vui lòng hoàn thành xác nhận bất thường,<br/>" +
                           "<span style='color:#0070c0;font-style: italic;'>Department please complete abnormal confirmation,</span><br/><br/>" +
                           "Xác nhận hợp lệ phải được quản lý Approve trước 15h. <br/>" +
                           "<span style='color:#0070c0;font-style: italic;'>Validation must be manager approve before 15h.</span><br/><br/>";
                Utilities.SendEmail("Báo cáo bất thường NGÀY #" + Utilities.GetDDMMYYYY(DateTime.Today.ToString()) ,userLogin.Email ,mailTo ,userLogin.Email ,body);
            } else {
                return Json("Fail, Không có yêu cầu cần xác nhận" ,JsonRequestBehavior.AllowGet);

            }
            return Json("Success" ,JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create(FormCollection fr) {
            try {


                var dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == userLogin.DeptID);
                ViewBag.user = userLogin;
                ViewBag.dept = dept;
                var app = db.OL_User_Approver.SingleOrDefault(s => s.UserCD == userLogin.Username);
                ViewBag.app = app;
                var hr = db.TBL_SYSTEM.SingleOrDefault(s => s.value3 == "HR_CB");
                ViewBag.hr = hr;
                //var lsItem = Session["ListAbnItem"] as List<HR_ABN_Items> ?? new List<HR_ABN_Items>();
                if(HttpContext.Request.HttpMethod.ToUpper() == HttpMethod.Post.Method && fr.AllKeys.Contains("hanhdong") && fr["hanhdong"].Contains("Upload")) {

                    var mesRow = 0;
                    try {
                        var file = Request.Files["UploadedFile"];
                        if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                            Session["ListAbnItem"] = new List<HR_ABN_Items>();
                            var lsItem = Session["ListAbnItem"] as List<HR_ABN_Items>;

                            var fileName = file.FileName;
                            var fileContentType = file.ContentType;
                            var fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                            using(var package = new ExcelPackage(file.InputStream)) {
                                var currentSheet = package.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;
                                for(var rowIterator = 4; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    mesRow = rowIterator;
                                    var division = workSheet.Cells[rowIterator ,1].Value;
                                    var shift = workSheet.Cells[rowIterator ,3].Value;
                                    var line = workSheet.Cells[rowIterator ,4].Value;
                                    var empId = workSheet.Cells[rowIterator ,5].Value;
                                    var name = workSheet.Cells[rowIterator ,6].Value;
                                    var operation = workSheet.Cells[rowIterator ,7].Value;
                                    var shiftStart = workSheet.Cells[rowIterator ,8].Value;
                                    var actualStart = workSheet.Cells[rowIterator ,9].Value;
                                    var shiftEnd = workSheet.Cells[rowIterator ,10].Value;
                                    var actualEnd = workSheet.Cells[rowIterator ,11].Value;
                                    var late = workSheet.Cells[rowIterator ,12].Value.ToString().Trim();
                                    var soon = workSheet.Cells[rowIterator ,13].Value.ToString().Trim();
                                    var abnormal = workSheet.Cells[rowIterator ,14].Value;
                                    var RequestChangeStart = workSheet.Cells[rowIterator ,15].Value ?? "";
                                    var RequestChangeEnd = workSheet.Cells[rowIterator ,16].Value ?? "";
                                    var Reason = workSheet.Cells[rowIterator ,17].Value ?? "";
                                    var abnormalDate =DateTime.Parse(workSheet.Cells[rowIterator ,18].Value.ToString().Trim());

                                    empId = Utilities.ValidEmpID((string)empId);
                                    var abnMst = db.HR_ABN_Mst.FirstOrDefault(a => a.EmpID == (string)empId && a.uploaddate == abnormalDate);
                                    if(abnMst != null && !lsItem.Any(a => a.EmpID == empId && a.uploaddate == abnormalDate)) {
                                        lsItem.Add(new HR_ABN_Items() {
                                            ID = abnMst.ID ,
                                            Division = abnMst.Division ,
                                            DeptID = abnMst.DeptID ,
                                            Shift = abnMst.Shift ,
                                            Line = abnMst.Line ,
                                            EmpID = abnMst.EmpID ,
                                            Name = abnMst.Name ,
                                            Operation = abnMst.Operation ,
                                            ShiftStart = abnMst.ShiftStart ,
                                            ActualStart = abnMst.ActualStart ,
                                            ShiftEnd = abnMst.ShiftEnd ,
                                            ActualEnd = abnMst.ActualEnd ,
                                            Late = abnMst.Late ,
                                            Soon = abnMst.Soon ,
                                            Abnormal = abnMst.Abnormal ,
                                            RequestChangeStart = RequestChangeStart.ToString().Trim() ,
                                            RequestChangeEnd = RequestChangeEnd.ToString().Trim() ,
                                            uploaddate = abnormalDate ,
                                            Reason = Reason.ToString().Trim() ,

                                        });

                                    }
                                }
                                TempData["msg"] = "Upload Success!";
                            }
                        }

                    } catch(Exception e) {
                        ViewBag.mss = "need contact to IT. " + e.Message + ",  row " + Convert.ToString(mesRow);
                        Utilities.WriteLogException(e ,ViewBag.Status);
                        Session["Uploaditems"] = "NONE";
                    }
                } else
                    if(HttpContext.Request.HttpMethod.ToUpper() == HttpMethod.Post.Method && Session["ListAbnItem"] != null) {
                        var lsItem = Session["ListAbnItem"] as List<HR_ABN_Items> ?? new List<HR_ABN_Items>();
                        var abnRequestID = 0;
                        try {
                            var abnRequest= new HR_ABN_Request() {
                                ApproveMail = app.ApproverEmail ,
                                ApproveName = app.ApproverName ,
                                CreateDate = Utilities.GetDate_VietNam(DateTime.Now) ,
                                RequestBy = userLogin.Username ,
                                HrName = hr.fullname ,
                                HrMail = hr.value ,
                                Status = 1
                            };
                            db.HR_ABN_Request.Add(abnRequest);
                            db.SaveChanges();
                            abnRequestID = abnRequest.ID;

                            // var datenow = Utilities.GetDate_VietNam(DateTime.Today);
                            string mss1 = "";
                            foreach(var item in lsItem) {
                                var itemPending =
                                    db.HR_ABN_Items.FirstOrDefault(m =>
                                        m.EmpID == item.EmpID && m.uploaddate == item.uploaddate &&
                                        m.HR_ABN_Request.Status > 0);

                                if(itemPending != null) {
                                    mss1 = mss1 + (mss1 != "" ? ", " : "") + item.EmpID;
                                }
                            }
                            if(mss1 == "" && lsItem.Any()) {
                                lsItem.ForEach(a => a.RequestID = abnRequestID);
                                db.HR_ABN_Items.AddRange(lsItem);
                                db.SaveChanges();
                                var body = "Dear " + app.ApproverName +
                                           ", <br/><br/> Vui lòng phê duyệt Báo cáo bất thường #" +
                                           abnRequestID + ".<br/> " +
                                           "<span style='color:#0070c0;font-style: italic;'>Please approve or reject Abnormal request #" +
                                           abnRequestID + ".</span>";
                                Utilities.SendEmail(
                                    "Báo cáo bất thường #" + abnRequestID + "/ Abnormal report #" + abnRequestID ,
                                    userLogin.Email ,app.ApproverEmail ,userLogin.Email ,body);
                                Session.Remove("ListAbnItem");
                                TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            } else {
                                TempData["msg"] = "Nhân viên đã được tạo hoặc không có bất thường: " + mss1;
                                db = new MyContext();
                                if(abnRequestID > 0) {
                                    db.HR_ABN_Request.Remove(db.HR_ABN_Request.Find(abnRequestID));
                                    db.SaveChanges();
                                }
                            }
                        } catch(Exception e) {
                            Utilities.WriteLogException(e ,"abn/create");
                            TempData["msg"] = "<script>alert('System error, Please retry or contact to IT team!');</script>";
                            db = new MyContext();
                            if(abnRequestID > 0) {
                                db.HR_ABN_Request.Remove(db.HR_ABN_Request.Find(abnRequestID));
                                db.SaveChanges();
                            }

                        }
                    }

            } catch(Exception e) {
                Utilities.WriteLogException(e ,"abn/create");
                TempData["msg"] = "<script>alert('System error, Please retry or contact to IT team!');</script>";
            }

            return View(db.HR_ABN_Mst.Where(a => a.MailAdmin.ToLower().Contains(userLogin.Email.ToLower()) || a.DeptID == userLogin.DeptID).OrderBy(a => a.EmpID).ToList());

        }
        public ActionResult EditAbn(int ID ,FormCollection fr) {

            if(HttpContext.Request.HttpMethod.ToUpper() == HttpMethod.Post.Method) {
                try {
                    var request = db.HR_ABN_Request.SingleOrDefault(a => a.ID == ID);
                    if(request != null) {
                        if(fr["tacdong"] == "Approve" && request.Status == 1 && userLogin.Email.ToLower() == request.ApproveMail.ToLower()) {

                            request.ApproveDate = Utilities.GetDate_VietNam(DateTime.Now);
                            request.Status = 2;
                            db.SaveChanges();
                            var body = "Dear C&B Team, <br/><br/> Báo cáo đã được phê duyệt, vui lòng xử lý. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Abnormal has been approved. Please process this request.</span>";
                            Utilities.SendEmail("Báo cáo bất thường #" + request.ID + "/ Abnormal report #" + request.ID ,userLogin.Email ,request.HrMail ,request.TBL_USERS_MST.EMAIL ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        } else if(fr["tacdong"] == "Reject" && request.Status == 1 && userLogin.Email.ToLower() == request.ApproveMail.ToLower()) {
                            request.Status = -1;
                            request.ReasonReject = fr["body"];
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Báo cáo không được phê duyệt. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Abnormal has been Rejected.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Báo cáo bất thường #" + request.ID + "/ Abnormal report #" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        } else if(fr["tacdong"] == "Process" && request.Status == 2 && userLogin.Email.ToLower() == request.HrMail.ToLower()) {
                            request.Status = 3;
                            db.SaveChanges();
                            var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Báo cáo đã được xử lý. <br/> " +
                                       "<span style='color:#0070c0;font-style: italic;'>Abnormal has been processed.</span><br/><br/>" +
                                       fr["body"];
                            Utilities.SendEmail("Báo cáo bất thường #" + request.ID + "/ Abnormal report #" + request.ID ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,body);
                            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                            return RedirectToAction("Index");
                        } else
                            TempData["msg"] = "<script>alert('Bạn không thể thực hiện hành động này / Access denied ');</script>";

                    }

                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"abn/Edit");
                    TempData["msg"] = "<script>alert('System error, Please retry or contact to IT team!');</script>";
                }
            }


            return View(db.HR_ABN_Request.SingleOrDefault(a => a.ID == ID));
        }
        public ActionResult ListItem() {

            return PartialView("ListItem" ,Session["ListAbnItem"] as List<HR_ABN_Items>);
        }
        public JsonResult EditItem(string empID ,int id ,string field ,string value) {
            var lsItem = Session["ListAbnItem"] as List<HR_ABN_Items>;
            switch(field) {
                case "RequestChangeStart":
                    lsItem.Where(w => w.ID == id).ToList().ForEach(s => s.RequestChangeStart = value);
                    break;
                case "RequestChangeEnd":
                    lsItem.Where(w => w.ID == id).ToList().ForEach(s => s.RequestChangeEnd = value);
                    break;
                case "Reason":
                    lsItem.Where(w => w.ID == id).ToList().ForEach(s => s.Reason = value);
                    break;

            }

            return Json("" ,JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddItem(string[] listEmpID ,string TacDong) {



            var lsItem = new List<HR_ABN_Items>();
            if(TacDong.Contains("Add")) {
                if(Session["ListAbnItem"] == null) {
                    Session["ListAbnItem"] = new List<HR_ABN_Items>();
                }
                lsItem = Session["ListAbnItem"] as List<HR_ABN_Items>;
            }
            if(listEmpID != null) {
                foreach(var empID in listEmpID) {
                    var items = db.HR_ABN_Mst.Where(m => m.EmpID == empID);
                    foreach(var item in items) {

                        if(!lsItem.Any(a => a.EmpID == empID && a.uploaddate == item.uploaddate)) {
                            lsItem.Add(new HR_ABN_Items() {
                                ID = item.ID ,
                                Name = item.Name ,
                                Abnormal = item.Abnormal ,
                                ActualEnd = item.ActualEnd ,
                                ActualStart = item.ActualStart ,
                                Division = item.Division ,
                                EmpID = item.EmpID ,
                                Late = item.Late ,
                                Line = item.Line ,
                                Operation = item.Operation ,
                                Shift = item.Shift ,
                                ShiftEnd = item.ShiftEnd ,
                                ShiftStart = item.ShiftStart ,
                                Soon = item.Soon ,
                                DeptID = item.DeptID ,
                                //TBL_DEPARTMENT_MST = db.TBL_DEPARTMENT_MST.Find(item.DeptID) ,
                                uploaddate = item.uploaddate
                            });
                        }
                    }
                }
                if(TacDong.Contains("Export")) {

                    var data=  lsItem.Select(a => new {
                        a.Division ,
                        db.TBL_DEPARTMENT_MST.Find(a.DeptID).NAME ,
                        a.Shift ,
                        a.Line ,
                        a.EmpID ,
                        a.Name ,
                        a.Operation ,
                        a.ShiftStart ,
                        a.ActualStart ,
                        a.ShiftEnd ,
                        a.ActualEnd ,
                        a.Late ,
                        a.Soon ,
                        a.Abnormal ,
                        a.RequestChangeStart ,
                        a.RequestChangeEnd ,
                        a.Reason ,
                        a.uploaddate
                    }).ToList();
                    var excel = new ExcelPackage();
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");


                    workSheet.Cells[2 ,1].Value = "Xưởng /Division";
                    workSheet.Cells["A2:A3"].Merge = true;
                    workSheet.Cells["A2:A3"].AutoFitColumns();
                    workSheet.Cells[2 ,2].Value = "Phòng /Department";
                    workSheet.Cells["B2:B3"].Merge = true;
                    workSheet.Cells["B2:B3"].AutoFitColumns();
                    workSheet.Cells[2 ,3].Value = "Ca làm việc /Shift";
                    workSheet.Cells["C2:C3"].Merge = true;
                    workSheet.Cells["C2:C3"].AutoFitColumns();
                    workSheet.Cells[2 ,4].Value = "Tổ/ Bộ phận /Line";
                    workSheet.Cells["D2:D3"].Merge = true;
                    workSheet.Cells["D2:D3"].AutoFitColumns();
                    workSheet.Cells[2 ,5].Value = "Mã NV /ID";
                    workSheet.Cells["E2:E3"].Merge = true;
                    workSheet.Cells["E2:E3"].AutoFitColumns();
                    workSheet.Cells[2 ,6].Value = "Họ và tên /Name";
                    workSheet.Cells["F2:F3"].Merge = true;
                    workSheet.Cells["F2:F3"].AutoFitColumns();
                    workSheet.Cells[2 ,7].Value = "Vị trí/Công đoạn /Operation";
                    workSheet.Cells["G2:G3"].Merge = true;
                    workSheet.Cells["G2:G3"].AutoFitColumns();
                    workSheet.Cells[2 ,8].Value = "Giờ bắt đầu ca làm việc /Shift start";
                    workSheet.Cells["H2:H3"].Merge = true;
                    workSheet.Cells["H2:H3"].AutoFitColumns();
                    workSheet.Cells[2 ,9].Value = "Giờ bắt đầu ca làm việc thực tế /Actual start";
                    workSheet.Cells["I2:I3"].Merge = true;
                    workSheet.Cells["I2:I3"].AutoFitColumns();
                    workSheet.Cells[2 ,10].Value = "Giờ kết thúc ca làm việc /Shift end";
                    workSheet.Cells["J2:J3"].Merge = true;
                    workSheet.Cells["J2:J3"].AutoFitColumns();
                    workSheet.Cells[2 ,11].Value = "Giờ kết thúc thực tế /Actual end";
                    workSheet.Cells["K2:K3"].Merge = true;
                    workSheet.Cells["K2:K3"].AutoFitColumns();
                    workSheet.Cells[2 ,12].Value = "Đi muộn /Late";
                    workSheet.Cells["L2:L3"].Merge = true;
                    workSheet.Cells["L2:L3"].AutoFitColumns();
                    workSheet.Cells[2 ,13].Value = "Về sớm /Soon";
                    workSheet.Cells["M2:M3"].Merge = true;
                    workSheet.Cells["M2:M3"].AutoFitColumns();
                    workSheet.Cells[2 ,14].Value = "Bất thường /Abnormal";
                    workSheet.Cells["N2:N3"].Merge = true;
                    workSheet.Cells["N2:N3"].AutoFitColumns();
                    workSheet.Cells[2 ,15].Value = "Đề nghị được sửa /Request to edit";
                    workSheet.Cells["O2:P2"].Merge = true;
                    workSheet.Cells["O2:P2"].AutoFitColumns();
                    workSheet.Cells[2 ,17].Value = "Lý do /Reason";
                    workSheet.Cells["Q2:Q3"].Merge = true;
                    workSheet.Cells["Q2:Q3"].AutoFitColumns();
                    workSheet.Cells[2 ,18].Value = "Ngày bất thường /Abnormal date";
                    workSheet.Column(18).Style.Numberformat.Format = "m/d/yyyy";
                    workSheet.Cells["R2:R3"].Merge = true;
                    workSheet.Cells["R2:R3"].AutoFitColumns();

                    workSheet.Cells[3 ,15].Value = "Giờ bắt đầu /Start";
                    workSheet.Cells[3 ,16].Value = "Giờ kết thúc /End";
                    workSheet.Column(5).Style.Numberformat.Format = "@";
                    workSheet.Column(8).Style.Numberformat.Format = "@";
                    workSheet.Column(9).Style.Numberformat.Format = "@";
                    workSheet.Column(10).Style.Numberformat.Format = "@";
                    workSheet.Column(11).Style.Numberformat.Format = "@";
                    workSheet.Column(15).Style.Numberformat.Format = "@";
                    workSheet.Column(16).Style.Numberformat.Format = "@";
                    workSheet.Cells[4 ,1].LoadFromCollection(data ,false);
                    using(var col = workSheet.Cells[2 ,1 ,data.Count() + 2 + 1 ,18]) {
                        col.AutoFitColumns();
                        col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    workSheet.Row(2).Style.WrapText = true;
                    workSheet.Cells["A2:R2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells["A2:R2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                    workSheet.Cells["A2:R2"].Style.Font.Bold = true;
                    workSheet.Cells["A2:R2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells["A2:R2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    workSheet.Cells["A3:R3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells["A3:R3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                    workSheet.Cells["A3:R3"].Style.Font.Bold = true;
                    workSheet.Cells["A3:R3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells["A3:R3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    using(var memoryStream = new MemoryStream()) {
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Abnormal.xlsx");
                        excel.SaveAs(memoryStream);
                        memoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }

                }
            } else {
                TempData["msg"] = "<script>alert('Chưa chọn nhân viên!')</script>";
            }
            return PartialView("ListItem" ,lsItem);
        }

        public ActionResult Remove(string id) {
            if(id == "ALL") {
                Session["ListAbnItem"] = new List<HR_ABN_Items>();
            }
            var lsItem = Session["ListAbnItem"] as List<HR_ABN_Items>;
            var item = lsItem.FirstOrDefault(m => m.ID == int.Parse(id));
            if(item != null) {
                lsItem.Remove(item);
            }
            return PartialView("ListItem" ,lsItem);
        }



    }
}