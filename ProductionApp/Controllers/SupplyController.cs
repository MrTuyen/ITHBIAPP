using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.Style;
using ProductionApp.Helpers;
using WebGrease.Css.Extensions;

namespace ProductionApp.Controllers {
    public class SupplyController:BaseController {
        // GET: Supply

        public ActionResult Index() {
            if(userLogin == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("List_Supply");
        }
        public ActionResult List_Supply() {
            if(!db.TBL_PERMISSION.Where(a => a.USERNAME == userLogin.Username).Select(a => a.TBL_CATEGORIES).Any(a => a.CA_URL.ToLower().Contains("supply"))) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }
            var listSupply = db.WH_Supply_Request.Where(s => (s.Status > 0 && s.Status < 5) || (s.WarehouseLocStatus == -3 && s.Status == -3) || (s.ManagerStatus == -2 && s.Status == -2)).OrderByDescending(s => s.ID);
            return View(listSupply);
        }

        public ActionResult Add_Supply() {
            if(!db.TBL_PERMISSION.Where(a => a.USERNAME == userLogin.Username).Select(a => a.TBL_CATEGORIES).Any(a => a.CA_URL.ToLower().Contains("supply"))) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            ViewBag.group_mst = db.TBL_GROUP_MST.ToList();
            ViewData["requestby"] = userLogin;
            ViewData["manager"] = db.WH_User_Approver.Where(s => s.value3 == "Prd_Approval").ToList();
            ViewData["loc"] = db.WH_User_Approver.Where(s => s.value3 == "WarehouseLoc").ToList();
            ViewData["issue"] = db.WH_User_Approver.Where(s => s.value3 == "WarehouseIssue").ToList();
            var supply = Session["supply"] as List<WH_Supply_Request_Item>;
            return View(supply);
        }
        [HttpPost]
        public ActionResult Delete(int id) {
            var one = Session["supply"] as List<WH_Supply_Request_Item>;
            var itemXoa = one.FirstOrDefault(m => m.ID == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Submit(int group_mst ,DateTime date ,string wl ,string manager ,string requestby ,string loc ,string issue) {
            try {

                if(wl.Length > 1 && date != null) {
                    var manager1 = db.TBL_USERS_MST.Single(s => s.USERNAME == manager);
                    var loc1 = db.WH_User_Approver.Single(s => s.id == loc && s.value3 == "WarehouseLoc");
                    var issue1 = db.WH_User_Approver.Single(s => s.id == issue && s.value3 == "WarehouseIssue");
                    var request = new WH_Supply_Request {
                        GroupID = group_mst ,
                        RequestBy = requestby ,
                        WL = wl ,
                        RequestDate = date ,
                        Createdate = Utilities.GetDate_VietNam(DateTime.Now) ,
                        ManagerMail = manager1.EMAIL ,
                        WarehouseLocMail = loc1.value ,
                        WarehouseLocStatus = 1 ,
                        WarehouseIssueMail = issue1.value ,
                        WarehouseIssueStatus = 1 ,
                        Status = 1 ,
                        ConfirmDate = date

                    };
                    db.WH_Supply_Request.Add(request);

                    var item = Session["supply"] as List<WH_Supply_Request_Item>;
                    if(item == null) {
                        TempData["msg"] = "<script>alert('Danh sách vật tư yêu cầu không được trống.');</script>";
                        return RedirectToAction("Add_Supply");
                    }
                    db.SaveChanges();

                    foreach(var t in item) {
                        var supplyItem = new WH_Supply_Request_Item {
                            RequestID = request.ID ,
                            Quantity = t.Quantity ,
                            SupplyName = t.SupplyName ,
                            Unit = t.Unit ,
                            Note = t.Note
                        };
                        db.WH_Supply_Request_Item.Add(supplyItem);

                    }
                    db.SaveChanges();
                   // Utilities.SendEmail("Supply Request No#" + request.ID ,userLogin.Email ,request.WarehouseLocMail ,userLogin.Email ,"Bạn nhận được một yêu cầu phê duyệt phiếu vật tư");
                    Session.Remove("supply");
                } else {
                    TempData["msg"] = "<script>alert('Nhập thiếu thông tin.');</script>";
                    return RedirectToAction("Add_Supply");
                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"");
                TempData["msg"] = "<script>alert('Thất bại.');</script>";
            }
            return RedirectToAction("List_Supply");
        }
        public ActionResult Modal_Supply() {
            ViewBag.supply = db.WH_Supply_MST.OrderBy(a => a.Name).ToList();
            return PartialView();
        }
        public ActionResult Modal_Supply1(int supplyID ,double qty_yc ,string note) {

            if(Session["supply"] == null) {
                Session["supply"] = new List<WH_Supply_Request_Item>();
            }
            var supplyRequest = Session["supply"] as List<WH_Supply_Request_Item>;
            var supply = db.WH_Supply_MST.Find(supplyID);
            if(supplyRequest.FirstOrDefault(m => m.SupplyName == supply.Name) == null) {

                var newItem = new WH_Supply_Request_Item() {

                    ID = supply.ID ,
                    SupplyName = supply.Name ,
                    Unit = supply.Unit ,
                    Quantity = qty_yc ,
                    Note = note

                };
                supplyRequest.Add(newItem);

            } else {

                var item = supplyRequest.FirstOrDefault(m => m.SupplyName == supply.Name);
                item.Quantity = qty_yc;
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Load_Supply() {
            var List = Session["supply"] as List<WH_Supply_Request_Item>;

            return Json(List ,JsonRequestBehavior.AllowGet);
        }
        int kiemtra() {

            if(userLogin != null) {
                var loc1 = db.WH_User_Approver.Where(s => s.value == userLogin.Email && s.value3 == "WarehouseLoc").ToList();
                if(loc1.Any())
                    return 0;

            }
            return 1;
        }

        public ActionResult Edit_Supply(int id) {
            try {


                var request = db.WH_Supply_Request.SingleOrDefault(a => a.ID == id);
                if(request == null)
                    return RedirectToAction("Index");

                ViewData["supply"] = request;
                ViewData["item"] = db.WH_Supply_Request_Item.Where(s => s.RequestID == id).ToList();
                ViewBag.loc1 = kiemtra() == 0 && request.Status == 2 ? 0 : 1;
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"");
                TempData["msg"] = "<script>alert('Thất bại.');</script>";
            }
            return View();
        }


        public ActionResult Export(DateTime ngaybatdau ,DateTime ngayketthuc) {
            var employeeList = (from b in db.WH_Supply_Request_Item
                                join c in db.WH_Supply_Request on b.RequestID equals c.ID
                                where c.RequestDate >= ngaybatdau & c.RequestDate <= ngayketthuc && c.Status >= 4
                                select new SupplyExport {
                                    ID = c.ID ,
                                    CreateDate = c.Createdate.ToString() ,
                                    RequestDate = c.RequestDate.ToString() ,
                                    Company = "" ,
                                    PlantCD = "" ,
                                    Document = c.WL ,
                                    NM = "" ,
                                    A = "" ,
                                    Item = b.SupplyName ,
                                    Weight = b.WH_Item_location.Sum(a => a.QuantityOut).ToString() ,
                                    Uom1 = b.Unit ,
                                    Location = "" ,
                                    WH_Item_location = b.WH_Item_location ,
                                    ConfirmDate = c.ConfirmDate.ToString() ,
                                    GroupName = c.TBL_GROUP_MST.GROUP_NAME ,
                                    Requester = c.RequestBy ,
                                }).ToList();

            foreach(var item in employeeList) {
                string tmp = "";
                foreach(var item1 in item.WH_Item_location) {
                    tmp += (tmp == "" ? "" : " ") + item1.LocationName + "(" + item1.QuantityOut + ")";
                }
                employeeList[employeeList.FindIndex(ind => ind.Item == item.Item)].Location = tmp;
            }
            if(employeeList.Count > 0) {
                var excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells["A1:P1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:P1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                workSheet.Cells["A1:P1"].Style.Font.Bold = true;

                workSheet.Cells[2 ,1].LoadFromCollection(employeeList ,true);
                using(var col = workSheet.Cells[1 ,1 ,employeeList.Count() + 1 ,16]) {
                    col.AutoFitColumns();
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using(var memoryStream = new MemoryStream()) {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition" ,"attachment;  filename=Supply-Request.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            } else

                TempData["msg"] = "<script>alert('Không tìm thấy dữ liệu');</script>";
            return RedirectToAction("List_Supply");

        }
        [HttpPost]
        public ActionResult Manager_Approve(int id) {
            var supply = db.WH_Supply_Request.Find(id);
            if(userLogin.Email.ToLower() == supply.ManagerMail.ToLower() && supply.Status == 1) {
                try {
                    supply.Status = 2;
                    supply.ManagerStatus = 2;
                    supply.ManagerDate = Utilities.GetDate_VietNam(DateTime.Now);
                    db.Entry(supply).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                   // Utilities.SendEmail("Supply Request No#" + supply.ID ,supply.ManagerMail ,supply.WarehouseLocMail ,supply.TBL_USERS_MST.EMAIL ,"Bạn có một yêu cầu cần confirm");

                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"");
                    TempData["msg"] = "<script>alert('Thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể phê duyệt');</script>";
            }

            return RedirectToAction("List_Supply");
        }
        [HttpPost]
        public ActionResult Manager_Reject(int id ,string body) {
            var supply = db.WH_Supply_Request.Find(id);
            if(userLogin.Email.ToLower() == supply.ManagerMail.ToLower() && supply.Status == 1) {
                try {
                    supply.Status = -2;
                    supply.ManagerStatus = -2;
                    supply.ManagerDate = Utilities.GetDate_VietNam(DateTime.Now);
                    supply.Note += "/Manager Reject: " + body;
                    db.Entry(supply).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                   // Utilities.SendEmail("Supply Request No#" + supply.WL ,supply.ManagerMail ,supply.TBL_USERS_MST.EMAIL ,"" ,body);
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"");
                    TempData["msg"] = "<script>alert('Thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể hủy phê duyệt');</script>";
            }
            return RedirectToAction("List_Supply");
        }
        [HttpPost]
        public ActionResult WarehouseLoc_Reject(int id ,string body) {
            var supply = db.WH_Supply_Request.Find(id);
            if(userLogin.Email.ToLower() == supply.WarehouseLocMail.ToLower() && supply.Status == 2) {
                try {
                    supply.Status = -3;
                    supply.WarehouseLocStatus = -3;
                    supply.WarehouseIssueStatus = -4;
                    supply.Note += "/WarehouseLoc Reject: " + body;
                    db.Entry(supply).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                   // Utilities.SendEmail("Supply Request No#" + supply.WL ,userLogin.Email ,supply.TBL_USERS_MST.EMAIL ,"" ,body);
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"");
                    TempData["msg"] = "<script>alert('Thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể hủy');</script>";
            }
            return RedirectToAction("List_Supply");
        }
        [HttpPost]
        public ActionResult Confirm(int id ,string loc) {
            var supply = db.WH_Supply_Request.SingleOrDefault(a => a.ID == id);
            if(supply != null) {
                if(userLogin.Email.ToLower() == supply.WarehouseLocMail.ToLower() && supply.Status == 2) {
                    var lsLoc = db.WH_Item_location.Where(a => a.WH_Supply_Request_Item.RequestID == id);
                    if(lsLoc.Any()) {
                        try {
                            supply.Status = 3;
                            supply.WarehouseLocStatus = 3;
                            supply.WarehouseLocDate = Utilities.GetDate_VietNam(DateTime.Now);
                            supply.ConfirmDate = Utilities.GetDate_VietNam(DateTime.Now);
                            db.Entry(supply).State = System.Data.Entity.EntityState.Modified;

                            foreach(var item in lsLoc) {
                                var supplyloc = db.WH_Supply_Location.FirstOrDefault(a =>
                                    a.WH_Supply_MST.Name == item.WH_Supply_Request_Item.SupplyName &&
                                    a.WH_Location_MST.LocationName == item.LocationName);
                                if(supplyloc != null) {
                                    supplyloc.inventory -= item.QuantityOut;
                                    db.Entry(supplyloc).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                            db.SaveChanges();

                          //  Utilities.SendEmail("Supply Request No#" + supply.ID ,userLogin.Email ,supply.WarehouseIssueMail ,"" ,"Bạn hãy gửi cho sản xuất và xác nhận");
                        } catch(Exception e) {
                            Utilities.WriteLogException(e ,"");
                            TempData["msg"] = "<script>alert('Thất bại');</script>";
                        }
                    } else {
                        TempData["msg"] = "<script>alert('Nhập thông tin vị trí và số lượng thực');</script>";
                        return RedirectToAction("Edit_Supply" ,new {
                            id = id
                        });
                    }
                } else {
                    TempData["msg"] = "<script>alert('Tài khoản của bạn không thể phê duyệt');</script>";
                }
            }
            return RedirectToAction("List_Supply");
        }
        [HttpPost]
        public ActionResult Send(int id ,string issue) {
            var supply = db.WH_Supply_Request.Find(id);
            if(userLogin.Email.ToLower() == supply.WarehouseIssueMail.ToLower() && supply.Status == 3) {
                try {
                    supply.Status = 4;
                    supply.WarehouseIssueStatus = 4;
                    supply.WarehouseIssueDate = Utilities.GetDate_VietNam(DateTime.Now);
                    db.Entry(supply).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                   // Helpers.Utilities.SendEmail("Supply Request No#" + supply.ID ,userLogin.Email ,supply.TBL_USERS_MST.EMAIL ,"" ,"Vật tư mà bạn yêu cầu đã được gửi, hãy xác nhận.");
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"");
                    TempData["msg"] = "<script>alert('Thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể phê duyệt');</script>";
            }

            return RedirectToAction("List_Supply");
        }

        [HttpPost]
        public ActionResult Receive(int id ,string Receive) {
            var supply = db.WH_Supply_Request.Find(id);
            if(userLogin.Username.ToLower() == Receive.ToLower() && (supply.Status == 4 || supply.WarehouseLocStatus == -3 || supply.ManagerStatus == -2)) {
                try {
                    supply.Status = supply.WarehouseLocStatus == -3 || supply.ManagerStatus == -2 ? -1 : 5;
                    supply.WarehouseIssueDate = Utilities.GetDate_VietNam(DateTime.Now);

                    db.Entry(supply).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"");
                    TempData["msg"] = "<script>alert('Thất bại');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể xác nhận');</script>";
            }

            return RedirectToAction("List_Supply");
        }
        public JsonResult Update_Loc(int ItemId ,int RequestId ,FormCollection fr) {
            // var tmp = fr["RequestId"];
            var item = db.WH_Supply_Request_Item.Find(ItemId);
            var locations = db.WH_Supply_Location.Where(a => a.WH_Supply_MST.Name == item.SupplyName);
            foreach(var location in locations) {
                var QuantityOut = double.Parse(fr["qty_tx_" + location.ID] == null || fr["qty_tx_" + location.ID] == "" ? "0" : fr["qty_tx_" + location.ID]);
                var newItemLocation = item.WH_Item_location.SingleOrDefault(a => a.LocationName == location.WH_Location_MST.LocationName);
                if(newItemLocation == null && QuantityOut > 0) {
                    newItemLocation = new WH_Item_location() {
                        LocationName = location.WH_Location_MST.LocationName ,
                        ItemID = ItemId ,
                        QuantityOut = QuantityOut
                    };
                    db.WH_Item_location.Add(newItemLocation);
                } else if(newItemLocation != null && QuantityOut > 0) {

                    newItemLocation.QuantityOut = QuantityOut;
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                } else if(newItemLocation != null) {
                    db.WH_Item_location.Remove(newItemLocation);
                }



            }

            db.SaveChanges();
            return Json("Success" ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Download_PDF(int id) {
            try {

                if(kiemtra() == 0) {
                    var rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/bin/CrystalReport") ,"Supply_print.rpt"));

                    var data = db.WH_Supply_Request_Item.Where(a => a.RequestID == id).Select(a => new SupplyCard() {
                        WL = a.WH_Supply_Request.WL ,
                        Createdate = a.WH_Supply_Request.Createdate ,
                        RequestDate = a.WH_Supply_Request.RequestDate ,
                        location_Name = "" ,
                        WH_Item_location = a.WH_Item_location.ToList() ,
                        GROUP_NAME = a.WH_Supply_Request.TBL_GROUP_MST.GROUP_NAME ,
                        unit_Name = a.Unit ,
                        // SupplyID = a.SupplyID ,
                        SupplyName = a.SupplyName ,
                        Quantity = a.Quantity ,
                        QuantityOut = a.WH_Item_location.Sum(b => b.QuantityOut) ,
                        Note = a.Note

                    }).ToList();
                    foreach(var item in data) {
                        string tmp = "";
                        foreach(var item1 in item.WH_Item_location) {
                            tmp += (tmp == "" ? "" : " ") + item1.LocationName + "(" + item1.QuantityOut + ")";
                        }
                        data[data.FindIndex(ind => ind.SupplyName == item.SupplyName)].location_Name = tmp;
                    }

                    rd.SetDataSource(data.ListToDataTable());
                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();
                    rd.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Landscape;
                    rd.PrintOptions.ApplyPageMargins(new CrystalDecisions.Shared.PageMargins(5 ,5 ,5 ,5));
                    rd.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;
                    var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0 ,SeekOrigin.Begin);
                    return File(stream ,"application/pdf" ,"Supply-Request#" + id + ".pdf");
                } else {
                    TempData["msg"] = "<script>alert('Tài khoản của bạn không thể in phiếu');</script>";
                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"");
                TempData["msg"] = "<script>alert('Thất bại');</script>";
            }
            return RedirectToAction("List_Supply");
        }


        [HttpPost]
        public ActionResult Upload_Supply() {

            var loc = db.WH_User_Approver.SingleOrDefault(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "WarehouseLoc");
            if(loc != null) {

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

                                var lsSupply= new List<WH_Supply_MST>();
                                var lsLocation= new List<WH_Location_MST>();
                                var lsSupplyLoc= new List<WH_Supply_Location>();
                                var newID = 0;
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {

                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    MesRow = rowIterator;
                                    var Name = workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                                    var LocationName = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                    var Unit = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                    var inventory = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                                    newID++;

                                    var existSupply = lsSupply.SingleOrDefault(a => a.Name == Name);
                                    if(existSupply == null) {
                                        existSupply = new WH_Supply_MST() {
                                            ID = newID ,
                                            Name = Name ,
                                            Unit = Unit
                                        };
                                        lsSupply.Add(existSupply);
                                    }

                                    var existLoc = lsLocation.SingleOrDefault(a => a.LocationName == LocationName);
                                    if(existLoc == null) {
                                        existLoc = new WH_Location_MST() {
                                            ID = newID ,
                                            LocationName = LocationName
                                        };
                                        lsLocation.Add(existLoc);
                                    }

                                    lsSupplyLoc.Add(new WH_Supply_Location() {
                                        ID = newID ,
                                        SupplyID = existSupply.ID ,
                                        LocationID = existLoc.ID ,
                                        inventory = double.Parse(inventory)
                                    });


                                }

                                var conString = ConfigurationManager.ConnectionStrings["ProductionAppEntities"].ConnectionString;
                                if(conString.ToLower().StartsWith("metadata=")) {
                                    System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(conString);
                                    conString = efBuilder.ProviderConnectionString;
                                }
                                //Delete All
                                db.Database.ExecuteSqlCommand("delete from WH_Supply_Location; delete from WH_Supply_MST; delete from WH_Location_MST; ");
                                //insert Supply
                                using(var con = new SqlConnection(conString)) {
                                    using(var sqlBulkCopy = new SqlBulkCopy(con)) {
                                        sqlBulkCopy.DestinationTableName = "WH_Supply_MST";
                                        sqlBulkCopy.ColumnMappings.Add("ID" ,"ID");
                                        sqlBulkCopy.ColumnMappings.Add("Name" ,"Name");
                                        sqlBulkCopy.ColumnMappings.Add("Unit" ,"Unit");

                                        con.Open();
                                        sqlBulkCopy.BulkCopyTimeout = 0;
                                        var table=lsSupply.ListToDataTable();
                                        sqlBulkCopy.WriteToServer(table);
                                        con.Close();
                                    }

                                }
                                //insert Location
                                using(var con = new SqlConnection(conString)) {
                                    using(var sqlBulkCopy = new SqlBulkCopy(con)) {
                                        sqlBulkCopy.DestinationTableName = "WH_Location_MST";
                                        sqlBulkCopy.ColumnMappings.Add("ID" ,"ID");
                                        sqlBulkCopy.ColumnMappings.Add("LocationName" ,"LocationName");
                                        con.Open();
                                        sqlBulkCopy.BulkCopyTimeout = 0;
                                        var table=lsLocation.ListToDataTable();
                                        sqlBulkCopy.WriteToServer(table);
                                        con.Close();
                                    }

                                }
                                //insert Supply Location
                                using(var con = new SqlConnection(conString)) {
                                    using(var sqlBulkCopy = new SqlBulkCopy(con)) {
                                        sqlBulkCopy.DestinationTableName = "WH_Supply_Location";
                                        sqlBulkCopy.ColumnMappings.Add("ID" ,"ID");
                                        sqlBulkCopy.ColumnMappings.Add("SupplyID" ,"SupplyID");
                                        sqlBulkCopy.ColumnMappings.Add("LocationID" ,"LocationID");
                                        sqlBulkCopy.ColumnMappings.Add("inventory" ,"inventory");
                                        con.Open();
                                        sqlBulkCopy.BulkCopyTimeout = 0;
                                        var table=lsSupplyLoc.ListToDataTable();
                                        sqlBulkCopy.WriteToServer(table);
                                        con.Close();
                                    }

                                }
                                mss = "Upload thành công!";
                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    } catch(Exception e) {
                        TempData["msg"] = "<script>alert('Error, Check row " + Convert.ToString(MesRow) + "' in the excel file and try again. " + e.Message + ", )</script>";

                        Utilities.WriteLogException(e);
                        //   return RedirectToAction("Index");
                    }
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload');</script>";
            }

            return RedirectToAction("List_Supply");
        }

    }
}