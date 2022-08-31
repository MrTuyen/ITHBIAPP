using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using System.Web.Mvc;
using System.Windows.Media.Media3D;
using Castle.Core.Internal;
using ProductionApp.Helpers;
using ProductionApp.Models;
using WebGrease.Css.Extensions;


namespace ProductionApp.Controllers {
    public class ScanCaseController:BaseController {

        #region ScanCase

        // GET: ScanCase


        public ActionResult Index(int? groupId ,string line ,int? SHIFT) {
            ViewBag.fgroupId = groupId;
            ViewBag.fline = line;
            ViewBag.fSHIFT = SHIFT;
            ViewBag.Group = db.TBL_WLOT_LOC.Where(w => w.ISSUE_DATE == DateTime.Today).Select(w => w.TBL_GROUP_MST).Where(a => a.Activate == 1).Distinct().OrderBy(a => a.GROUP_NAME).ToList();
            if(Request.Browser.Browser.ToUpper() == "IE" ||
               Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER") {
                ViewBag.mess = "<script>alert('Warning, Bạn không thể in Scan bằng trình duyệt này!');</script>";
                return View();
            }
            if(line == null)
                return View();
            ViewBag.Line = db.TBL_WLOT_LOC.Where(g => g.GROUP_ID == groupId && g.ISSUE_DATE == DateTime.Today).OrderBy(g => g.LINE.Trim()).Select(a => a.LINE).Distinct().ToList();
            ViewBag.SHIFT = db.TBL_WLOT_LOC.Where(g => g.GROUP_ID == groupId && g.ISSUE_DATE == DateTime.Today).OrderBy(g => g.SHIFT).Select(a => a.SHIFT).Distinct().ToList();
            var listScan = db.TBL_CASE_LABEL.Where(c => DbFunctions.TruncateTime(c.TS_1) == DateTime.Today && c.LINE == line).OrderByDescending(c => c.TS_1).ToList();
            Session["count"] = listScan.Count;
            return View(listScan);
        }

        public ActionResult ShowData(string id ,int? groupId ,string line ,int? SHIFT) {
            var tmp = new ShowScanCase();

            if(userLogin == null)
                RedirectToAction("Index");
            if(Session["count"] == null)
                Session["count"] = 0;
            tmp.count = Convert.ToInt16(Session["count"]);
            tmp.CaseID = "0";
            if(id != null) {
                id = id.Trim();
                var datenow = DateTime.Now;
                try {

                    if(id.Length >= 9) {

                        if(db.TBL_CASE_LABEL.FirstOrDefault(t => t.LABEL_ID == id) == null) {
                            var oneCase = db.TBL_RAW_DATA.FirstOrDefault(t => t.LABEL_ID == id);
                            var wk = db.TBL_WLOT_LOC.FirstOrDefault(t => t.LINE == line && t.ISSUE_DATE == DateTime.Today);
                            if(oneCase != null && wk != null) {

                                if(SHIFT == 0 || groupId == 0) {
                                    ViewBag.Status = "DATAVALIDATE";
                                } else {
                                    tmp.CaseID = id;
                                    Session["count"] = Convert.ToInt16(Session["count"]) + 1;
                                    tmp.count = Convert.ToInt16(Session["count"]);
                                    tmp.Qty = Convert.ToDouble(oneCase.QUANTITY);


                                    var caseRecord = new TBL_CASE_LABEL {
                                        WLOT_ID = oneCase.WLOT.Trim() ,
                                        LABEL_ID = oneCase.LABEL_ID ,
                                        QUANTITY = oneCase.QUANTITY ,
                                        PkgStyle = oneCase.PkgStyle ,
                                        COLOR = oneCase.COLOR ,
                                        TYPE = oneCase.LABEL_TYPE ,
                                        SIZE = oneCase.SIZE ,
                                        TS_1 = datenow ,
                                        TS_1_USER = userLogin.Username ,
                                        TS_2_USER = userLogin.Username ,
                                        TS_2 = datenow ,
                                        STATUS = 1 ,
                                        PLANT_CODE = oneCase.Plant_Code ,
                                        GROUP_ID = groupId ,
                                        SellingStyle = oneCase.SellingStyle ,
                                        MnfStyle = oneCase.MnfStyle ,
                                        LINE = line ,
                                        SHIFT = SHIFT ,
                                        WK = wk.WK
                                    };
                                    db.TBL_CASE_LABEL.Add(caseRecord);
                                    // db.TBL_RAW_DATA.Remove(oneCase);
                                    db.SaveChanges();
                                    ViewBag.Status = "OK";
                                }
                                //}
                                //else
                                //{
                                //    ViewBag.Status = "GroupNotFound";
                                //    var cf = new TBL_LABEL_FAILURE();
                                //    cf.LABEL_ID = id;
                                //    cf.TS_1 = datenow;
                                //    cf.TS_1_USER = usr.Username;
                                //    db.TBL_LABEL_FAILURE.Add(cf);
                                //    db.SaveChanges();

                                //}
                            } else {
                                ViewBag.Status = "TemNotFound";
                                //var cf = new TBL_LABEL_FAILURE
                                //{
                                //    LABEL_ID = id, TS_1 = datenow, TS_1_USER = userLogin.Username
                                //};
                                //db.TBL_LABEL_FAILURE.Add(cf);
                                //db.SaveChanges();

                            }
                        } else {
                            ViewBag.Status = "Duplicate";
                        }
                    }


                } catch(DbEntityValidationException e) {
                    //foreach(var eve in e.EntityValidationErrors) {
                    //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:" ,
                    //        eve.Entry.Entity.GetType().Name ,eve.Entry.State);
                    //    foreach(var ve in eve.ValidationErrors) {
                    //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"" ,
                    //            ve.PropertyName ,ve.ErrorMessage);
                    //    }
                    //}
                    Utilities.WriteLogException(e ,"ScanCase");
                  
                } catch(Exception e) {
                    ViewBag.Status = "SYSERROR";
                    Utilities.WriteLogException(e ,"ScanCase");
                }
            }
            ViewBag.tmp = tmp;
            return PartialView("_ShowData");
        }

        public ActionResult LoadTable(String caseId) {
            try {
                var item = db.TBL_CASE_LABEL.FirstOrDefault(s => s.LABEL_ID == caseId);
                if(Session["count__itemTableScanCase"] != null && Session["count__itemTableScanCase"] == Session["count"])
                    return PartialView("_itemTableScanCase");
                Session["count__itemTableScanCase"] = Session["count"];
                ViewBag.no = Session["count"];
                return PartialView("_itemTableScanCase" ,item);
            } catch(Exception e) {
                ViewBag.Status = "SYSERROR";
                Utilities.WriteLogException(e ,"ScanCase/LoadTable");
                return PartialView("_itemTableScanCase");
            }

        }
        public JsonResult GetLineByGroup(int GROUP_ID) {
            return Json(db.TBL_WLOT_LOC.Where(g => g.GROUP_ID == GROUP_ID && g.ISSUE_DATE == DateTime.Today).OrderBy(g => g.LINE).Select(a => a.LINE.Trim()).ToList() ,JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetShiftByGroup(int GROUP_ID) {
            return Json(db.TBL_WLOT_LOC.Where(g => g.GROUP_ID == GROUP_ID && g.ISSUE_DATE == DateTime.Today).OrderBy(g => g.SHIFT).Select(a => a.SHIFT).ToList() ,JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region OddCase
        public ActionResult OddCase(TBL_CASE_LABEL newCase) {
            var item = db.TBL_CASE_LABEL.FirstOrDefault(a => a.LABEL_ID == newCase.LABEL_ID);
            ViewBag.listItem = db.TBL_CASE_LABEL.Where(b => b.WLOT_ID == "OddLot" && b.GROUP_ID != null && b.TS_1 >= DateTime.Today).OrderByDescending(c => c.TS_1).ToList();
            ViewBag.Business = db.TBL_BUSINESS_MST.Where(b => b.ACTIVATE == 1).OrderBy(b => b.BIZ_NAME).ToList();
           // ViewBag.listStyle = db.TBL_SAH_MST.Select(a => a.Pkg_Style).Distinct().ToList();
            return View(item);
        }
        public ActionResult SaveOddCase(TBL_CASE_LABEL newCase ,string action) {
            try {
                var item = new TBL_CASE_LABEL {
                    WLOT_ID = "OddLot" ,
                    QUANTITY = newCase.QUANTITY ,
                    TYPE = "NM" ,
              //      PkgStyle = newCase.PkgStyle ,
                    COLOR = newCase.COLOR ,
                    SIZE = newCase.SIZE ,
                    STATUS = 1 ,
                    GROUP_ID = newCase.GROUP_ID ,
                    TS_1 = DateTime.Now ,
                    TS_2 = DateTime.Now ,
                    TS_1_USER = userLogin.Username ,
                    TS_2_USER = userLogin.Username ,
                    LABEL_ID = newCase.LABEL_ID ,
                    TBL_GROUP_MST = db.TBL_GROUP_MST.FirstOrDefault(a => a.GROUP_ID == newCase.GROUP_ID)
                };
                db.TBL_CASE_LABEL.Add(item);
                db.SaveChanges();

                return PartialView("_OddCaseItem" ,item);
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Scancase/SaveOddCase");
                return PartialView("_OddCaseItem");
            }
        }

        public JsonResult GetGroupByWshop(int wshopId) {
            return Json(db.TBL_GROUP_MST.Where(g => g.WSHOP_ID == wshopId).Select(g => new { g.GROUP_ID ,g.GROUP_NAME }) ,JsonRequestBehavior.AllowGet);
        }
        //public JsonResult GetSizeBySell(String PkgStyle) {
        //    return Json(db.TBL_SAH_MST.Where(g => g.Sel_Style == PkgStyle).Select(g => new { g.Size_Des , }) ,JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult GetColorBySize(String size) {
        //    return Json(db.TBL_SAH_MST.Where(g => g.Size_Des == size).Select(g => new { g.Color }) ,JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region PrintPackingList

        public ActionResult PrintPackingTest() {

            return View();
        }

        public ActionResult PrintPackingList(string wlId) {
            try {




                ViewBag.Group = db.TBL_WLOT_LOC.Where(w => w.ISSUE_DATE == DateTime.Today).Select(w => w.TBL_GROUP_MST).Where(a => a.Activate == 1).Distinct().OrderBy(a => a.GROUP_NAME).ToList();
                ViewBag.Carton = db.TBL_Carton_Mst.OrderBy(g => g.Remarks).ToList();
                if(Request.Browser.Browser.ToUpper() == "IE" ||
                   Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER") {
                    ViewBag.mess = "<script>alert('Warning, Bạn không thể in Packing bằng trình duyệt này!');</script>";
                    return View();
                }
                if(wlId == null)
                    return View();
                var listCase = db.TBL_CASE_LABEL.Where(w => w.WLOT_ID == wlId).ToList();
                var firstCase = listCase.FirstOrDefault();


                ViewBag.listCase = listCase;
                ViewBag.listCaseOdd = db.TBL_CASE_LABEL.Where(w => w.WLOT_ID == "OddLot" && w.GROUP_ID == firstCase.GROUP_ID && DbFunctions.TruncateTime(w.TS_1) == DateTime.Today).ToList();
                var fdate = DateTime.Now.AddDays(-1);
                ViewBag.listWL = db.TBL_CASE_LABEL.Where(g => g.PalletID == null && g.GROUP_ID == firstCase.GROUP_ID && (DbFunctions.TruncateTime(g.TS_1) == DateTime.Today || DbFunctions.TruncateTime(g.TS_1) == DbFunctions.TruncateTime(fdate))).Select(a => a.WLOT_ID).Distinct().ToList();


                var palletId = RenderPalletID();
                var TBL_KANBAN_DATA  =db.TBL_KANBAN_DATA.FirstOrDefault(a => a.AsstWO == wlId.Substring(2,6));

                if(firstCase != null) {
                    var toDay =  Utilities.GetDate_VietNam(DateTime.Today).Date;
                    PackingList packingList = new PackingList() {
                        Business = firstCase.TBL_GROUP_MST.TBL_BUSINESS_MST ,
                        Line = firstCase.LINE ,
                        Priority = TBL_KANBAN_DATA == null || TBL_KANBAN_DATA.Priority.IsNullOrEmpty() ? "Ký xác nhận(Ghi rõ họ tên)" : TBL_KANBAN_DATA.Priority ,
                        Group = firstCase.TBL_GROUP_MST ,
                        Color = firstCase.COLOR ,
                        PkgStyle = firstCase.PkgStyle ,
                        PalletId = palletId ,
                        QtyCarton = 0 ,
                        Carton = null ,
                        MnfStyle = firstCase.MnfStyle ,
                        SellStyle = firstCase.SellingStyle ,
                        Size = firstCase.SIZE ,
                        Ts1 = Utilities.GetMMDDYYYY(toDay.ToString()) ,
                        Wl = firstCase.WLOT_ID ,
                        Barcode = new Barcode().Create(palletId ,227 ,50) ,
                        wk = firstCase.WK
                    };

                    return View(packingList);

                }
                return View();
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Scancase/PrintPackingList");
                ViewBag.mess = "System error, Please retry or contact to IT team!";
                return View();
            }

        }
        public JsonResult GetWLByGroup(int GROUP_ID) {
            var fdate = DateTime.Now.AddDays(-1);
            return Json(db.TBL_CASE_LABEL.Where(g => g.PalletID == null && g.GROUP_ID == GROUP_ID && (DbFunctions.TruncateTime(g.TS_1) == DateTime.Today || DbFunctions.TruncateTime(g.TS_1) == DbFunctions.TruncateTime(fdate))).Select(a => a.WLOT_ID).Distinct().ToList() ,JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdatePalletID(string LABEL_ID ,string PalletID) {
            try {
                //var caseLabel = db.TBL_CASE_LABEL.FirstOrDefault(w => w.LABEL_ID == LABEL_ID && w.PalletID == null);
                //if(caseLabel != null) {

                //    caseLabel.PalletID = PalletID;


                //    db.SaveChanges();
                //}

                var update =  "update [TBL_CASE_LABEL]  set PalletID='" + PalletID + "'  where [LABEL_ID] in (" + LABEL_ID + ")";
                var result= db.Database.ExecuteSqlCommand(update);
                if(result > 0)
                    return Json(true ,JsonRequestBehavior.AllowGet);
                return Json(false ,JsonRequestBehavior.AllowGet);
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"ScanCase/UpdatePalletID");
                return Json(false ,JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CheckPalletID(string PalletID) {
            try {
                var caseLabel = db.TBL_CASE_LABEL.Any(w => w.PalletID == PalletID);
                if(caseLabel) {
                    return Json(true ,JsonRequestBehavior.AllowGet);
                }

                return Json(false ,JsonRequestBehavior.AllowGet);
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"ScanCase/UpdatePalletID");
                return Json(false ,JsonRequestBehavior.AllowGet);
            }
        }



        private String RenderPalletID() {
            try {
                var pallet = db.TBL_SYSTEM.Single(s => s.id == "PalletId");
                pallet.value = (int.Parse(pallet.value) + 1).ToString();
                db.SaveChanges();

                return DateTime.Now.Year.ToString().Substring(2) + "000000".Substring(pallet.value.Length) + pallet.value;
                //return pallet.value;

            } catch(Exception e) {
                Utilities.WriteLogException(e ,"/Scancase/RenderPalletID");
                throw;
            }

        }

        #endregion


    }

}