using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;
using System.Data.Entity;
using System.Net.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers {
    public class WLotLocController:BaseController {
        // GET: MasterData
        public ActionResult Index() {
            if((UserModels)Session["SignedInUser"] == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("UploadWlotLoc");
        }
        public ActionResult UploadWlotLoc(FormCollection fr) {

            var RdoModule = fr["RdoModule"];
            UserModels usr = (UserModels)Session["SignedInUser"];
            if(Request != null) {
                var MesRow = 0;
                var mss = "";
                try {

                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                        var fileName = file.FileName;
                        var fileContentType = file.ContentType;
                        var fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                        using(var package = new ExcelPackage(file.InputStream)) {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;

                            if("UploadSewingPlan" == RdoModule) {
                                var groups = db.TBL_GROUP_MST.ToDictionary(p => p.GROUP_NAME);
                                for(var rowIterator = 2; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    MesRow = rowIterator;
                                    var groupName = workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                                    var line = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                    var wk = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                    var shift = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                                    var hours = workSheet.Cells[rowIterator ,5].Value.ToString().Trim();
                                    var quantity = workSheet.Cells[rowIterator ,6].Value.ToString().Trim();
                                    var issDate = Convert.ToDateTime(workSheet.Cells[rowIterator ,7].Value.ToString());

                                    var tmp = db.TBL_WLOT_LOC.SingleOrDefault(t =>
                                        t.LINE == line && t.ISSUE_DATE == issDate);
                                    if(tmp != null) {
                                        db.TBL_WLOT_LOC.Remove(tmp);
                                    }
                                    if(double.Parse(hours) > 0) {
                                        var record = new TBL_WLOT_LOC {
                                            ISSUE_DATE = issDate ,
                                            GROUP_ID = groups.Single(a => a.Key == groupName).Value.GROUP_ID ,
                                            TS_1 = DateTime.Now ,
                                            TS_1_USER = usr.Username ,
                                            WK = wk.Trim() ,
                                            QUANTITY = quantity == "" ? 0 : Math.Round(double.Parse(quantity) ,2) ,
                                            LINE = line.Trim() ,
                                            HOURS = double.Parse(hours) ,
                                            SHIFT = int.Parse(shift)

                                        };
                                        db.TBL_WLOT_LOC.Add(record);
                                    }
                                    db.SaveChanges();

                                }
                            } else if("TargetOfEff" == RdoModule) {
                                var groups = db.TBL_GROUP_MST.ToDictionary(p => p.GROUP_NAME);
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    MesRow = rowIterator;
                                    var groupName = workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                                    var line = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                    var wk = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                    var shift = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                                    var hours = workSheet.Cells[rowIterator ,5].Value.ToString().Trim();
                                    var quantity = workSheet.Cells[rowIterator ,6].Value.ToString().Trim();
                                    var issDate = Convert.ToDateTime(workSheet.Cells[rowIterator ,7].Value.ToString());
                                    var record = db.TBL_Eff_Target.SingleOrDefault(t =>
                                        t.LINE == line && t.ISSUE_DATE == issDate);
                                    if(record != null) {
                                        db.TBL_Eff_Target.Remove(record);
                                    }
                                    if(groups.Any(a => a.Key == groupName)) {
                                        record = new TBL_Eff_Target() {
                                            GROUP_ID = groups.Single(a => a.Key == groupName).Value.GROUP_ID ,
                                            HOURS = double.Parse(hours) ,
                                            SHIFT = int.Parse(shift) ,
                                            LINE = line ,
                                            ISSUE_DATE = issDate ,
                                            QUANTITY = Math.Round(double.Parse(quantity) ,2) ,
                                            TS_1 = DateTime.Now ,
                                            TS_1_USER = usr.Username ,
                                            WK = wk
                                        };
                                        db.TBL_Eff_Target.Add(record);
                                        db.SaveChanges();

                                    } else {
                                        mss += "</br>Group/Tổ không đúng,  dòng " + Convert.ToString(MesRow);
                                    }
                                }


                            } else if("WorkingHour" == RdoModule) {
                                var groups = db.TBL_GROUP_MST.ToDictionary(p => p.GROUP_NAME);
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    MesRow = rowIterator;
                                    var groupName = workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                                    var LINE = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                    var issDate = Convert.ToDateTime(workSheet.Cells[rowIterator ,3].Value.ToString());
                                    var QUANTITY = double.Parse(workSheet.Cells[rowIterator ,4].Value.ToString().Trim());
                                    var HOURS = double.Parse(workSheet.Cells[rowIterator ,5].Value.ToString().Trim());
                                    var Shift = int.Parse(workSheet.Cells[rowIterator ,6].Value.ToString().Trim());
                                    var record = db.Tbl_Working_Hour.SingleOrDefault(t => t.LINE == LINE && t.DATEIN == issDate);
                                    if(record != null) {
                                        db.Tbl_Working_Hour.Remove(record);
                                    }

                                    record = new Tbl_Working_Hour() {
                                        GROUPID = groups.Single(a => a.Key == groupName).Value.GROUP_ID ,
                                        LINE = LINE ,
                                        SHIFT = Shift ,
                                        DATEIN = issDate ,
                                        HOURS = HOURS ,
                                        QUANTITY = QUANTITY ,
                                    };
                                    db.Tbl_Working_Hour.Add(record);
                                    db.SaveChanges();

                                }
                            } else {
                                mss += "Vui lòng chọn loại tài liệu";
                            }
                        }
                        ViewBag.Status = mss == "" ? "Successful" : mss;
                    }
                } catch(Exception e) {
                    ViewBag.Status = "need contact to IT. " + e.Message + ",  row " + Convert.ToString(MesRow);
                    Utilities.WriteLogException(e ,ViewBag.Status);
                }

            }
            return View("UploadWlotLoc");
        }


        public ActionResult UploadLotDetail() {

            UserModels usr = (UserModels)Session["SignedInUser"];

            if(Request != null) {
                int MesRow = 0;
                try {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                        using(var package = new ExcelPackage(file.InputStream)) {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for(int rowIterator = 7; rowIterator <= noOfRow; rowIterator++) {
                                MesRow = rowIterator;
                                if(workSheet.Cells[rowIterator ,4].Value.ToString() != "Produced" && workSheet.Cells[rowIterator ,5].Value.ToString() != "" && workSheet.Cells[rowIterator ,7].Value.ToString() != "ODDSLOT" && workSheet.Cells[rowIterator ,7].Value.ToString() != "OFFLOT2") {
                                    string WL = "000000".Substring(workSheet.Cells[rowIterator ,7].Value.ToString().Length) + workSheet.Cells[rowIterator ,7].Value;
                                    string Ass_WL = workSheet.Cells[rowIterator ,5].Value.ToString().Trim();
                                    string style = (workSheet.Cells[rowIterator ,8].Value.ToString()).Trim();
                                    string color = (workSheet.Cells[rowIterator ,9].Value.ToString()).Trim();
                                    string size = (workSheet.Cells[rowIterator ,10].Value.ToString()).Trim();
                                    double Qty = Convert.ToDouble(workSheet.Cells[rowIterator ,16].Value.ToString().Trim());
                                    //string ASN = (workSheet.Cells[rowIterator, 18].Value.ToString());
                                    var Revdate = workSheet.Cells[rowIterator ,19].Value;
                                    var IssDate = workSheet.Cells[rowIterator ,20].Value;
                                    string group_name = workSheet.Cells[rowIterator ,3].Value.ToString();
                                    TBL_WL tmp = db.TBL_WL.SingleOrDefault(t => t.WL_ID == (WL));
                                    if(tmp == null) {
                                        TBL_WL record = new TBL_WL();
                                        record.WL_ID = WL;
                                        record.ASST_WL_ID = Ass_WL;
                                        record.STYLE = style;
                                        record.COLOR = color;
                                        record.SIZE = size;
                                        record.QUANTITY = Convert.ToDouble(Qty);
                                        if(IssDate != null)
                                            record.ISSUE_DATE = Convert.ToDateTime(IssDate);
                                        if(Revdate != null)
                                            record.RECIEVE_DATE = Convert.ToDateTime(Revdate);
                                        record.LOCATION = group_name;
                                        record.TS_1 = DateTime.Now;
                                        record.TS_1_USER = "Admin";
                                        db.TBL_WL.Add(record);
                                        //db.SaveChanges();

                                        TBL_ASST_WL tmpA = db.TBL_ASST_WL.SingleOrDefault(t => t.ASST_ID == (Ass_WL));
                                        if(tmpA == null) {
                                            TBL_ASST_WL recordA = new TBL_ASST_WL();
                                            string selStyle = (workSheet.Cells[rowIterator ,13].Value.ToString());
                                            string selColor = (workSheet.Cells[rowIterator ,14].Value.ToString());
                                            string selSize = (workSheet.Cells[rowIterator ,15].Value.ToString());
                                            string ASN = (workSheet.Cells[rowIterator ,18].Value.ToString());
                                            string Plant = (workSheet.Cells[rowIterator ,2].Value.ToString());
                                            recordA.ASST_ID = Ass_WL;
                                            recordA.STYLE = selStyle.Replace(" " ,string.Empty);
                                            recordA.COLOR = selColor.Replace(" " ,string.Empty);
                                            recordA.SIZE = selSize.Replace(" " ,string.Empty);
                                            recordA.ASN = ASN;
                                            recordA.PLANT = Convert.ToInt16(Plant);
                                            recordA.TS_1 = DateTime.Now;
                                            db.TBL_ASST_WL.Add(recordA);
                                            //db.SaveChanges();
                                        }
                                    } else {
                                        tmp.ASST_WL_ID = Ass_WL;


                                    }
                                }
                                db.SaveChanges();
                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
                } catch(Exception e) {
                    ViewBag.Status = "Dữ liệu sai tại dòng:  " + e.Message + ",  dòng " + Convert.ToString(MesRow);
                    Utilities.WriteLogException(e ,ViewBag.Status);
                }
            }
            return View("UploadLotDetail");
        }
    }
}