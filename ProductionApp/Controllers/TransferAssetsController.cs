using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Office2013.Word;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PdfRpt.Core.Contracts;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class TransferAssetsController:BaseController {
        // GET: TransferAssets
        public ActionResult Index() {
            var approve = db.GRC_UserApprove.SingleOrDefault(a => a.Email.ToLower() == userLogin.Email.ToLower());
            ViewBag.per = approve != null ? 2 : 1;
            if(approve != null)
                ViewBag.ApproveMail = approve.authority;
            ViewBag.security = db.GRC_UserApprove.Single(a => a.Permission == "Security");

            var ls=db.GRC_TransferAssets.Where(a => (a.RequestStatus >= 1 && (a.ApproveStatus == 1 || a.FinanceStatus == 1 || a.HrStatus == 1 || a.DirectorStatus == 1 || a.SecurityStatus == 1)) && (a.RequestBy == userLogin.Username || a.ApproveMail.ToLower() == userLogin.Email.ToLower() || a.FinanceMail.ToLower() == userLogin.Email.ToLower() || a.HrMail.ToLower() == userLogin.Email.ToLower() || a.DirectorMail.ToLower() == userLogin.Email.ToLower() || a.SecurityMail.ToLower() == userLogin.Email.ToLower())).OrderByDescending(a => a.ID).ThenBy(a => new { a.ApproveStatus ,a.FinanceStatus ,a.HrStatus ,a.DirectorStatus }).ToList();
            if(userLogin.Username.ToLower() == "admin")
                ls = db.GRC_TransferAssets.OrderByDescending(a => a.ID).ThenBy(a => new { a.ApproveStatus ,a.FinanceStatus ,a.HrStatus ,a.DirectorStatus }).Take(100).ToList();
            return View(ls);
        }
        public ActionResult Create(FormCollection fr) {
            try {
                ViewBag.userLogin = userLogin;
                var security = db.GRC_UserApprove.SingleOrDefault(a => a.Permission == "Security");
                var appManager = db.TBL_USERS_MST.SingleOrDefault(a => a.DEPT == userLogin.DeptID && a.POSID == 1);
                ViewBag.appManager = appManager;

                var appFinace = db.GRC_UserApprove.SingleOrDefault(a => a.Permission == "finMgr");
                if(appFinace.authority != null && appFinace.authority.Length > 2) {
                    var tmp = db.OL_User_Approver.Single(a => a.EmpEmail.ToLower() == appFinace.authority.ToLower());
                    appFinace = new GRC_UserApprove {
                        id = tmp.UserCD ,
                        Email = tmp.EmpEmail ,
                        FullName = tmp.EmpName
                    };
                }
                ViewBag.appFinace = appFinace;

                var appHR = db.GRC_UserApprove.Single(a => a.Permission == "hrMgr");
                if(appHR.authority != null && appHR.authority.Length > 2) {
                    var tmp = db.OL_User_Approver.Single(a => a.EmpEmail.ToLower() == appHR.authority.ToLower());
                    appHR = new GRC_UserApprove {
                        id = tmp.UserCD ,
                        Email = tmp.EmpEmail ,
                        FullName = tmp.EmpName
                    };
                }
                ViewBag.appHR = appHR;

                var appFac = db.GRC_UserApprove.Single(a => a.Permission == "facMgr");
                if(appFac.authority != null && appFac.authority.Length > 2) {
                    var tmp = db.OL_User_Approver.Single(a => a.EmpEmail.ToLower() == appFac.authority.ToLower());
                    appFac = new GRC_UserApprove {
                        id = tmp.UserCD ,
                        Email = tmp.EmpEmail ,
                        FullName = tmp.EmpName
                    };
                }
                ViewBag.appFac = appFac;

                if(HttpContext.Request.HttpMethod.ToUpper() == HttpMethod.Post.Method) {
                    string fullPath = "";
                    if(Request != null) {
                        HttpPostedFileBase file = Request.Files["UploadedFile"];
                        if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                            fullPath = @"~\Uploads\transfer\" + file.FileName;
                            file.SaveAs(Server.MapPath(fullPath));
                        }
                    }

                    var item = new GRC_TransferAssets {
                        RequestDate = Utilities.GetDate_VietNam(DateTime.Now) ,
                        TransferDate = DateTime.Parse(fr["TransferDate"]) ,
                        Local = fr["InternalAssets"] != null ,
                        Detail = fr["Detail"] ,
                        Count = int.Parse(fr["Quantity"]) ,
                        Unit = fr["Unit"] ,
                        TransferTo = fr["Address"] ,
                        Temporary = fr["Temporary"] != null ,
                        RequestStatus = 1 ,
                        RequestBy = userLogin.Username ,
                        DeptID = userLogin.DeptID ,
                        ApproveMgr = appManager.FULLNAME ,
                        ApproveMail = appManager.EMAIL ,
                        ApproveStatus = 1 ,
                        Security = security.FullName ,
                        SecurityMail = security.Email ,
                        SecurityStatus = 1
                    };

                    if(fullPath.Length > 0) {
                        item.attackfile = fullPath;
                    }

                    if(item.Temporary) {
                        item.TemporaryStart = DateTime.Parse(fr["TemporaryStart"]);
                        item.TemporaryEnd = DateTime.Parse(fr["TemporaryEnd"]);
                    }
                    if(!item.Local) {
                        item.FinanceMgr = appFinace.FullName;
                        item.FinanceMail = appFinace.Email;
                        item.FinanceStatus = 1;

                        item.HrMgr = appHR.FullName;
                        item.HrMail = appHR.Email;
                        item.HrStatus = 1;

                        item.Director = appFac.FullName;
                        item.DirectorMail = appFac.Email;
                        item.DirectorStatus = 1;

                    }

                    db.GRC_TransferAssets.Add(item);
                    db.SaveChanges();

                    var detail = "Tên người chuyển/Full Name: " + userLogin.Fullname;
                    detail += "<br/>Bộ phận/Department: " + userLogin.TBL_DEPARTMENT_MST.NAME;
                    detail += "<br/>Tên tài sản/ Assets name: " + item.Detail;
                    detail += "<br/>Địa chỉ/Address: " + item.TransferTo;
                    detail += "<br/>Ngày Chuyển/Date: " + string.Format("{0:MM/dd/yyyy}" ,item.TransferDate);
                    detail += "<br/>Số lượng/Quantity: " + item.Count;
                    detail += "<br/>Đơn vị tính/Unit: " + item.Unit;
                    if(item.Temporary) {
                        detail += "<br/>Chuyển tạm thời/Temporary transfer: ";
                        detail += string.Format("{0:MM/dd/yyyy}" ,item.TemporaryStart) + " - " + string.Format("{0:MM/dd/yyyy}" ,item.TemporaryEnd);

                    }
                    var body = "Dear " + item.ApproveMgr +
                               ", <br/><br/> Vui lòng phê duyệt phiếu chuyển tài sản #" + item.ID + ".<br/> " +
                               "<span style='color:#0070c0;font-style: italic;'>Please approve or reject transfer assets request #" + item.ID + ".</span><br/> <br/> " + detail;
                    Utilities.SendEmail("Phiếu chuyển tài sản #" + item.ID + "/ transfer assets #" + item.ID ,userLogin.Email ,item.ApproveMail ,userLogin.Email ,body);

                    if(item.HrMail != null) {
                        body = "Dear " + item.HrMgr +
                                  ", <br/><br/> Vui lòng phê duyệt phiếu chuyển tài sản #" + item.ID + ".<br/> " +
                                  "<span style='color:#0070c0;font-style: italic;'>Please approve or reject transfer assets request #" + item.ID + ".</span><br/> <br/> " + detail;
                        Utilities.SendEmail("Phiếu chuyển tài sản #" + item.ID + "/ transfer assets #" + item.ID ,userLogin.Email ,item.HrMail ,"" ,body);
                    }
                    if(item.FinanceMail != null) {
                        body = "Dear " + item.FinanceMgr +
                                  ", <br/><br/> Vui lòng phê duyệt phiếu chuyển tài sản #" + item.ID + ".<br/> " +
                                  "<span style='color:#0070c0;font-style: italic;'>Please approve or reject transfer assets request #" + item.ID + ".</span><br/> <br/> " + detail;
                        Utilities.SendEmail("Phiếu chuyển tài sản #" + item.ID + "/ transfer assets #" + item.ID ,userLogin.Email ,item.FinanceMail ,"" ,body);
                    }
                    if(item.DirectorMail != null) {
                        body = "Dear " + item.Director +
                                  ", <br/><br/> Vui lòng phê duyệt phiếu chuyển tài sản #" + item.ID + ".<br/> " +
                                  "<span style='color:#0070c0;font-style: italic;'>Please approve or reject transfer assets request #" + item.ID + ".</span><br/> <br/> " + detail;
                        Utilities.SendEmail("Phiếu chuyển tài sản #" + item.ID + "/ transfer assets #" + item.ID ,userLogin.Email ,item.DirectorMail ,"" ,body);
                    }


                    TempData["msg"] = "<script>alert('Success!');</script>";
                    return RedirectToAction("Index");
                }

            } catch(Exception e) {
                ViewBag.mss = "need contact to IT. " + e.Message;
                Utilities.WriteLogException(e ,ViewBag.Status);
            }
            return View();
        }

        public ActionResult ViewFile(int ID) {
            try {
                var request = db.GRC_TransferAssets.SingleOrDefault(a => a.ID == ID);
                if(request.attackfile.ToLower().Contains(".xls")) {
                    var pdf= new ExcelFileToPdf();
                    pdf.ReadExcelFile(Server.MapPath(request.attackfile) ,1);
                    using(var memoryStream = new MemoryStream(pdf.SaveToClient())) {

                        Response.ContentType = "application/pdf";
                        Response.AddHeader("content-disposition" ,"inline;  filename=checklist.pdf");
                        memoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                } else if(request.attackfile.ToLower().Contains(".pdf")) {
                    Response.ContentType = "application/pdf";
                    Response.AppendHeader("Content-Disposition" ,"inline; filename=MyFile.pdf");

                    // Write the file to the Response  
                    const int bufferLength = 10000;
                    byte[] buffer = new Byte[bufferLength];
                    int length = 0;
                    Stream download = null;
                    try {
                        download = new FileStream(Server.MapPath(request.attackfile) ,
                            FileMode.Open ,
                            FileAccess.Read);
                        do {
                            if(Response.IsClientConnected) {
                                length = download.Read(buffer ,0 ,bufferLength);
                                Response.OutputStream.Write(buffer ,0 ,length);
                                buffer = new Byte[bufferLength];
                            } else {
                                length = -1;
                            }
                        }
                        while(length > 0);
                        Response.Flush();
                        Response.End();
                    } finally {
                        if(download != null)
                            download.Close();
                    }
                }





            } catch(Exception e) {
                ViewBag.mss = "need contact to IT. " + e.Message;
                Utilities.WriteLogException(e ,ViewBag.Status);
            }
            return View();

        }

        public ActionResult Edit(int ID ,FormCollection fr ,string Code ,string CodeAction) {

            if(userLogin == null) {
                return RedirectToAction("Login" ,"Account");
            }

            var request = db.GRC_TransferAssets.SingleOrDefault(a => a.ID == ID);
            var detail = "Tên người chuyển/Full Name: " + request.TBL_USERS_MST.FULLNAME;
            detail += "<br/>Bộ phận/Department: " + request.TBL_DEPARTMENT_MST.NAME;
            detail += "<br/>Tên tài sản/ Assets name: " + request.Detail;
            detail += "<br/>Địa chỉ/Address: " + request.TransferTo;
            detail += "<br/>Ngày Chuyển/Date: " + string.Format("{0:MM/dd/yyyy}" ,request.TransferDate);
            detail += "<br/>Số lượng/Quantity: " + request.Count;
            detail += "<br/>Đơn vị tính/Unit: " + request.Unit;
            if(request.Temporary) {
                detail += "<br/>Chuyển tạm thời/Temporary transfer: ";
                detail += string.Format("{0:MM/dd/yyyy}" ,request.TemporaryStart) + " - " + string.Format("{0:MM/dd/yyyy}" ,request.TemporaryEnd);
            }

            //request.ApproveMail = userLogin.Email;
            //request.FinanceMail = userLogin.Email;
            //request.HrMail = userLogin.Email;
            //request.DirectorMail = userLogin.Email;
            if(HttpContext.Request.HttpMethod.ToUpper() == HttpMethod.Post.Method) {
                if(fr["tacdong"] == "Approve") {
                    var valid = false;
                    if(userLogin.Email.ToLower() == request.ApproveMail.ToLower() && request.ApproveStatus != null && request.ApproveStatus == 1) {
                        request.ApproveDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.ApproveStatus = 2;
                        valid = true;
                    } else if(request.FinanceStatus != null && userLogin.Email.ToLower() == request.FinanceMail.ToLower() && request.FinanceStatus == 1) {
                        request.FinanceDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.FinanceStatus = 2;
                        valid = true;
                    } else if(request.HrStatus != null && userLogin.Email.ToLower() == request.HrMail.ToLower() && request.HrStatus == 1) {
                        request.HrDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.HrStatus = 2;
                        valid = true;
                    } else if(request.DirectorStatus != null && userLogin.Email.ToLower() == request.DirectorMail.ToLower() && request.DirectorStatus == 1) {
                        request.DirectorDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.DirectorStatus = 2;
                        valid = true;
                    } else if(request.SecurityStatus != null && userLogin.Email.ToLower() == request.SecurityMail.ToLower() && request.SecurityStatus == 1) {
                        request.SecurityDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.SecurityStatus = 2;
                        request.RequestStatus = 2;
                        valid = true;
                    }

                    if(valid) {
                        db.SaveChanges();
                        var body = "Dear " + request.TBL_USERS_MST.FULLNAME + ", <br/><br/> Yêu cầu đã được phê duyệt. <br/> " +
                                   "<span style='color:#0070c0;font-style: italic;'>Request has been approved.</span> <br/> <br/>" + detail;
                        Utilities.SendEmail("Phiếu chuyển tài sản #" + request.ID + "/ transfer assets #" + request.ID ,
                            userLogin.Email ,request.TBL_USERS_MST.EMAIL ,userLogin.Email ,body);
                        TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                    } else {
                        TempData["msg"] = "<script>alert('Bạn không thể phê duyệt / Access denied');</script>";
                    }

                    return RedirectToAction("Index");
                } else if(fr["tacdong"] == "Reject") {
                    var valid = false;
                    if(userLogin.Email.ToLower() == request.ApproveMail.ToLower() && request.ApproveStatus == 1) {
                        request.ApproveDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.ApproveStatus = -2;
                        valid = true;
                    } else if(request.FinanceStatus != null && userLogin.Email.ToLower() == request.FinanceMail.ToLower() && request.FinanceStatus == 1) {
                        request.FinanceDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.FinanceStatus = -2;
                        valid = true;
                    } else if(request.HrStatus != null && userLogin.Email.ToLower() == request.HrMail.ToLower() && request.HrStatus == 1) {
                        request.HrDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.HrStatus = -2;
                        valid = true;
                    } else if(request.DirectorStatus != null && userLogin.Email.ToLower() == request.DirectorMail.ToLower() && request.DirectorStatus == 1) {
                        request.DirectorDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.DirectorStatus = -2;
                        valid = true;
                    } else if(request.SecurityStatus != null && userLogin.Email.ToLower() == request.SecurityMail.ToLower() && request.SecurityStatus == 1) {
                        request.SecurityDate = Utilities.GetDate_VietNam(DateTime.Now);
                        request.SecurityStatus = -2;
                        valid = true;
                    }

                    if(valid) {
                        request.RequestStatus = -2;
                        request.ReasonReject = fr["body"];
                        db.SaveChanges();
                        var body = "Dear " + request.TBL_USERS_MST.FULLNAME +
                                   ", <br/><br/> Yêu cầu không được phê duyệt. <br/> " +
                                   "<span style='color:#0070c0;font-style: italic;'>Request has been Rejected.</span><br/><br/>" +
                                   fr["body"] + "<br/> <br/>" + detail;
                        Utilities.SendEmail("Phiếu chuyển tài sản #" + request.ID + "/ transfer assets #" + request.ID ,
                            userLogin.Email ,request.TBL_USERS_MST.EMAIL ,userLogin.Email ,body);
                        TempData["msg"] = "<script>alert('Thành công / Success');</script>";
                    } else {
                        TempData["msg"] = "<script>alert('Bạn không thể phê duyệt / Access denied');</script>";
                    }

                    return RedirectToAction("Index");
                }

            }

            return View(request);
        }

        public ActionResult ManagerDeligate(string email) {
            var user = db.OL_User_Approver.SingleOrDefault(a => a.EmpEmail.ToLower() == email.ToLower());
            if(user == null) {
                TempData["msg"] = "<script>alert('Email nhập không đúng / Email not valid');</script>";
                return RedirectToAction("Index");
            }

            var approve = db.GRC_UserApprove.SingleOrDefault(a => a.Email.ToLower() == userLogin.Email.ToLower());
            if(approve != null) {
                approve.authority = user.EmpEmail;
                db.SaveChanges();
            }

            var body = "Dear " + user.EmpName +
                       ", <br/><br/> Bạn đã được ủy quyền phê duyệt chuyển tài sản. <br/> " +
                       "<span style='color:#0070c0;font-style: italic;'>You have authorized property transfer approval.</span><br/><br/>";

            Utilities.SendEmail("Ủy quyền phê duyệt phiếu chuyển tài sản / Authorized to approve property transfer assets" ,
                userLogin.Email ,user.EmpEmail ,userLogin.Email ,body);
            TempData["msg"] = "<script>alert('Thành công / Success');</script>";

            return RedirectToAction("Index");
        }
        public ActionResult ManagerDeligateRemove() {
            var approve = db.GRC_UserApprove.SingleOrDefault(a => a.Email.ToLower() == userLogin.Email.ToLower());
            var user = db.OL_User_Approver.SingleOrDefault(a => a.EmpEmail.ToLower() == approve.authority.ToLower());
            if(approve != null) {
                approve.authority = null;
                db.SaveChanges();
            }
            var body = "Dear " + user.EmpName +
                       ", <br/><br/> Quyền phê duyệt tài sản đã được thu hồi <br/> " +
                       "<span style='color:#0070c0;font-style: italic;'>Property approval rights have been revoked.</span><br/><br/>";

            Utilities.SendEmail("Ủy quyền phê duyệt phiếu chuyển tài sản / Authorized to approve property transfer assets" ,
                userLogin.Email ,user.EmpEmail ,userLogin.Email ,body);
            TempData["msg"] = "<script>alert('Thành công / Success');</script>";
            return RedirectToAction("Index");

        }

        public ActionResult Export(DateTime date ,DateTime date1) {
            string ip = db.TBL_SYSTEM.SingleOrDefault(a => a.id == "website").value;
            var data = db.GRC_TransferAssets.Where(a => a.RequestDate >= date & a.RequestDate <= DbFunctions.AddDays(date1 ,1)).OrderByDescending(a => a.TransferDate).Select(a => new {
                a.ID ,
                a.Detail ,
                a.TransferDate ,
                a.TransferTo ,
                a.Count ,
                a.Unit ,
                a.TBL_USERS_MST.FULLNAME ,
                a.TBL_DEPARTMENT_MST.NAME ,
                a.ApproveMgr ,
                // checklist = !string.IsNullOrEmpty(a.attackfile) ? "<a href='" + ip + "/TransferAssets/Edit/" + a.ID + "'>Check list<a>" : "" ,
                // checklist = !string.IsNullOrEmpty(a.attackfile) ? "HYPERLINK(\"" + ip + "TransferAssets/Edit/" + a.ID + "\",\"Check list\")" : "" ,
                a.attackfile ,
                Status = a.RequestStatus == -2 ? "Rejected" : (a.RequestStatus == 1 ? "Pending" : "Successful")
            });

            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("report");
            workSheet.Row(1).Height = 30;
            var c = 1;
            var r = 1;
            var fc = c;
            workSheet.Cells[r ,c++].Value = "No#";
            workSheet.Cells[r ,c++].Value = "Description";
            workSheet.Cells[r ,c++].Value = "Transfer Date";
            workSheet.Column(c - 1).Style.Numberformat.Format = "dd/MM/yyyy";
            workSheet.Cells[r ,c++].Value = "Address";
            workSheet.Cells[r ,c++].Value = "Quantity";
            workSheet.Cells[r ,c++].Value = "Unit";
            workSheet.Cells[r ,c++].Value = "Requester";
            workSheet.Cells[r ,c++].Value = "Department";
            workSheet.Cells[r ,c++].Value = "Manager";
            workSheet.Cells[r ,c++].Value = "Check list";
            var collink = c - 1;
            workSheet.Cells[r ,c++].Value = "Status";

            workSheet.Cells[r ,fc ,r ,c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[r ,fc ,r ,c].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells[r ,fc ,r ,c].Style.Font.Bold = true;
            workSheet.Cells[r + 1 ,fc].LoadFromCollection(data ,false);


            var startCell = workSheet.Dimension.Start;
            var endCell = workSheet.Dimension.End;
            for(var row = startCell.Row + 1; row < endCell.Row + 1; row++) {
                if(workSheet.Cells[row ,collink].Value == null ||
                    workSheet.Cells[row ,collink].Value.ToString().Length <= 1)
                    continue;
                workSheet.Cells[row ,collink].Hyperlink = new Uri(ip + "/TransferAssets/viewfile/" + workSheet.Cells[row ,1].Value);
                workSheet.Cells[row ,collink].Value = "Click me!";
                workSheet.Cells[row ,collink].Style.Font.Color.SetColor(System.Drawing.ColorTranslator.FromHtml("#ff0000"));
            }

            using(var col = workSheet.Cells[r ,fc ,data.Count() + r ,c]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=TransferAsset.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return View();
        }
    }
}