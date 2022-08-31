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
using OfficeOpenXml.Style;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers {
    public class TeaBreakController:BaseController {
        // GET: TeaBreak
        public ActionResult Index() {
            if(userLogin == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("PR_Request");
        }
        public ActionResult PR_Request() {



            if(userLogin != null) {
                if(userLogin.Username == "admin") {
                    ViewBag.per = 3;
                    ViewBag.list = db.PR_TeaBreak_Request.OrderByDescending(a => a.ID).ToList();
                } else {
                    var user1 = db.TBL_USERS_MST.Find(userLogin.Username);
                    if(user1.DEPT == 5 && user1.POSID == 1) {
                        ViewBag.per = 2;
                        ViewBag.list = db.PR_TeaBreak_Request.Where(s => s.Status > 0 && s.Status < 5).OrderByDescending(a => a.ID).ToList();

                    } else {
                        var user2 = db.TBL_USERS_MST.Find(userLogin.Username);
                        var hr_team = db.TBL_SYSTEM.Where(s => s.value == user2.EMAIL & s.value3 == "HR_TEAM").ToList();

                        if(hr_team.Count > 0) {
                            ViewBag.per = 2;
                            ViewBag.list = db.PR_TeaBreak_Request.Where(s => s.Status > 0 && s.Status < 5).OrderByDescending(a => a.ID).ToList();
                        } else {
                            ViewBag.per = 1;
                            var request = db.TBL_USERS_MST.Find(userLogin.Username);
                            ViewBag.list = db.PR_TeaBreak_Request.Where(s => s.DeptID == request.DEPT).OrderByDescending(a => a.ID).Take(50).ToList();
                        }
                    }
                }
                //   ViewBag.list = db.PR_TeaBreak_Request.Where(s => s.Status > 0 && s.Status < 5).ToList();
            } else {
                ViewBag.list = "";
            }



            return View();
        }

        public ActionResult TeaBreak1() {
            ViewBag.group = db.PR_TeaBreak_Group.ToList();
            ViewBag.Dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == userLogin.DeptID);
            ViewBag.manager = userLogin.DeptID == 3 ? new TBL_USERS_MST { FULLNAME = "Trần Văn Phú" ,EMAIL = "Tran.Phu@hanes.com" } : db.TBL_USERS_MST.Single(s => s.DEPT == userLogin.DeptID && s.POSID == 1);
            ViewBag.hrteam = db.TBL_SYSTEM.Where(s => s.value3 == "HR_TEAM").ToList();
            ViewBag.hrmanager = db.TBL_USERS_MST.Where(s => s.DEPT == 5 && s.POSID == 1).ToList();
            return View();
        }
        public ActionResult Load_TeaBreak() {
            double tongtien = 0;
            ViewBag.total = 0;
            List<TeaBreak> List = null;
            if(Session["teabreak"] != null) {
                List = (List<TeaBreak>)Session["teabreak"];
                foreach(TeaBreak tb in List) {
                    tongtien = tongtien + tb.Total;
                }

            }

            return Json(new { ds = List ,tongtien = tongtien } ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult List_TeaBreak(int id) {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.PR_TeaBreak_Mst.Where(s => s.GroupID == id).OrderBy(a => a.Name_teabreak).ToList();
                return Json(ds ,JsonRequestBehavior.AllowGet);
            } catch(Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            } finally {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            };
        }
        public ActionResult Modal_TeaBreak(int groupid ,int id_teabreak ,int qty) {


            if(Session["teabreak"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["teabreak"] = new List<TeaBreak>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            var teabreak = Session["teabreak"] as List<TeaBreak>;
            var name_teabreak = db.PR_TeaBreak_Mst.Find(id_teabreak);
            var name_group = db.PR_TeaBreak_Group.Find(groupid);
            if(teabreak.FirstOrDefault(m => m.ID == id_teabreak) == null) { //
                var newItem = new TeaBreak() {

                    GroupID = groupid ,
                    ID = id_teabreak ,
                    Name_Group = name_group.Name_Group ,
                    Name_TeaBreak = name_teabreak.Name_teabreak ,
                    Price = Double.Parse(name_teabreak.Price.ToString()) ,
                    Qty = qty ,

                };
                teabreak.Add(newItem);

            } else {

                TeaBreak item = teabreak.FirstOrDefault(m => m.ID == id_teabreak);
                item.Qty = qty;
            }


            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Edit_TeaBreak(int id) {
            var request = db.PR_TeaBreak_Request.Find(id);
            try {
                ViewData["dept"] = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == request.DeptID);
                ViewData["request"] = db.PR_TeaBreak_Request.Single(s => s.ID == id);
                ViewData["item"] = db.PR_TeaBreak_Items.Where(s => s.RequestID == id).ToList();
                ViewData["manager"] = db.TBL_USERS_MST.Single(s => s.EMAIL == request.ManagerMail);
                ViewData["hrmanager"] = db.TBL_USERS_MST.Single(s => s.EMAIL == request.HRManagerMail);
                ViewData["hrteam"] = db.TBL_SYSTEM.Single(s => s.value == request.HRProcessMail && s.value3 == "HR_TEAM");
            } catch(Exception e) {
                TempData["msg"] = "<script>alert('Error,Please check the data and retry again.')</script>";
                Utilities.WriteLogException(e ,"");
            }
            return View(request);
        }
        [HttpPost]
        public ActionResult Upload_Group() {
            int MesRow = 0;
            try {

                if(Request != null) {
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
                            for(var rowIterator = 2; rowIterator <= noOfRow; rowIterator++) {
                                if(workSheet.Cells[rowIterator ,1].Value == null)
                                    break;
                                MesRow = rowIterator;
                                var id = workSheet.Cells[rowIterator ,1].Value.ToString();
                                var name = workSheet.Cells[rowIterator ,2].Value;
                                var emRecord = db.PR_TeaBreak_Group.SingleOrDefault(t => t.Name_Group == name.ToString());
                                if(emRecord == null) {
                                    emRecord = new PR_TeaBreak_Group() {
                                        ID = int.Parse(id.ToString()) ,
                                        Name_Group = name.ToString() ,
                                    };
                                    db.PR_TeaBreak_Group.Add(emRecord);
                                    db.SaveChanges();
                                }

                            }

                            TempData["msg"] = "<script> alert('Thành công')</script>";
                        }


                    }
                }
            } catch(Exception e) {
                TempData["msg"] = "<script>alert('Error,Please check the data and retry again. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                Utilities.WriteLogException(e ,"");
            }
            return RedirectToAction("PR_Request");
        }

        [HttpPost]
        public ActionResult Upload_TeaBreak() {


            if(Request != null) {
                int MesRow = 0;
                try {

                    HttpPostedFileBase file = Request.Files["UploadedFile1"];
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

                            for(var rowIterator = 2; rowIterator <= noOfRow; rowIterator++) {
                                if(workSheet.Cells[rowIterator ,1].Value == null)
                                    break;
                                MesRow = rowIterator;
                                var id = workSheet.Cells[rowIterator ,1].Value.ToString();
                                var groupid = workSheet.Cells[rowIterator ,2].Value.ToString();
                                var name = workSheet.Cells[rowIterator ,3].Value;
                                var price = workSheet.Cells[rowIterator ,4].Value.ToString();

                                var emRecord =
                                        db.PR_TeaBreak_Mst.SingleOrDefault(t => t.Name_teabreak == name.ToString());

                                if(emRecord == null) {
                                    emRecord = new PR_TeaBreak_Mst() {
                                        ID = int.Parse(id.ToString()) ,
                                        GroupID = int.Parse(groupid.ToString()) ,
                                        Name_teabreak = name.ToString() ,
                                        Price = Double.Parse(price.ToString())
                                    };
                                    db.PR_TeaBreak_Mst.Add(emRecord);
                                    db.SaveChanges();
                                }

                                mss = "Upload thành công!";


                            }

                        }

                        TempData["msg"] = "<script> alert('" + mss + "')</script>";
                    }
                } catch(Exception e) {
                    TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ",  Row " +
                                      Convert.ToString(MesRow) + "')</script>";

                    Utilities.WriteLogException(e ,"");
                    //   return RedirectToAction("Index");
                }
            }


            return RedirectToAction("PR_Request");
        }

        public JsonResult Tea(int id) {

            double price = 0;
            var tea = db.PR_TeaBreak_Mst.Single(s => s.ID == id);
            if(tea != null) {
                price = double.Parse(tea.Price.ToString());
            }

            return Json(price ,JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AddRequest(string manager ,string hrmanager ,
            string hrteam ,int dept ,string content) {


            var managermail = db.TBL_USERS_MST.SingleOrDefault(s => s.EMAIL.ToLower() == manager.ToLower());
            var hrmanagermail = db.TBL_USERS_MST.SingleOrDefault(s => s.USERNAME == hrmanager);
            var hrteammail = db.TBL_SYSTEM.SingleOrDefault(s => s.id == hrteam);
            var request = new PR_TeaBreak_Request();
            try {
                var teabreak = Session["teabreak"] as List<TeaBreak>;
                if(teabreak != null && teabreak.Count > 0) {
                    request = new PR_TeaBreak_Request {
                        // ID = ma(),
                        DeptID = dept ,
                        RequestDate = Utilities.GetDate_VietNam(DateTime.Now) ,
                        RequestBy = userLogin.Username ,
                        ManagerMail = managermail.EMAIL ,
                        ManagerStatus = -2 ,
                        HRManagerMail = hrmanagermail.EMAIL ,
                        HRManagerStatus = -3 ,
                        HRProcessMail = hrteammail.value ,
                        HRProcessStatus = -4 ,
                        Status = 1 ,
                        Content = content
                    };
                    db.PR_TeaBreak_Request.Add(request);
                    db.SaveChanges();
                    foreach(var t in teabreak) {
                        var item = new PR_TeaBreak_Items {
                            RequestID = request.ID ,
                            TeaBreakID = t.ID ,
                            price = t.Price ,
                            Quantity = t.Qty ,
                            TotalPrice = t.Total
                        };
                        db.PR_TeaBreak_Items.Add(item);
                    }
                    db.SaveChanges();
                    Utilities.SendEmail("Yêu cầu xuất ăn đặc biệt số #" + request.ID ,userLogin.Email ,request.ManagerMail ,userLogin.Email ,"Dear " + managermail.FULLNAME + ", <br/>Bạn nhận được một yêu cầu phê duyệt xuất ăn đặc biệt");
                    TempData["msg"] = "<script>alert('Thành công');</script>";
                    Session.Remove("teabreak");
                }
            } catch(Exception ex) {
                if(request.ID > 0) {
                    db = new MyContext();
                    db.PR_TeaBreak_Request.Remove(db.PR_TeaBreak_Request.Find(request.ID));
                    db.SaveChanges();
                }
                Utilities.WriteLogException(ex ,"");
                TempData["msg"] = "<script>alert('Thất bại');</script>";
            }

            return RedirectToAction("PR_Request");

        }
        //public ActionResult Dept1(int id) {
        //    bool proxyCreation = db.Configuration.ProxyCreationEnabled;
        //    try {
        //        //set ProxyCreation to false
        //        db.Configuration.ProxyCreationEnabled = false;
        //        var ds = db.TBL_USERS_MST.FirstOrDefault(s => s.DEPT == id & s.POSID == 1);
        //        return Json(ds ,JsonRequestBehavior.AllowGet);
        //    } catch(Exception ex) {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json(ex.Message);
        //    } finally {
        //        //restore ProxyCreation to its original state
        //        db.Configuration.ProxyCreationEnabled = proxyCreation;
        //    }
        //}
        [HttpGet]
        public JsonResult Get(int id) {
            int price = 0;
            string ma = "";
            var rs = db.PR_TeaBreak_Request.Find(id);
            if(rs != null) {
                price = rs.ID;
                ma = rs.ManagerMail;
            }
            return Json(new { price ,ma } ,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Approve(int id) {
            var request = db.PR_TeaBreak_Request.Find(id);
            // var sysMail = db.TBL_SYSTEM.Single(s => s.id == "hycmail").value;
            var hrManager = db.TBL_USERS_MST.Single(s => s.EMAIL == request.HRManagerMail.ToLower());
            if(userLogin.Email.ToLower() == request.ManagerMail.ToLower() && request.Status == 1) {
                try {
                    request.Status = 2;
                    request.ManagerStatus = 2;
                    request.ManagerDate = Utilities.GetDate_VietNam(DateTime.Now);
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Helpers.Utilities.SendEmail("Yêu cầu xuất ăn đặc biệt số #" + request.ID ,userLogin.Email ,request.HRManagerMail ,"" ,"Dear " + hrManager.FULLNAME + ", <br/>Bạn nhận được một yêu cầu phê duyệt xuất ăn đặc biệt");
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
        public ActionResult HR_Approve(int id) {
            var request = db.PR_TeaBreak_Request.Find(id);
            //   var sysMail = db.TBL_SYSTEM.Single(s => s.id == "hycmail").value;
            if(userLogin.Email.ToLower() == request.HRManagerMail.ToLower() && request.Status == 2) {
                try {
                    request.Status = 3;
                    request.HRManagerStatus = 3;
                    request.HRManagerDate = Utilities.GetDate_VietNam(DateTime.Now);
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Helpers.Utilities.SendEmail("Yêu cầu xuất ăn đặc biệt số #" + request.ID ,userLogin.Email ,request.HRProcessMail ,"Giang.Pham1@hanes.com" ,"Hi HR Admin Team,<br/> Bạn nhận được một yêu cầu cần xử lý");
                    TempData["msg"] = "<script>alert('Thành công');</script>";

                } catch {
                    TempData["msg"] = "<script>alert('thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Request chưa được duyệt');</script>t";

            }

            return RedirectToAction("PR_Request");
        }
        [HttpPost]

        public ActionResult HRteam_Approve(int id ,string hrteam) {
            var request = db.PR_TeaBreak_Request.Find(id);
            var requestby = db.TBL_USERS_MST.Find(request.RequestBy);
            //var sysMail = db.TBL_SYSTEM.Single(s => s.id == "hycmail").value;
            if(userLogin.Email.ToLower() == request.HRProcessMail.ToLower() && request.Status == 3) {
                request.HRProcessStatus = 4;
                request.Status = 5;
                request.HRProcessDate = Utilities.GetDate_VietNam(DateTime.Now);
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                Helpers.Utilities.SendEmail("Yêu cầu xuất ăn đặc biệt số #" + request.ID ,userLogin.Email ,requestby.EMAIL ,"" ,"Dear " + requestby.FULLNAME + ", <br/>Yêu cầu xuất ăn đặc biệt đã được phê duyệt");
                TempData["msg"] = "<script>alert('Thành công";
            } else {
                TempData["msg"] = "<script>alert('Request chưa đủ điều kiện để bạn duyệt');</script>";

            }



            return RedirectToAction("PR_Request");
        }

        [HttpPost]
        public ActionResult SendMail(int id ,string body) {
            var request = db.PR_TeaBreak_Request.Find(id);
            if(userLogin.Email.ToLower() == request.ManagerMail.ToLower()) {
                var reby = db.TBL_USERS_MST.Find(request.RequestBy);
                request.Status = -1;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Từ chối thành công');</script>";
                Helpers.Utilities.SendEmail("Yêu cầu xuất ăn đặc biệt số #" + request.ID ,userLogin.Email ,reby.EMAIL ,"" ,"Dear " + reby.FULLNAME + ", <br/>Trưởng phòng đã từ chối yêu cầu của bạn: " + body);
            }

            return RedirectToAction("PR_Request");
        }
        [HttpPost]
        public ActionResult SendMail1(int id ,string body1) {
            var request = db.PR_TeaBreak_Request.Find(id);
            if(userLogin.Email.ToLower() == request.HRManagerMail.ToLower()) {
                var reby = db.TBL_USERS_MST.Find(request.RequestBy);
                request.Status = -2;
                db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Từ chối thành công');</script>";
                Helpers.Utilities.SendEmail("Yêu cầu xuất ăn đặc biệt số #" + request.ID ,userLogin.Email ,reby.EMAIL ,"" ,"Dear " + reby.FULLNAME + ", <br/>Nhân sự đã từ chối yêu cầu của bạn: " + body1);
            }

            return RedirectToAction("PR_Request");
        }
        public ActionResult Export(DateTime date ,DateTime date1) {
            var employeeList1 = (from a in db.PR_TeaBreak_Request
                                 join b in db.PR_TeaBreak_Items on a.ID equals b.RequestID
                                 join c in db.PR_TeaBreak_Mst on b.TeaBreakID equals c.ID
                                 join d in db.TBL_DEPARTMENT_MST on a.DeptID equals d.DEPT_ID
                                 where a.RequestDate >= date & a.RequestDate <= date1 && a.Status == 5
                                 select new {
                                     a.RequestDate ,
                                     d.NAME ,
                                     a.Content ,
                                     Teabreak = c.Name_teabreak ,
                                     b.price ,
                                     b.Quantity ,
                                     b.TotalPrice
                                 }).ToList();

            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("TeaBreak");
            workSheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells["A1:G1"].Style.Font.Bold = true;
            workSheet.Row(1).Height = 30;
            var i = 0;
            workSheet.Cells[1 ,++i].Value = "RequestDate";
            workSheet.Column(i).Style.Numberformat.Format = "m/d/yyyy";
            workSheet.Cells[1 ,++i].Value = "FullName";
            workSheet.Cells[1 ,++i].Value = "Content";
            workSheet.Cells[1 ,++i].Value = "Teabreak";
            workSheet.Cells[1 ,++i].Value = "price";
            workSheet.Cells[1 ,++i].Value = "Quantity";
            workSheet.Cells[1 ,++i].Value = "TotalPrice";

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
                Response.AddHeader("content-disposition" ,"attachment;  filename=HR-TeaBreak.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return RedirectToAction("PR_Request");

        }

    }
}