using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers {
    public class StationaryController:BaseController {
        // GET: Stationary
        public ActionResult Index() {
            if(userLogin == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("Load_Stationaty");
        }
        public ActionResult Load_Stationaty(FormCollection fr) {

            if(userLogin != null) {
                if(userLogin.Username.ToLower() == "admin") {
                    ViewBag.list = db.STA_Orders.OrderByDescending(a => a.OrderId).ToList();
                    ViewBag.per = 3;
                } else {
                    var dateNow = Utilities.GetDate_VietNam(DateTime.Now);
                    //var user2 = db.TBL_USERS_MST.Find(user.Username);
                    var hr_team = db.TBL_SYSTEM.Where(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_TEAM").ToList();

                    if(hr_team.Count > 0) {
                        ViewBag.list = db.STA_Orders.Where(s => s.Status > 0 ).OrderByDescending(a => a.OrderId).Take(150).ToList();
                        ViewBag.per = 2;
                    } else {
                        if(db.STA_Orders.Any(a => a.DepId == userLogin.DeptID)) {
                            var fMonth = dateNow.Month == 1 ? 12 : dateNow.Month - 1;
                            var oderCheck= db.STA_Orders.Any(a => a.DepId == userLogin.DeptID);
                            var addbonus= db.STA_Orders.SingleOrDefault(a => a.DepId == userLogin.DeptID && a.DateTime.Value.Month == fMonth);
                            if(oderCheck && addbonus == null) {
                                var STA_Dep_Budget =db.STA_Dep_Budget.SingleOrDefault(a => a.DepId == userLogin.DeptID);
                                db.Entry(STA_Dep_Budget).State = System.Data.Entity.EntityState.Modified;
                                if(STA_Dep_Budget != null) {
                                    STA_Dep_Budget.BudgetBonus += STA_Dep_Budget.Budget;
                                    db.STA_Orders.Add(new STA_Orders() {
                                        Status = -99 ,
                                        Cost = STA_Dep_Budget.Budget * -1 ,
                                        Requester = userLogin.Username ,
                                        Approver = STA_Dep_Budget.Approver ,
                                        DateTimeApprove = DateTime.Now ,
                                        DepId = userLogin.DeptID ,
                                        Description = "Bù Budget do quên không đặt VPP" ,
                                        DateTime = dateNow.AddMonths(-1)
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                        ViewBag.per = 1;
                        ViewBag.list = db.STA_Orders.Where(s => s.DepId == userLogin.DeptID && s.Status != -99).OrderByDescending(a => a.OrderId).ToList();
                    }
                }

                if(fr["DownloadBudget"] != null && fr["DownloadBudget"] == "Download Budget") {
                    var dept = db.TBL_DEPARTMENT_MST;
                    var ls = db.STA_Dep_Budget.OrderBy(a => a.DepId).Select(a => new {
                        a.DepId ,
                        DeptName = dept.FirstOrDefault(b => b.DEPT_ID == a.DepId).NAME ,
                        a.Budget ,
                        a.BudgetBonus ,
                        a.Approver ,
                        a.Name ,
                        a.Division
                    }).ToList();


                    var excel = new ExcelPackage();
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                    workSheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    workSheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                    workSheet.Cells["A1:G1"].Style.Font.Bold = true;

                    workSheet.Cells[1 ,1].Value = "Dept ID";
                    workSheet.Cells[1 ,2].Value = "Dept Name";
                    workSheet.Cells[1 ,3].Value = "Budget";
                    workSheet.Cells[1 ,4].Value = "Budget Bonus";
                    workSheet.Cells[1 ,5].Value = "User Approve";
                    workSheet.Cells[1 ,6].Value = "FullName";
                    workSheet.Cells[1 ,7].Value = "Division";

                    workSheet.Cells[2 ,1].LoadFromCollection(ls ,false);
                    using(var col = workSheet.Cells[1 ,1 ,ls.Count() + 1 ,7]) {
                        col.AutoFitColumns();
                        col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    using(var memoryStream = new MemoryStream()) {
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Sta-Budget.xlsx");
                        excel.SaveAs(memoryStream);
                        memoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }

                }


            } else {
                ViewBag.list = "";
            }
            return View();
        }
        public ActionResult Add() {
            //  var tmp = db.STA_Item.OrderBy(a => a.StaName).ToList();
            ViewBag.stationary = db.STA_Item.OrderBy(a => a.StaName).ToList();

            var dept = db.TBL_USERS_MST.Find(userLogin.Username);
            var cost = db.STA_Dep_Budget.SingleOrDefault(s => s.DepId == dept.DEPT);
            ViewBag.ngansach = cost == null ? 0 : decimal.Parse((cost.Budget + cost.BudgetBonus).ToString());
            ViewBag.dept = db.TBL_DEPARTMENT_MST.Single(a => a.DEPT_ID == userLogin.DeptID);
            return View();
        }
        public ActionResult Edit(int id) {
            ViewData["list"] = db.STA_Order_Item.Where(s => s.OrderID == id).OrderBy(a => a.STA_Item.StaName).ToList();
            ViewData["order"] = db.STA_Orders.Find(id);

            return View();
        }
        public JsonResult Price_Unit(string id) {

            double price = 0;
            string unit = "";
            var tea = db.STA_Item.SingleOrDefault(s => s.StaId == id);
            if(tea != null) {
                price = double.Parse(tea.Price.ToString(CultureInfo.InvariantCulture));
                unit = tea.Unit;
            }
            return Json(new {
                price ,
                unit
            } ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Load() {
            decimal tongtien = 0;
            ViewBag.total = 0;
            List<stationary> List = null;
            if(Session["stationary"] != null) {
                List = (List<stationary>)Session["stationary"];
                foreach(stationary tb in List) {
                    tongtien = tongtien + tb.Total;
                }

            }

            return Json(new {
                ds = List ,
                tongtien
            } ,JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult Delete(string id) {
            var one = Session["stationary"] as List<stationary>;
            var itemXoa = one.FirstOrDefault(m => m.statiID == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add_Stat(string statiID ,int qty ,string unit ,string note ,Double price) {

            if(Session["stationary"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["stationary"] = new List<stationary>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            var name = db.STA_Item.SingleOrDefault(s => s.StaId == statiID);
            List<stationary> stationary = Session["stationary"] as List<stationary>;
            if(stationary.FirstOrDefault(m => m.statiID == statiID) == null) { //

                stationary newItem = new stationary() {
                    statiID = statiID ,
                    name = name.StaName ,
                    Qty = qty ,
                    Price = price ,
                    Unit = unit ,

                    Note = note

                };
                stationary.Add(newItem);

            } else {

                stationary item = stationary.FirstOrDefault(m => m.statiID == statiID);
                item.Qty = qty;
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }

        decimal tongtien() {
            decimal tongtien = 0;
            List<stationary> stationary = Session["stationary"] as List<stationary>;
            foreach(stationary sta in stationary) {
                tongtien = tongtien + sta.Total;

            }
            return tongtien;
        }
        int Count(int thang ,int dept) {
            int n = 0;
            var list = db.STA_Orders.Where(s => s.Status != -1).ToList();
            foreach(STA_Orders order in list) {
                if(order.DateTime.Value.Month == thang & order.DepId == dept) {
                    n = 1;
                }
            }
            return n;
        }
        [HttpPost]
        public ActionResult Submit(string feeback ,string description) {
            try {
                var date = db.STA_AllowDate.First();
                var stationary = Session["stationary"] as List<stationary>;
                //var app = db.TBL_USERS_MST.SingleOrDefault(s => s.DEPT == userLogin.DeptID & s.POSID == 1);
                var cost = db.STA_Dep_Budget.SingleOrDefault(s => s.DepId == userLogin.DeptID);
                var ngansach = cost == null ? 0 : decimal.Parse((cost.Budget + cost.BudgetBonus).ToString());
                var dept = int.Parse(userLogin.DeptID.ToString());
                var dateNow = Utilities.GetDate_VietNam(DateTime.Now);
                if(Count(dateNow.Month ,dept) == 0) {
                    if(date.BeginDate <= dateNow.Day & dateNow.Day <= date.EndDate) {
                        if(tongtien() <= ngansach) {
                            var order = new STA_Orders {
                                // OrderId = ma(),
                                Requester = userLogin.Username ,
                                DepId = userLogin.DeptID ,
                                DateTime = Utilities.GetDate_VietNam(DateTime.Now) ,
                                FeedBack = feeback ,
                                Description = description ,
                                Cost = tongtien() ,
                                Approver = cost.Approver ,
                                TBL_USERS_MST1 = db.TBL_USERS_MST.Single(a => a.USERNAME == cost.Approver) ,
                                Status = 1
                            };
                            db.STA_Orders.Add(order);
                            db.SaveChanges();

                            foreach(var t in stationary) {
                                var item = new STA_Order_Item {
                                    OrderID = order.OrderId ,
                                    StaId = t.statiID ,
                                    Price = decimal.Parse(t.Price.ToString()) ,
                                    Qty = t.Qty ,
                                    Note = t.Note
                                };
                                db.STA_Order_Item.Add(item);

                            }
                            db.SaveChanges();
                            Helpers.Utilities.SendEmail("Yêu cầu phê duyệt VPP/Stationery request for your approval  No#" + order.OrderId ,userLogin.Email ,order.TBL_USERS_MST1.EMAIL ,userLogin.Email ,"Dear " + order.TBL_USERS_MST1.FULLNAME + ",<br/><br/>Vui lòng phê duyệt đề nghị VPP .<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject stationery request .</span>");
                            TempData["msg"] = "<script>alert('Order VPP của bạn được tạo thành công!');</script>";
                            Session.Remove("stationary");
                            return RedirectToAction("Load_Stationaty");
                        } else {
                            TempData["msg"] = "<script>alert('Order của bạn vượt quá ngân sách!');</script>";
                        }

                    } else {
                        TempData["msg"] = "<script>alert('Bạn không thể order vào thời gian này!');</script>";
                    }
                } else {
                    TempData["msg"] = "<script>alert('Order đã được tạo trong tháng này');</script>";

                }
            } catch(Exception e) {
                TempData["msg"] = "<script>alert('Error, need to contact IT')</script>";
                Utilities.WriteLogException(e ,"");
            }
            return RedirectToAction("Add");
        }
        [HttpPost]
        public ActionResult Approve(int id) {
            var request = db.STA_Orders.Find(id);
            // var Approver = db.TBL_USERS_MST.SingleOrDefault(s => s.USERNAME == user.Username);
            if(userLogin.Username.ToLower() == request.Approver.ToLower() && request.Status == 1) {
                try {
                    request.DateTimeApprove = Utilities.GetDate_VietNam(DateTime.Now);
                    request.Status = 2;
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    var cost = db.STA_Dep_Budget.SingleOrDefault(s => s.DepId == request.DepId);
                    cost.BudgetBonus = (cost.BudgetBonus + cost.Budget) - request.Cost;
                    db.Entry(cost).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Helpers.Utilities.SendEmail("Yêu cầu phê duyệt VPP/Stationery request for your approval No#" + request.OrderId ,userLogin.Email ,request.TBL_USERS_MST.EMAIL ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Order VPP của bạn đã được phê duyệt.<br/><span style='color:#0070c0;font-style: italic;'>Stationery request has been approved.</span>");

                    TempData["msg"] = "<script>alert('Phê duyệt thành công!/Request has been approved');</script>";
                } catch(Exception e) {
                    TempData["msg"] = "<script>alert('Phê duyệt thất bại!/Not successful');</script>";
                    Utilities.WriteLogException(e ,"");
                }


            } else {
                TempData["msg"] = "<script>alert('Bạn không có quyền đê phê duyệt!/You are not allowed to approve this request');</script>";

            }
            return RedirectToAction("Load_Stationaty");
        }
        [HttpPost]
        public ActionResult Manager_Reject(int id ,string body) {
            var order = db.STA_Orders.Find(id);
            //var Approver = db.TBL_USERS_MST.SingleOrDefault(s => s.USERNAME == user.Username);
            if(userLogin.Username.ToLower() == order.Approver.ToLower() && order.Status == 1) {
                try {
                    order.Status = -1;
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["msg"] = "<script>alert('Thành công/Successful');</script>";
                    Helpers.Utilities.SendEmail("Yêu cầu phê duyệt VPP/Stationery request for your approval No#" + order.OrderId ,userLogin.Email ,order.TBL_USERS_MST.EMAIL ,"" ,"Dear " + order.TBL_USERS_MST.FULLNAME + ",<br/><br/>Yêu cầu VPP đã bị từ chối.<br/><span style='color:#0070c0;font-style: italic;'>Stationery request has been rejected.</span> <br/><br/>" + body);
                } catch {
                    TempData["msg"] = "<script>alert('Thất bại/Not successful');</script>";

                }

            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể hủy phê duyệt/Your account can't approve this request');</script>";
            }
            return RedirectToAction("Load_Stationaty");
        }
        [HttpPost]
        public ActionResult Upload_Dept() {
            // var user1 = db.TBL_USERS_MST.SingleOrDefault(s => s.USERNAME == user.Username);
            var loc1 = db.TBL_SYSTEM.FirstOrDefault(a => a.value.ToLower() == userLogin.Email.ToLower() & a.value3 == "HR_TEAM");
            if(loc1 != null) {

                if(Request != null) {
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


                                for(var rowIterator = 2; rowIterator <= noOfRow; rowIterator++) {
                                    MesRow = rowIterator;
                                    if(workSheet.Cells[rowIterator ,2].Value == null)
                                        break;
                                    var dept = workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                                    var budget = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                    var budgetbouns = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                                    var app = workSheet.Cells[rowIterator ,5].Value.ToString().Trim();
                                    var name = workSheet.Cells[rowIterator ,6].Value.ToString().Trim();

                                    var division = workSheet.Cells[rowIterator ,7].Value.ToString().Trim();

                                    int deptID = int.Parse(dept);
                                    var dept1 = db.TBL_DEPARTMENT_MST.SingleOrDefault(s => s.DEPT_ID == deptID);
                                    if(dept1 != null) {
                                        var emRecord = db.STA_Dep_Budget.SingleOrDefault(s => s.DepId == deptID);
                                        if(emRecord == null) {
                                            emRecord = new STA_Dep_Budget() {
                                                DepId = deptID ,
                                                Budget = decimal.Parse(budget) ,
                                                BudgetBonus = decimal.Parse(budgetbouns) ,
                                                Name = name ,
                                                Division = division ,
                                                Approver = app
                                            };
                                            db.STA_Dep_Budget.Add(emRecord);

                                        } else {
                                            emRecord.Budget = decimal.Parse(budget);
                                            emRecord.BudgetBonus = decimal.Parse(budgetbouns);
                                            emRecord.Name = name;
                                            emRecord.Division = division;
                                            emRecord.Approver = app;
                                            db.Entry(emRecord).State = System.Data.Entity.EntityState.Modified;


                                        }
                                        db.SaveChanges();
                                        mss = "Upload thành công!/Successful";
                                    } else {
                                        mss = "Kiểm tra lại Row " + Convert.ToString(MesRow) + " phòng của bạn không tồn tại";

                                    }


                                }

                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }

                    } catch(Exception e) {
                        TempData["msg"] = "<script>alert('Error, need to contact IT. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                        Utilities.WriteLogException(e ,"");
                    }
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload/ You are not allowed to upload master data');</script>";


            }

            return RedirectToAction("Load_Stationaty");
        }
        [HttpPost]
        public ActionResult Upload_STA_Item() {
            var loc1 = db.TBL_SYSTEM.FirstOrDefault(a => a.value.ToLower() == userLogin.Email.ToLower() & a.value3 == "HR_TEAM");
            if(loc1 != null) {


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
                                    MesRow = rowIterator;
                                    var id = workSheet.Cells[rowIterator ,1].Value.ToString();

                                    var name = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                    var price = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                    var unit = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();

                                    if(id == "") {
                                        mss = "Vui lòng kiểm tra lại dữ liệu/Please check data again...";
                                        break;
                                    }
                                    var emRecord = db.STA_Item.SingleOrDefault(t => t.StaId == id);

                                    if(emRecord == null) {
                                        emRecord = new STA_Item() {
                                            StaId = id ,
                                            StaName = name ,
                                            Price = decimal.Parse(price) ,
                                            Unit = unit
                                        };
                                        db.STA_Item.Add(emRecord);
                                        db.SaveChanges();
                                    } else {
                                        emRecord.Price = decimal.Parse(price);
                                        emRecord.StaName = name;
                                        emRecord.Unit = unit;
                                        db.Entry(emRecord).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    mss = "Upload thành công!/Successful";

                                }

                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    } catch(Exception e) {
                        TempData["msg"] = "<script>alert('Error, need to contact IT for support. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                        Utilities.WriteLogException(e ,"");
                    }
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload/You are not allowed to upload master data');</script>";


            }


            return RedirectToAction("Load_Stationaty");
        }
        public ActionResult Export(DateTime ngaybatdau ,DateTime ngayketthuc) {
            var ls = (from a in db.STA_Orders
                      join b in db.STA_Order_Item on a.OrderId equals b.OrderID
                      join c in db.STA_Item on b.StaId equals c.StaId
                      join d in db.STA_Dep_Budget on a.DepId equals d.DepId
                      join e in db.TBL_DEPARTMENT_MST on a.DepId equals e.DEPT_ID
                      join f in db.TBL_USERS_MST on a.Requester equals f.USERNAME
                      where a.Status == 2 && a.DateTime >= ngaybatdau & a.DateTime <= ngayketthuc
                      select new {
                          a.OrderId ,
                          a.DateTime ,
                          a.Cost ,
                          f.FULLNAME ,
                          e.NAME ,
                          d.Division ,
                          c.StaName ,
                          c.Price ,
                          b.Qty ,
                          c.Unit

                      }).OrderBy(a => new { a.Division ,a.NAME ,a.OrderId }).ToList();




            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells["A1:J1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:J1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
            workSheet.Cells["A1:J1"].Style.Font.Bold = true;
            workSheet.Column(2).Style.Numberformat.Format = "dd/MM/yyyy";
            var i = 1;
            workSheet.Cells[1 ,i++].Value = "OrderId";
            workSheet.Cells[1 ,i++].Value = "DateTime";
            workSheet.Cells[1 ,i++].Value = "Cost";
            workSheet.Cells[1 ,i++].Value = "FullName";
            workSheet.Cells[1 ,i++].Value = "Dept Name";
            if(db.STA_Dep_Budget.Count(a => a.Division != null) < 3) {

                workSheet.Cells[2 ,1].LoadFromCollection(ls.Select(a => new {
                    a.OrderId ,
                    a.DateTime ,
                    a.Cost ,
                    a.FULLNAME ,
                    a.NAME ,
                    a.StaName ,
                    a.Price ,
                    a.Qty ,
                    a.Unit
                }) ,false);
            } else {
                workSheet.Cells[1 ,i++].Value = "Division";
                workSheet.Cells[2 ,1].LoadFromCollection(ls ,false);
            }
            workSheet.Cells[1 ,i++].Value = "StaName";
            workSheet.Cells[1 ,i++].Value = "Price";
            workSheet.Cells[1 ,i++].Value = "Qty";
            workSheet.Cells[1 ,i].Value = "Unit";


            using(var col = workSheet.Cells[1 ,1 ,ls.Count() + 1 ,i]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=Stationary#" + DateTime.Now.Month + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Load_Stationaty");

        }
    }
}