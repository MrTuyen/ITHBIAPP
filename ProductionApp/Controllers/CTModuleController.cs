using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using Castle.Core.Internal;
using ProductionApp.Controllers;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers
{
    public class CTModuleController : BaseController
    {

        public ActionResult FabricRcv()
        {


            Session["count"] = 0;
            Session["FabricReceived"] = new List<FabricReceivedModel>();
            return View();
        }

        public ActionResult ShowFabricRcv(string id, string id2)
        {


            List<FabricReceivedModel> FabricReceivedList = (List<FabricReceivedModel>)(Session["FabricReceived"]);
            if (id != null)
            {


                TBL_CT_ORDER_ST Wo = db.TBL_CT_ORDER_ST.FirstOrDefault(t => t.WO == id && t.PROCESS_ID == 5);
                FabricReceivedModel TmpFaRe = new FabricReceivedModel();
                if (Wo == null && id2 == "1")
                {
                    TBL_CT_PLAN PlanRecord = db.TBL_CT_PLAN.SingleOrDefault(t => t.WO == id);
                    if (PlanRecord == null)
                    {
                        TmpFaRe.WO = id;
                        TmpFaRe.Status = "Không có trong KH";
                        ViewBag.Status = "Not in Plan/ Không có trong Kế Hoạch!";
                        FabricReceivedList.Add(TmpFaRe);
                    }
                    else
                    {
                        InsertOrderStatus(id, "", 10, 5, DateTime.Now);
                        Session["count"] = Utilities.ConvertStringToInt(Session["count"].ToString()) + 1;
                        TmpFaRe.WO = id;
                        TmpFaRe.Status = "OK";
                        FabricReceivedList.Add(TmpFaRe);
                        //FabricReceivedList.Add(new FabricReceivedModel() { WO=id, Status="OK" });
                    }

                }
                else if (Wo == null && id2 == "2")
                {
                    TmpFaRe.WO = id;
                    TmpFaRe.Status = "Chưa nhận vải!";
                    FabricReceivedList.Add(TmpFaRe);
                }
                else if (Wo != null && Wo.STATUS_ID == 10 && id2 == "2")
                {
                    Wo.STATUS_ID = 11;
                    Wo.TS_1 = DateTime.Now;
                    Wo.TS_1_USER = userLogin.Username;
                    TmpFaRe.WO = id;
                    TmpFaRe.Status = "Đã return vải!";
                    FabricReceivedList.Add(TmpFaRe);
                    db.SaveChanges();
                    Session["count"] = Utilities.ConvertStringToInt(Session["count"].ToString()) + 1;
                }
                else if (Wo != null && Wo.STATUS_ID == 11 && id2 == "1")
                {
                    Wo.STATUS_ID = 10;
                    Wo.TS_1 = DateTime.Now;
                    Wo.TS_1_USER = userLogin.Username;
                    TmpFaRe.WO = id;
                    TmpFaRe.Status = "Đã nhận lại vải!";
                    FabricReceivedList.Add(TmpFaRe);
                    db.SaveChanges();
                    Session["count"] = Utilities.ConvertStringToInt(Session["count"].ToString()) + 1;
                }
                else
                {
                    TmpFaRe.WO = id;
                    TmpFaRe.Status = "Trùng lặp";
                    FabricReceivedList.Add(TmpFaRe);
                    ViewBag.Status = "Duplicate/ Trùng lặp!";
                }
            }
            Session["FabricReceived"] = FabricReceivedList;

            return PartialView("_ShowFabricRcv", FabricReceivedList);
        }

        public ActionResult ScanWoSprd()
        {

            Session["count_Spread"] = 0;
            Session["tableID"] = "N/A";
            Session["TS_2_USER"] = "N/A";
            Session["FabricSprd"] = new List<FabricSpreadModel>();
            return View();
        }

        public ActionResult ShowWoSprd(string id)
        {

            List<FabricSpreadModel> ListSprdWO = (List<FabricSpreadModel>)(Session["FabricSprd"]);

            if (id != null)
            {
                int WO;
                int.TryParse(id, out WO);
                if ((id.Length == 1 || id.Length == 2) && WO != 0)
                {

                    TBL_CT_TABLE_MST TableRec = db.TBL_CT_TABLE_MST.SingleOrDefault(t => t.TABLE_ID == WO && t.ACTIVATE == 1);
                    if (TableRec == null) { Session["tableID"] = "N/A"; } else { Session["tableID"] = id; Session["count_Spread"] = 0; (Session["FabricSprd"]) = null; (Session["FabricSprd"]) = new List<FabricSpreadModel>(); }
                }
                else if ((id.Length == 1 || id.Length == 2) && WO == 0)
                {
                    Session["tableID"] = "N/A";
                }
                else if ((id.Length == 6) && db.OL_User_Approver.Any(a => a.EmpID == id))
                {
                    Session["TS_2_USER"] = id;
                }
                else
                {
                    string[] TmpData = id.Split('-');
                    string OrderNo = TmpData[0];
                    OrderNo = Stand_WO(OrderNo);
                    double Qty = Convert.ToDouble(TmpData[1]);
                    int CutType = Utilities.ConvertStringToInt(TmpData[2]);
                    string marker = TmpData.Length > 3 ? TmpData[3] : "N" + OrderNo;


                    FabricSpreadModel FabSprd = new FabricSpreadModel();
                    FabSprd.Qty = Qty;
                    FabSprd.WO = OrderNo;
                    FabSprd.Type = CutType;
                    FabSprd.MARKER = marker;

                    if (OrderNo != null && CutType != 0 && Qty != 0)
                    {
                        TBL_CT_ORDER_ST OrderSttRec = db.TBL_CT_ORDER_ST.SingleOrDefault(t => t.WO == OrderNo && t.STATUS_ID == 10);
                        if (OrderSttRec == null)
                        {
                            FabSprd.Status = "Chưa được được cấp vải!";
                        }
                        else if (OrderSttRec != null)
                        {

                            TBL_CT_PLAN CTPlanRec = db.TBL_CT_PLAN.SingleOrDefault(t => t.WO == OrderNo);
                            double TmpQty = GetOrderCTProcQty(OrderNo, CutType) + Qty;
                            // var check = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(a => a.WO == OrderNo && a.MARKER == marker);
                            var datenow = DateTime.Now;
                            //  if(check == null) {
                            if (Convert.ToDouble(CTPlanRec.QUANTITY) <= 10 && Convert.ToDouble(CTPlanRec.QUANTITY) > 0 && Convert.ToDouble(CTPlanRec.QUANTITY * 1.9) < TmpQty)
                            {
                                FabSprd.Status = "Scan Trùng Lặp hoặc số lượng vượt quá kế hoạch!";
                            }
                            else if (Convert.ToDouble(CTPlanRec.QUANTITY) <= 20 && Convert.ToDouble(CTPlanRec.QUANTITY) > 10 && Convert.ToDouble(CTPlanRec.QUANTITY * 1.4) < TmpQty)
                            {
                                FabSprd.Status = "Scan Trùng Lặp hoặc số lượng vượt quá kế hoạch!";
                            }
                            else if (Convert.ToDouble(CTPlanRec.QUANTITY) <= 50 && Convert.ToDouble(CTPlanRec.QUANTITY) > 20 && Convert.ToDouble(CTPlanRec.QUANTITY * 1.3) < TmpQty)
                            {
                                FabSprd.Status = "Scan Trùng Lặp hoặc số lượng vượt quá kế hoạch!";
                            }
                            else if (Convert.ToDouble(CTPlanRec.QUANTITY) > 50 && Convert.ToDouble(CTPlanRec.QUANTITY * 1.1) < TmpQty)
                            {
                                FabSprd.Status = "Scan Trùng Lặp hoặc số lượng vượt quá kế hoạch!";
                            }
                            else
                            {
                                TBL_CT_ORDER_SPR_TBL TmpOderSprdRec = new TBL_CT_ORDER_SPR_TBL();
                                TmpOderSprdRec.CUTTING_TYPE = CutType;
                                TmpOderSprdRec.TBL_NO = Utilities.ConvertStringToInt(Session["tableID"].ToString());
                                TmpOderSprdRec.QUANTITY = Qty;
                                TmpOderSprdRec.MARKER = marker;
                                TmpOderSprdRec.WO = OrderNo;
                                TmpOderSprdRec.TS_1 = DateTime.Now;
                                FabSprd.Status = "WIP OK";
                                db.TBL_CT_ORDER_SPR_TBL.Add(TmpOderSprdRec);
                                db.SaveChanges();

                                InsertOrderStatus(OrderNo, marker, 20, 10, datenow);
                                InsertCtProcStatus(TmpOderSprdRec.ORDER_TBL_ID, 20, 10, datenow);
                                Session["count_Spread"] = Utilities.ConvertStringToInt(Session["count_Spread"].ToString()) + 1;
                            }

                            //} else {
                            //    if(Session["TS_2_USER"] != null && (string)Session["TS_2_USER"] == "N/A") {
                            //        FabSprd.Status = "Chưa Scan mã công nhân!";
                            //    } else if(!db.TBL_CT_ORDER_ST.Any(a => a.WO == OrderNo && a.MARKER == marker && a.STATUS_ID == 21)) {
                            //        check.TS_2 = DateTime.Now;
                            //        check.TS_2_USER = (string)Session["TS_2_USER"];
                            //        db.SaveChanges();
                            //        FabSprd.Status = "CUT OK";
                            //        InsertOrderStatus(OrderNo ,marker ,21 ,10 ,datenow);
                            //        InsertCtProcStatus(check.ORDER_TBL_ID ,21 ,10 ,datenow);
                            //        Session["count_CutDone"] = Utilities.ConvertStringToInt(Session["count_CutDone"].ToString()) + 1;
                            //    } else
                            //        FabSprd.Status = "Scan Trùng Lặp hoặc số lượng vượt quá kế hoạch!";

                            //}
                        }

                    }
                    ListSprdWO.Add(FabSprd);
                }

            }
            return PartialView("_ShowWoSprd", ListSprdWO);
        }
        public ActionResult CompleteCut()
        {

            Session["count_CutDone"] = 0;
            Session["tableID"] = "N/A";
            Session["TS_2_USER"] = "N/A";
            Session["FabricSprd"] = new List<FabricSpreadModel>();
            return View();
        }

        public ActionResult ShowCompleteCut(string id)
        {

            List<FabricSpreadModel> ListSprdWO = (List<FabricSpreadModel>)(Session["FabricSprd"]);

            if (id != null)
            {
                int WO;
                int.TryParse(id, out WO);
                if ((id.Length == 1 || id.Length == 2) && WO != 0)
                {

                    TBL_CT_TABLE_MST TableRec = db.TBL_CT_TABLE_MST.SingleOrDefault(t => t.TABLE_ID == WO && t.ACTIVATE == 1);
                    if (TableRec == null) { Session["tableID"] = "N/A"; } else { Session["tableID"] = id; Session["count_CutDone"] = 0; (Session["FabricSprd"]) = null; (Session["FabricSprd"]) = new List<FabricSpreadModel>(); }
                }
                else if ((id.Length == 1 || id.Length == 2) && WO == 0)
                {
                    Session["tableID"] = "N/A";
                }
                else if ((id.Length == 6) && db.OL_User_Approver.Any(a => a.EmpID == id))
                {
                    Session["TS_2_USER"] = id;
                }
                else
                {
                    string[] TmpData = id.Split('-');
                    string OrderNo = TmpData[0];
                    OrderNo = Stand_WO(OrderNo);
                    double Qty = Convert.ToDouble(TmpData[1]);
                    // string marker = !TmpData[2].IsNullOrEmpty() ? TmpData[2] : "-1";


                    FabricSpreadModel FabSprd = new FabricSpreadModel();
                    FabSprd.Qty = Qty;
                    FabSprd.WO = OrderNo;



                    if (OrderNo != null && Qty != 0)
                    {
                        TBL_CT_ORDER_ST OrderSttRec = db.TBL_CT_ORDER_ST.SingleOrDefault(t => t.WO == OrderNo && t.STATUS_ID == 10);
                        if (OrderSttRec == null)
                        {
                            FabSprd.Status = "Chưa được được cấp vải!";
                        }
                        else if (OrderSttRec != null)
                        {
                            var check = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(a => a.WO == OrderNo);
                            var datenow = DateTime.Now;
                            if (check != null)
                            {
                                FabSprd.Type = check.CUTTING_TYPE.Value;
                                FabSprd.MARKER = check.MARKER;
                                if (Session["TS_2_USER"] != null && (string)Session["TS_2_USER"] == "N/A")
                                {
                                    FabSprd.Status = "Chưa Scan mã công nhân!";
                                }
                                else if (!db.TBL_CT_ORDER_ST.Any(a => a.WO == OrderNo && a.MARKER == check.MARKER && a.STATUS_ID == 21))
                                {
                                    check.TS_2 = DateTime.Now;
                                    check.TS_2_USER = (string)Session["TS_2_USER"];
                                    check.TBL_NO_CUT = Utilities.ConvertStringToInt(Session["tableID"].ToString());
                                    db.SaveChanges();
                                    FabSprd.Status = "CUT OK";
                                    InsertOrderStatus(OrderNo, check.MARKER, 21, 10, datenow);
                                    InsertCtProcStatus(check.ORDER_TBL_ID, 21, 10, datenow);
                                    Session["count_CutDone"] = Utilities.ConvertStringToInt(Session["count_CutDone"].ToString()) + 1;
                                }
                                else
                                    FabSprd.Status = "Scan Trùng Lặp hoặc số lượng vượt quá kế hoạch!";

                            }
                            else
                            {
                                FabSprd.Status = "Vải chưa được trải WIP!";
                            }
                        }

                    }
                    ListSprdWO.Add(FabSprd);
                }

            }
            return PartialView("_Showcutdone", ListSprdWO);
        }

        public double GetOrderCTProcQty(string WO, int type)
        {
            double Qty = 0;
            Double type2 = 0, type3 = 0;
            List<PROC_GET_CT_LAST_CUTTING_PRC_ST_BY_ORDER_Result> CtWORec = (from item in db.GetCTLastCuttingPrcStByOrder(WO) select item).ToList();
            if (CtWORec != null)
            {

                foreach (var item in CtWORec)
                {
                    if ((item.CUTTING_TYPE == 1) && (item.STATUS_ID != 60))
                    {
                        type2 += Convert.ToDouble(item.QUANTITY);
                        type3 += Convert.ToDouble(item.QUANTITY);
                    }
                    else if ((item.CUTTING_TYPE == 2) && (item.STATUS_ID != 60))
                    {
                        type2 += Convert.ToDouble(item.QUANTITY);
                    }
                    else if ((item.CUTTING_TYPE == 3) && (item.STATUS_ID != 60)) { type3 += Convert.ToDouble(item.QUANTITY); }
                }

                if (type == 1) { Qty = type2 > type3 ? type2 : type3; } else if (type == 2) { Qty = type2; } else if (type == 3) { Qty = type3; }
            }
            return Qty;
        }

        public void InsertOrderStatus(string WO, string Marker, int Stt, int Proc, DateTime datenow)
        {
            //if(Stt == 21) {
            //    var tmp = db.TBL_CT_ORDER_ST.SingleOrDefault(a => a.WO == WO);
            //    tmp.STATUS_ID = Stt;
            //    db.SaveChanges();
            //} else {

            var tmpOrderStt = new TBL_CT_ORDER_ST
            {
                MARKER = Marker,
                STATUS_ID = Stt,
                WO = WO,
                PROCESS_ID = Proc,
                TS_1 = datenow,
                TS_1_USER = userLogin.Username
            };
            db.TBL_CT_ORDER_ST.Add(tmpOrderStt);
            db.SaveChanges();
            // }
        }

        public void InsertCtProcStatus(int OrderTblID, int Stt, int proc, DateTime datenow)
        {
            //if(Stt == 21) {
            //    var tmp = db.TBL_CT_CUTTING_PROCESS_ST.SingleOrDefault(a => a.ORDER_TBL_ID == OrderTblID);
            //    tmp.STATUS_ID = Stt;
            //    db.SaveChanges();
            //} else {

            var tmpCtProcStt = new TBL_CT_CUTTING_PROCESS_ST
            {
                ORDER_TBL_ID = OrderTblID,
                STATUS_ID = Stt,
                PROCESS_ID = proc,
                TS_1 = datenow,
                TS_1_USER = userLogin.Username
            };
            db.TBL_CT_CUTTING_PROCESS_ST.Add(tmpCtProcStt);
            db.SaveChanges();
            // }
        }

        public string Stand_WO(string str)
        {
            while (str.Length < 6)
            {
                str = "0" + str;
            }
            return str;
        }

        public ActionResult TBLMonitoring()
        {
            //if ((UserModels)Session["SignedInUser"] == null)
            //{
            //    return RedirectToAction("NeedLogin", "Notification", "Scan");
            //}

            return View();
        }

        public ActionResult ShowTBLMonitoring(string id)
        {
            DateTime dt;
            dt = id.IsNullOrEmpty() ? DateTime.Now.Date : Convert.ToDateTime(id).Date;

            var ActTBL = (from item in db.GetCTActivateTBL() select item).ToList();
            var AllTBLStt = new List<PROC_GET_CT_LAST_STT_BY_TBLNO_DATE_Result>();
            foreach (var item in ActTBL)
            {
                var OneTBLStt = (from Sitem in db.PROC_GET_CT_LAST_STT_BY_TBLNO_DATE(item.TABLE_ID, dt) select Sitem).ToList();
                AllTBLStt.AddRange(OneTBLStt);
            }

            List<ViewCTTableMonitoringModel> ListViewTableMornt = new List<ViewCTTableMonitoringModel>();
            foreach (var item in AllTBLStt)
            {

                ViewCTTableMonitoringModel TMPListViewTbl = new ViewCTTableMonitoringModel();
                TMPListViewTbl.WO = item.WO;
                TMPListViewTbl.TableNo = item.TBL_NO;
                TMPListViewTbl.OrderTblID = item.ORDER_TBL_ID;
                TMPListViewTbl.Quantity = Convert.ToDouble(item.QUANTITY);
                TMPListViewTbl.type = Utilities.ConvertStringToInt(item.CUTTING_TYPE.Value.ToString());
                var x = (from item2 in db.GetWorkingShift(item.TS_1) select item2.SHIFT_ID).FirstOrDefault();
                TMPListViewTbl.Shift = x == 0 ? 3 : Utilities.ConvertStringToInt(x.ToString());
                int tmpStt = Utilities.ConvertStringToInt(item.STATUS_ID.ToString());
                if (StatusToColor(tmpStt) != "")
                {
                    TMPListViewTbl.color = StatusToColor(tmpStt);
                }
                else
                {
                    TBL_CT_ORDER_SPR_TBL TmpOrderSprd = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(t => t.ORDER_TBL_ID == item.ORDER_TBL_ID);
                    if (TmpOrderSprd != null)
                    {
                        DateTime tmpDate = Convert.ToDateTime(TmpOrderSprd.TS_1);
                        if (item.STATUS_ID == 21 && tmpDate.AddHours(4) < DateTime.Now)
                            TMPListViewTbl.color = "yellow";

                    }
                }
                ListViewTableMornt.Add(TMPListViewTbl);

            }

            ViewBag.ActTBL = ActTBL;

            return PartialView("_ShowTBLMonitoring", ListViewTableMornt);
        }

        public string StatusToColor(int status)
        {
            string color = "";
            switch (status)
            {
                //case 20:
                //    color = "white";
                //break;

                case 20:
                    color = "#d3d3d3";
                    break;

                case 21:
                    color = "#ffffff";
                    break;

                case 30:
                    color = "orange";
                    break;
                case 50:
                    color = "greenyellow";
                    break;

                case 60:
                    color = "orangered";
                    break;
                case 40:
                    color = "NA";
                    break;
            }
            return color;
        }

        [HttpPost]
        public ActionResult ScanQC(FormCollection fc)
        {
            //if ((UserModels)Session["SignedInUser"] == null)
            //{
            //    return RedirectToAction("NeedLogin", "Notification", "Scan");
            //}

            var date = fc["Date"];
            var QcModule = fc["QcModule"];
            if (date != null && QcModule != null)
            {
                ViewBag.Date = Convert.ToDateTime(date).Date;
                ViewBag.QCModule = QcModule;
            }

            return View();
        }
        [HttpGet]
        public ActionResult ScanQCTblConf()
        {


            IEnumerable<PROC_GET_CT_ACTIVATE_TBL_Result> TblList = (from item in db.GetCTActivateTBL() select item).ToList();
            ViewBag.TblList = new SelectList(TblList, "TABLE_ID", "TABLE_ID");

            return View();
        }

        public ActionResult ShowQCTblConf(string dt, string tbl)
        {
            List<ViewCTTableMonitoringModel> ListViewTableMornt = new List<ViewCTTableMonitoringModel>();
            if (dt == null && tbl == null)
                return PartialView("_ShowQCTblConf", ListViewTableMornt);

            int TblNo = Utilities.ConvertStringToInt(tbl);
            DateTime Date = Convert.ToDateTime(dt);

            var AllTBLStt = (from Sitem in db.PROC_GET_CT_LAST_STT_BY_TBLNO_DATE(TblNo, Date) select Sitem).ToList();

            // ADD COLOR FOR SPRD_ID ADND GET OVERDUE STATUS

            foreach (var item in AllTBLStt)
            {
                ViewCTTableMonitoringModel TMPListViewTbl = new ViewCTTableMonitoringModel();
                if (item.PROCESS_ID < 30 && item.STATUS_ID > 20)
                {
                    TMPListViewTbl.WO = item.WO;
                    TMPListViewTbl.MARKER = item.MARKER;
                    TMPListViewTbl.TableNo = item.TBL_NO;
                    TMPListViewTbl.OrderTblID = item.ORDER_TBL_ID;
                    TMPListViewTbl.Quantity = Convert.ToDouble(item.QUANTITY);
                    TMPListViewTbl.type = item.CUTTING_TYPE.Value;
                    TMPListViewTbl.Status = item.STATUS_ID.Value;
                    TMPListViewTbl.Process = item.PROCESS_ID.Value;
                    int tmpStt = item.STATUS_ID.Value;
                    if (StatusToColor(tmpStt) != "")
                    {
                        TMPListViewTbl.color = StatusToColor(tmpStt);
                    }
                    else
                    {
                        TBL_CT_ORDER_SPR_TBL TmpOrderSprd = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(t => t.ORDER_TBL_ID == item.ORDER_TBL_ID);
                        if (TmpOrderSprd != null)
                        {
                            DateTime tmpDate = Convert.ToDateTime(TmpOrderSprd.TS_1);
                            if (item.STATUS_ID == 21 && tmpDate.AddHours(4) < DateTime.Now)
                                TMPListViewTbl.color = "yellow";
                        }
                    }
                    ListViewTableMornt.Add(TMPListViewTbl);
                }
            }
            return PartialView("_ShowQCTblConf", ListViewTableMornt);
        }

        [HttpPost]
        public ActionResult UpdateTblConf(FormCollection fc)
        {

            int total = Utilities.ConvertStringToInt(fc["TxtTotal"].ToString());
            var datenow = DateTime.Now;
            for (int i = 1; i <= total; i++)
            {
                int TblID = Utilities.ConvertStringToInt(fc["TxtTblID" + i.ToString()].ToString());
                string Conf = (fc["TxtConf" + TblID.ToString()]);
                if (Conf != null && Conf.ToUpper() == "Pass".ToUpper())
                {
                    InsertCtProcStatus(TblID, 30, 20, datenow);
                    TBL_CT_ORDER_SPR_TBL OrderSprdRecord = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(t => t.ORDER_TBL_ID == TblID);
                    InsertOrderStatus(OrderSprdRecord.WO, OrderSprdRecord.MARKER, 30, 20, datenow);
                }
                else if (Conf != null && Conf.ToUpper() == "Reject".ToUpper())
                {
                    InsertCtProcStatus(TblID, 60, 20, datenow);
                    TBL_CT_ORDER_SPR_TBL OrderSprdRecord = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(t => t.ORDER_TBL_ID == TblID);
                    InsertOrderStatus(OrderSprdRecord.WO, OrderSprdRecord.MARKER, 60, 20, datenow);
                }
            }

            return RedirectToAction("ScanQCTblConf");
        }

        [HttpGet]
        public ActionResult ScanQCCmpntInsp()
        {
            IEnumerable<PROC_GET_CT_ACTIVATE_TBL_Result> TblList = (from item in db.GetCTActivateTBL() select item).ToList();
            ViewBag.TblList = new SelectList(TblList, "TABLE_ID", "TABLE_ID");

            if ((PostbackValue)Session["Postback_CmpntInspt"] != null)
            {
                PostbackValue Postback = (PostbackValue)Session["Postback_CmpntInspt"];
            }
            else
            {
                Session["Postback_CmpntInspt"] = new PostbackValue();
            }
            return View();
        }

        public ActionResult ShowQCCmpntInsp(string dt, string tbl)
        {
            PostbackValue Postback = (PostbackValue)Session["Postback_CmpntInspt"];
            if (((PostbackValue)Session["Postback_CmpntInspt"]).TS_1 != null)
            {
                if (dt.IsNullOrEmpty() || tbl.IsNullOrEmpty())
                {
                    dt = Postback.TS_1.ToString() == "" ? null : Postback.TS_1.ToString();
                    tbl = Postback.num.ToString() == "" ? null : Postback.num.ToString();
                }
            }

            List<ViewCTComponetInspModel> ListViewCmpntInsp = new List<ViewCTComponetInspModel>();
            if (dt.IsNullOrEmpty() || tbl.IsNullOrEmpty())
                return PartialView("_ShowQCCmpntInsp", ListViewCmpntInsp);

            int TblNo = Utilities.ConvertStringToInt(tbl);
            DateTime Date = Convert.ToDateTime(dt);

            var AllTBLStt = db.PROC_GET_CT_LAST_STT_BY_TBLNO_DATE(TblNo, Date).Select(x => new
            {
                WO = x.WO,
                MARKER = x.MARKER,
                TableNo = x.TBL_NO,
                OrderTblID = x.ORDER_TBL_ID,
                Quantity = Convert.ToDouble(x.QUANTITY),
                type = x.CUTTING_TYPE.Value,
                Status = x.STATUS_ID.Value,
                Process = x.PROCESS_ID.Value,
                ProcessName =
                    (from item in db.TBL_CT_CUTTING_PROCESS_MST.Where(t => t.PROCESS_ID == x.PROCESS_ID)
                     select item.PROCESS_NAME).SingleOrDefault(),
                // Cmpnt = db.GetCTCmpntByWO((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.ORDER_TBL_ID == x.ORDER_TBL_ID) select item.WO).FirstOrDefault()) ,
                Garment = ((from item in db.TBL_CT_PLAN.Where(t => t.WO == x.WO) select item.GARMENT_STYLE)
                    .SingleOrDefault()),
                color = x.STATUS_ID.Value,
                //AQL = Convert.ToInt32(
                //    (from item in db.TBL_QC_AQL.Where(t =>
                //            t.RANGE_FROM <= x.QUANTITY * 12 && t.RANGE_TO >= x.QUANTITY * 12)
                //     select item.SAMPLE_QTY).FirstOrDefault()) ,
                //MaxDefect = Convert.ToInt32(
                //    (from item in db.TBL_QC_AQL.Where(t =>
                //            t.RANGE_FROM <= x.QUANTITY * 12 && t.RANGE_TO >= x.QUANTITY * 12)
                //     select item.MAX_DEF).FirstOrDefault())
            }).GroupBy(a => new { a.MARKER, a.type, a.Status, a.Garment, a.Process, a.ProcessName, a.color })            //      ,a.AQL ,a.MaxDefect 
                .Select(x => new ViewCTComponetInspModel
                {
                    MARKER = x.Key.MARKER,
                    Quantity = Convert.ToDouble(x.Sum(a => a.Quantity)),
                    type = x.Key.type,
                    Status = x.Key.Status,
                    Process = x.Key.Process,
                    ProcessName = x.Key.ProcessName,
                    Cmpnt = db.GetCTCmpntByWO(x.First().WO).ToList(),
                    Garment = x.Key.Garment,
                    color = StatusToColor(x.Key.color),
                    //   AQL = x.Key.AQL ,
                    //  MaxDefect = x.Key.MaxDefect
                }).GroupBy(x => x.MARKER).Where(g => g.Count() == 1).Select(g => g.FirstOrDefault()).Where(a => a.Status > 20).ToList();



            if (db.GetModuleByMidUser(((UserModels)Session["SignedInUser"]).Username, 60).SingleOrDefault() == null)
            {
                List<ViewCTComponetInspModel> removeitem = AllTBLStt.Where(t => t.Process == 30 && t.Status == 50).ToList();
                foreach (var item in removeitem) { AllTBLStt.Remove(item); }
            }

            //foreach(var item in AllTBLStt) {
            //    if(item.color == "") {
            //        TBL_CT_ORDER_SPR_TBL TmpOrderSprd = db.TBL_CT_ORDER_SPR_TBL.SingleOrDefault(t => t.ORDER_TBL_ID == item.OrderTblID);
            //        if(TmpOrderSprd != null) {
            //            DateTime tmpDate = Convert.ToDateTime(TmpOrderSprd.TS_1);
            //            if(item.Status == 20 && tmpDate.AddHours(4) < DateTime.Now)
            //                item.color = "yellow";
            //        }
            //    }
            //}

            List<ViewLastSttComponentModel> ComponentStt = new List<ViewLastSttComponentModel>();
            foreach (var item in AllTBLStt)
            {
                //List<PROC_GET_CT_LAST_CMPNT_STT_BY_SPRDTBLID_Result> CmpntRec = (from item2 in db.GetCTLastCmpntSttBySprdTblID(item.OrderTblID) select item2).ToList();
                var CmpntRec = db.TBL_CT_QC_COMPNT_STT.Where(a => a.MARKER == item.MARKER);
                if (CmpntRec != null)
                {
                    foreach (var item2 in CmpntRec)
                    {
                        ViewLastSttComponentModel tmp = new ViewLastSttComponentModel();
                        tmp.Marker = item2.MARKER.ToString();
                        tmp.ComponentID = Utilities.ConvertStringToInt(item2.COMPNT_ID.ToString());
                        tmp.Color = (item2.STATUS_ID == 50 ? "greenyellow" : "orangered");
                        ComponentStt.Add(tmp);
                    }
                }
            }
            ViewBag.AllCmpntMst = db.GetCTGarmentCmpntMst().ToList();
            ViewBag.ComponentStt = ComponentStt;

            Postback.TS_1 = Date.Date;
            Postback.num = TblNo;
            Session["Postback_CmpntInspt"] = Postback;

            return PartialView("_ShowQCCmpntInsp", AllTBLStt);
        }

        [HttpPost]
        public ActionResult UpdateCmpntInsp(FormCollection fc)
        {


            int total = Utilities.ConvertStringToInt(fc["TxtTotal"].ToString());
            for (int i = 1; i <= total; i++)
            {
                string TxtMARKER = fc["TxtMARKER" + i.ToString()].ToString();
                var lsOrderID = db.TBL_CT_ORDER_SPR_TBL.Where(a => a.MARKER == TxtMARKER).Select(a => a.ORDER_TBL_ID).ToList();
                string FixCmpnt = (fc["ChkFix" + TxtMARKER.ToString()]);
                string Result = (fc["RdoResult" + TxtMARKER.ToString()]);
                foreach (var OrderID in lsOrderID)
                {
                    if (db.GetModuleByMidUser(userLogin.Username, 60).SingleOrDefault() == null)
                    {
                        var Ispassed = db.TBL_CT_CUTTING_PROCESS_ST
                            .Where(t => t.ORDER_TBL_ID == OrderID && t.PROCESS_ID == 30 && t.STATUS_ID == 50)
                            .FirstOrDefault();
                        if (Ispassed != null)
                            break;
                    }

                    if (Result != null && Utilities.ConvertStringToInt(Result) == 1)
                    {

                        for (int j = 1; j <= 6; j++)
                        {
                            string Check = fc["ChkCmpnt" + TxtMARKER + "-" + j];

                            if (Check != null)
                            {
                                //foreach(var OrderID in lsOrderID) {
                                InsertQcCmpntSttTable(TxtMARKER, OrderID, j, 50);
                                // }
                            }
                        }

                        for (int j = 1; j <= 6; j++)
                        {
                            string Check = fc["ChkCmpnt" + TxtMARKER + "-" + j];

                            if (Check != null)
                            {
                                int f = Utilities.ConvertStringToInt(fc["TxtF" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtF" + TxtMARKER]);
                                var M = Utilities.ConvertStringToInt(fc["TxtM" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtM" + TxtMARKER]);
                                var L = Utilities.ConvertStringToInt(fc["TxtL" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtL" + TxtMARKER]);
                                var F_Qty = Utilities.ConvertStringToInt(fc["TxtFQty" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtFQty" + TxtMARKER]);
                                var M_Qty = Utilities.ConvertStringToInt(fc["TxtMQty" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtMQty" + TxtMARKER]);
                                var L_Qty = Utilities.ConvertStringToInt(fc["TxtLQty" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtLQty" + TxtMARKER]);
                                if (f != 0 || M != 0 || L != 0)
                                {
                                    // foreach(var OrderID in lsOrderID) {
                                    InsertQcCmpntDefect(TxtMARKER, OrderID, j, f, M, L, F_Qty, M_Qty, L_Qty);
                                    //  }
                                }

                                break;
                            }
                        }
                    }
                    else if (Result != null && Utilities.ConvertStringToInt(Result) == 0)
                    {
                        for (int j = 1; j <= 6; j++)
                        {
                            string Check = fc["ChkCmpnt" + TxtMARKER + "-" + j];
                            if (Check != null)
                            {
                                int f = Utilities.ConvertStringToInt(fc["TxtF" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtF" + TxtMARKER]);
                                var M = Utilities.ConvertStringToInt(fc["TxtM" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtM" + TxtMARKER]);
                                var L = Utilities.ConvertStringToInt(fc["TxtL" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtL" + TxtMARKER]);
                                var F_Qty = Utilities.ConvertStringToInt(fc["TxtFQty" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtFQty" + TxtMARKER]);
                                var M_Qty = Utilities.ConvertStringToInt(fc["TxtMQty" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtMQty" + TxtMARKER]);
                                var L_Qty = Utilities.ConvertStringToInt(fc["TxtLQty" + TxtMARKER] == ""
                                    ? "0"
                                    : fc["TxtLQty" + TxtMARKER]);

                                InsertQcCmpntDefect(TxtMARKER, OrderID, j, f, M, L, F_Qty, M_Qty, L_Qty);
                                InsertQcCmpntSttTable(TxtMARKER, OrderID, j, 60);


                                break;
                            }
                        }
                    }
                }

                if (FixCmpnt != null)
                {
                    var datenow = DateTime.Now;
                    foreach (var OrderID in lsOrderID)
                    {
                        var CompStt = db.PROC_GET_CT_DETAIL_CMPNT_STT_BY_SPRDTBLID(OrderID).ToList();
                        if (CompStt != null)
                        {
                            bool check = true;
                            foreach (var item in CompStt)
                            {
                                if (item.STATUS_ID == 60)
                                {
                                    check = false;
                                    break;
                                }
                            }



                            if (check == true)
                            {
                                InsertCtProcStatus(OrderID, 50, 30, datenow);
                                InsertOrderStatus((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.ORDER_TBL_ID == OrderID) select item.WO).SingleOrDefault(), TxtMARKER, 30, 30, datenow);
                            }
                            else
                            {
                                InsertCtProcStatus(OrderID, 60, 30, datenow);
                                InsertOrderStatus((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.ORDER_TBL_ID == OrderID) select item.WO).SingleOrDefault(), TxtMARKER, 60, 30, datenow);
                            }
                        }

                    }
                }
            }
            return RedirectToAction("ScanQCCmpntInsp");
        }

        public void InsertQcCmpntSttTable(string MARKER, int ORDER_TBL_ID, int cmptId, int status)
        {
            var qcCmpntRec = new TBL_CT_QC_COMPNT_STT
            {
                COMPNT_ID = cmptId,
                STATUS_ID = status,
                ORDER_TBL_ID = ORDER_TBL_ID,
                MARKER = MARKER,
                TS_1 = DateTime.Now,
                TS_1_USER = userLogin.Username
            };
            db.TBL_CT_QC_COMPNT_STT.Add(qcCmpntRec);
            db.SaveChanges();
        }

        public void InsertQcCmpntDefect(string MARKER, int ORDER_TBL_ID, int cmptId, int F, int M, int L, int F_Qty, int M_Qty, int L_Qty)
        {
            var qcCmpntRec = new TBL_CT_QC_COMPNT_DEFECT
            {
                MARKER = MARKER,
                ORDER_TBL_ID = ORDER_TBL_ID,
                COMPNT_ID = cmptId,
                F = F,
                M = M,
                L = L,
                F_QTY = F_Qty,
                M_QTY = M_Qty,
                L_QTY = L_Qty,
                TS_1 = DateTime.Now,
                TS_1_USER = userLogin.Username
            };

            db.TBL_CT_QC_COMPNT_DEFECT.Add(qcCmpntRec);
            db.SaveChanges();
        }

        public ActionResult ScanQcFinConf()
        {

            Session["Count_FinConf"] = 0;
            Session["AllFinConfView"] = new List<ViewFinConfModel>();
            return View();
        }

        public ActionResult ShowQCFinConf(string re, string wo, string qt, string DefectCode)
        {

            //   var cutpart = cu;
            // var WaistBind = wb;
            // var LegBind = lb;
            var result = re;
            var WO = wo != null ? Stand_WO(wo) : "";
            var Qty = qt;
            var DefCode = DefectCode;
            // var DefQty = dq;
            var count = Utilities.ConvertStringToInt(Session["Count_FinConf"].ToString());

            List<ViewFinConfModel> AllFinConfView = (List<ViewFinConfModel>)(Session["AllFinConfView"]);
            ViewFinConfModel FinConfView = new ViewFinConfModel();

            FinConfView.WO = WO;
            FinConfView.Quantity = Utilities.ConvertStringToInt(Qty);

            if (result == "1")
            {
                bool check = true;
                double PlanQty = Convert.ToDouble((from item in db.TBL_CT_PLAN.Where(t => t.WO == WO) select item.QUANTITY).SingleOrDefault());
                TBL_CT_ORDER_ST TmpMaxSprd = db.TBL_CT_ORDER_ST.Where(t => t.WO == WO).OrderByDescending(t => t.TS_1).FirstOrDefault();
                if (TmpMaxSprd == null)
                {
                    FinConfView.Status = "Lỗi, WO chưa được cắt !";
                    check = false;
                }
                else if (db.TBL_CT_ORDER_ST.Where(t => t.WO == WO && t.PROCESS_ID == 40 && t.STATUS_ID == 50).FirstOrDefault() != null)
                {
                    FinConfView.Status = "Douplicate, Đã được pass !";
                    check = false;
                }
                //else if (CheckIsEnoughCmpnt(WO, WaistBind, LegBind) == false)
                //{
                //    FinConfView.Status = "Không đủ chi tiết !";
                //    check = false;
                //}
                else if (!CheckIsEnoughQty(WO, Convert.ToDouble(Qty), 30))
                {
                    FinConfView.Status = "Vượt quá số lượng được Pass !";
                    check = false;
                }
                else if (PlanQty <= 10 && (Convert.ToDouble(Qty) > (PlanQty * 1.9) || Convert.ToDouble(Qty) < (PlanQty * 0.8)))
                {
                    FinConfView.Status = "Số lượng không khớp với kế hoạch " + (Convert.ToDouble(Qty) - PlanQty) + " Dz !";
                    check = false;
                }
                else if (PlanQty <= 20 && PlanQty > 10 && (Convert.ToDouble(Qty) > (PlanQty * 1.4) || Convert.ToDouble(Qty) < (PlanQty * 0.8)))
                {
                    FinConfView.Status = "Số lượng không khớp với kế hoạch " + (Convert.ToDouble(Qty) - PlanQty) + " Dz !";
                    check = false;
                }
                else if (PlanQty > 20 && PlanQty <= 50 && (Convert.ToDouble(Qty) > (PlanQty * 1.3) || Convert.ToDouble(Qty) < (PlanQty * 0.8)))
                {
                    FinConfView.Status = "Số lượng không khớp với kế hoạch " + (Convert.ToDouble(Qty) - PlanQty) + " Dz !";
                    check = false;
                }
                else if (PlanQty > 50 && (Convert.ToDouble(Qty) > (PlanQty * 1.1) || Convert.ToDouble(Qty) < (PlanQty * 0.9)))
                {
                    FinConfView.Status = "Số lượng không khớp với kế hoạch " + (Convert.ToDouble(Qty) - PlanQty) + " Dz !";
                    check = false;
                }
                if (check == true)
                {
                    InsertOrderStatus(WO, TmpMaxSprd.MARKER, 50, 40, DateTime.Now);
                    InsertQCTblFinalConfSTT(WO, Convert.ToDouble(Qty), 50);
                    FinConfView.Status = "OK !";
                    count += 1;
                    CheckAndConfFullAsst(WO);
                }

                //AllFinConfView.Add(FinConfView);
            }
            else if (result == "0")
            {
                TBL_CT_ORDER_ST TmpMaxSprd = db.TBL_CT_ORDER_ST.Where(t => t.WO == wo && t.PROCESS_ID == 40).OrderByDescending(t => t.TS_1).FirstOrDefault();
                if (db.TBL_CT_ORDER_ST.Where(t => t.WO == WO && t.PROCESS_ID == 40 && t.STATUS_ID == 50).FirstOrDefault() != null)
                {
                    FinConfView.Status = "Lỗi, đã được Pass !";
                }
                // CHECK WL IS EXIST
                else if (db.TBL_CT_ORDER_ST.Where(t => t.WO == WO).FirstOrDefault() == null)
                {
                    FinConfView.Status = "Lỗi, WL không tồn tại !";
                }
                else
                {
                    InsertOrderStatus(WO, TmpMaxSprd.MARKER, 60, 40, DateTime.Now);
                    InsertQCTblFinalConfSTT(WO, Convert.ToDouble(Qty), 60);
                    //if (WaistBind == "1")
                    //    InsertQCTblFinalConfDefect(WO, Utilities.ConvertStringToInt(DefQty), DefCode, 3);
                    //else if (LegBind == "1")
                    //    InsertQCTblFinalConfDefect(WO, Utilities.ConvertStringToInt(DefQty), DefCode, 2);
                    //else if (cutpart == "1")
                    //InsertQCTblFinalConfDefect(WO, Utilities.ConvertStringToInt(DefQty), DefCode, 1);
                    InsertQCTblFinalConfDefect(WO, Utilities.ConvertStringToInt("0"), DefCode, 4);
                    FinConfView.Status = "Rejected chi tiết !";
                    count += 1;
                }
            }
            AllFinConfView.Add(FinConfView);
            Session["AllFinConfView"] = AllFinConfView;
            Session["Count_FinConf"] = count;
            ViewBag.Status = FinConfView.Status;

            return PartialView("_ShowQCFinConf", AllFinConfView);
        }

        public void InsertQCTblFinalConfSTT(string WO, double Qty, int status)
        {
            TBL_CT_ORDER_ST TmpMaxPassOrder = db.TBL_CT_ORDER_ST.Where(t => t.WO == WO && t.STATUS_ID == status).OrderByDescending(t => t.TS_1).FirstOrDefault();
            TBL_CT_QC_FINAL_CONF_STT FinConf = new TBL_CT_QC_FINAL_CONF_STT();
            FinConf.ORDER_STT_ID = TmpMaxPassOrder.ID;
            FinConf.QUANTITY = Qty;
            FinConf.STATUS_ID = status;
            FinConf.WO = WO;
            FinConf.TS_1 = DateTime.Now;
            FinConf.TS_1_USER = (UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username;
            db.TBL_CT_QC_FINAL_CONF_STT.Add(FinConf);
            db.SaveChanges();
        }

        public void InsertQCTblFinalConfDefect(string WO, int DefQty, string DefCode, int PartType)
        {
            TBL_CT_QC_FINAL_CONF_STT TmpMaxRejFinConf = db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == WO && t.STATUS_ID == 60).OrderByDescending(t => t.TS_1).FirstOrDefault();
            TBL_CT_QC_FINAL_CONF_DEFECT FinConf = new TBL_CT_QC_FINAL_CONF_DEFECT();
            FinConf.COMPNT_TYPE = PartType;
            FinConf.QUANTITY = Utilities.ConvertStringToDouble(DefQty.ToString());
            FinConf.DEFECT_CODE = DefCode;
            FinConf.FIN_CONF_STT_ID = TmpMaxRejFinConf.ID;
            FinConf.TS_1 = DateTime.Now;
            FinConf.TS_1_USER = (UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username;
            db.TBL_CT_QC_FINAL_CONF_DEFECT.Add(FinConf);
            db.SaveChanges();
        }

        public bool CheckIsEnoughCmpnt(string Wo, string waitsB, string legB)
        {
            bool isEnoughCut = true;
            bool isEnouhtBind = true;
            List<PROC_GET_CT_LAST_CMPNT_STT_BY_ORDER_Result> ComponentStt = db.GetCTLastCmpntSttByOrder(Wo).ToList();
            List<PROC_GET_CT_CMPNT_BY_WO_Result> WOCmpnt = db.GetCTCmpntByWO(Wo).ToList();

            foreach (var item in WOCmpnt)
            {
                if (item.TYPE == 1)
                {
                    var check2 = false;
                    foreach (var item2 in ComponentStt)
                    {

                        if (item2.COMPNT_ID == item.CMPNT_ID && item2.STATUS_ID == 50) { check2 = true; break; }
                        check2 = false;
                    }
                    if (check2 == false) { isEnoughCut = false; break; }
                }

            }
            foreach (var item in WOCmpnt)
            {
                if ((item.CMPNT_ID == 7 && legB == "0") || (item.CMPNT_ID == 8 && waitsB == "0"))
                    isEnouhtBind = false;
            }

            if (isEnoughCut == true && isEnouhtBind == true)
            {
                return true;
            }
            return false;
        }

        public bool CheckIsEnoughQty(string Wo, double OrderQty, int proc)
        {
            List<PROC_GET_CT_LAST_CUTTING_PRC_ST_BY_ORDER_Result> LastCmpntStt = db.GetCTLastCuttingPrcStByOrder(Wo).ToList();
            List<PROC_GET_CT_LAST_CUTTING_PRC_ST_BY_ORDER_Result> itemremove = LastCmpntStt.Where(r => r.PROCESS_ID != proc).ToList();
            foreach (var item in itemremove) { LastCmpntStt.Remove(item); }


            double qty = 0;
            double type2 = 0;
            double type3 = 0;

            foreach (var item in LastCmpntStt)
            {
                if (item.CUTTING_TYPE == 1 && item.STATUS_ID == 50)
                {
                    type2 += Convert.ToDouble(item.QUANTITY);
                    type3 += Convert.ToDouble(item.QUANTITY);
                }
                else if (item.CUTTING_TYPE == 2 && item.STATUS_ID == 50)
                {
                    type2 += Convert.ToDouble(item.QUANTITY);
                }
                else if (item.CUTTING_TYPE == 3 && item.STATUS_ID == 50)
                {
                    type3 += Convert.ToDouble(item.QUANTITY);
                }
            }
            qty = (type2 > type3 ? type3 : type2);
            return OrderQty <= qty;
        }

        public void CheckAndConfFullAsst(string WO)
        {
            string Asst = (from item in db.TBL_CT_PLAN.Where(t => t.WO == WO) select item.ASST).FirstOrDefault();
            List<String> ListTTSOrder = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.ASST == Asst) select item.WO).ToList();
            var check = true;
            foreach (var item in ListTTSOrder)
            {
                var status = (from item2 in db.TBL_CT_ORDER_ST.Where(t => t.WO == item && t.PROCESS_ID == 40 && t.STATUS_ID == 50) select item2.STATUS_ID).FirstOrDefault();
                if (status == null) { check = false; break; }
            }

            if (check == true)
            {
                foreach (var item in ListTTSOrder)
                {
                    var order = (from item2 in db.TBL_CT_ORDER_ST.Where(t => t.WO == item && t.PROCESS_ID == 40 && t.STATUS_ID == 50) select item2).FirstOrDefault();
                    InsertOrderStatus(item, order.MARKER, 70, 40, DateTime.Now);
                }
            }
        }

        public ActionResult ScanTransfWH()
        {

            Session["Count_TransfWH"] = 0;
            Session["TransfWH"] = new List<ViewTransWHModel>();
            return View();
        }

        public ActionResult ShowTransfWH(string WOData)
        {

            if (WOData == null)
                return PartialView("_ShowTransfWH");
            List<ViewTransWHModel> ListTransfWH = (List<ViewTransWHModel>)(Session["TransfWH"]);
            ViewTransWHModel TransfWH = new ViewTransWHModel();
            var tmp = WOData.Split('-');
            var WO = Stand_WO(tmp[0]);
            var Qty = Convert.ToDouble(tmp[1]);
            var marker = tmp.Length > 2 ? tmp[2] : "N" + tmp[0];

            TransfWH.WO = WO;
            TransfWH.Quantity = Convert.ToDouble(Qty);
            int count = Utilities.ConvertStringToInt(Session["Count_TransfWH"].ToString());
            double QCconfirmedQty = GetQcFinalConfSttQty(WO, 50);
            double TransferedQty = Convert.ToDouble((from item in db.GetCTRptWOTransfWHQty(WO) select item.TOTAL_QTY).SingleOrDefault());
            if (!CheckIsExistOrder(WO))
            {
                TransfWH.Status = "Lỗi, Order không tồn tại !";
            }
            else if (QCconfirmedQty == 0)
            {
                TransfWH.Status = "Lỗi, Chưa được QC Pass !";
            }
            else if ((Qty + TransferedQty) > QCconfirmedQty)
            {
                TransfWH.Status = "Lỗi, Vượt quá số lượng QC Pass (" + (Qty + TransferedQty) + " > " + QCconfirmedQty + ") !";
            }
            else
            {
                TBL_CT_TRANSFER_WH TransfRecord = new TBL_CT_TRANSFER_WH();
                TransfRecord.QUANTITY = Qty;
                TransfRecord.WO = WO;
                TransfRecord.TS_1 = DateTime.Now;
                TransfRecord.TS_1_USER = userLogin.Username;
                db.TBL_CT_TRANSFER_WH.Add(TransfRecord);
                db.SaveChanges();
                count += 1;
                TransfWH.Status = "OK !";
                InsertOrderStatus(WO, marker, 80, 80, DateTime.Now);
            }
            ListTransfWH.Add(TransfWH);
            Session["Count_TransfWH"] = count;
            ViewBag.Status = TransfWH.Status;
            // SHO DATA TO VIEW
            Session["TransfWH"] = ListTransfWH;
            return PartialView("_ShowTransfWH", ListTransfWH);
        }

        public double GetQcFinalConfSttQty(string Wo, int status)
        {
            TBL_CT_QC_FINAL_CONF_STT FinalConf = db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == Wo && t.STATUS_ID == status).FirstOrDefault();
            if (FinalConf != null)
            {
                return Convert.ToDouble(FinalConf.QUANTITY);
            }
            return 0;
        }

        public bool CheckIsExistOrder(string WO)
        {
            return db.TBL_CT_PLAN.Where(t => t.WO == WO).SingleOrDefault() != null;

        }

        public ActionResult test()
        {
            return View();

        }

        public ActionResult ExporttoExcel(FormCollection fc)
        {
            var Criterial = fc["TxtCri"];
            var OptType = fc["TxtOpt"];
            DateTime Fromdate = Convert.ToDateTime(fc["TxtFrom"]); //Convert.ToDateTime("02/26/2017");
            DateTime Todate = Convert.ToDateTime(fc["TxtTo"]); //DateTime.Now; //fc[""];
            //List<PROC_GET_TOTAL_SAH_BY_DATE_Result> TotalSah = (from item in db.GetTotalSahByDate(From_date, To_date) select item).ToList();
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

            List<ViewCTOutputDetail> ListOutputDetail = new List<ViewCTOutputDetail>();
            if (Criterial == "1")
            {
                ListOutputDetail = db.GetCTRptPlanByRangeDate(Fromdate, Todate).Select(x => new ViewCTOutputDetail
                {
                    WO = x.WO,
                    Assortment = x.ASST,
                    MfgStyle = x.MFG_STYLE,
                    SellingStyle = x.SELLING_STYLE,
                    Garment = x.GARMENT_STYLE,
                    color = x.GARMENT_COLOR,
                    Business = (from item in db.GetWCInforForOneSelling(x.SELLING_STYLE) select item.UNIT).SingleOrDefault(),
                    FabricCode = x.FABRIC_CODE,
                    FabricCode2 = x.FABRIC_CODE_2,
                    Size = x.SIZE,
                    Quantity = Convert.ToDouble(x.QUANTITY),
                    PlanDate = Convert.ToDateTime(x.CUT_PLAN_DATE),
                    TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x.WO) select item.STATUS).SingleOrDefault(),
                    Status = new CTRptModuleController().StatusIDtoName(Utilities.ConvertStringToInt((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault().ToString())),
                    Fabric_Recieved = Convert.ToDateTime((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x.WO && (t.STATUS_ID == 10 || t.STATUS_ID == 11)).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                    WipDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                    CutDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_2) select item.TS_2).FirstOrDefault()),
                    CutBody = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x.WO, 2) select item.TOTAL_QTY).SingleOrDefault()),
                    CutLiner = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x.WO, 3) select item.TOTAL_QTY).SingleOrDefault()),
                    Produced = Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x.WO && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                    TransferWH = Convert.ToDouble((from item in db.GetCTRptWOTransfWHQty(x.WO) select item.TOTAL_QTY).SingleOrDefault()),
                    TransferDate = Convert.ToDateTime((from item in db.TBL_CT_TRANSFER_WH.Where(t => t.WO == x.WO).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                    Discrapancy = Convert.ToDouble(x.QUANTITY) - Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x.WO && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                    FullAsst = db.TBL_CT_ORDER_ST.Where(t => t.STATUS_ID == 70 && t.WO == x.WO).SingleOrDefault() == null ? "" : "Full"
                }).ToList();
            }
            else
            {
                if (OptType == "1")
                {
                    ListOutputDetail = db.GetCTRptProdByRangeDate(Fromdate, Todate).Select(x => new ViewCTOutputDetail
                    {
                        WO = x,
                        Assortment = (from item in db.GetCTRptPlanByWO(x) select item.ASST).SingleOrDefault(),
                        MfgStyle = (from item in db.GetCTRptPlanByWO(x) select item.MFG_STYLE).SingleOrDefault(),
                        SellingStyle = (from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault(),
                        Garment = (from item in db.GetCTRptPlanByWO(x) select item.GARMENT_STYLE).SingleOrDefault(),
                        GarmentColor = (from item in db.GetCTRptPlanByWO(x) select item.GARMENT_COLOR).SingleOrDefault(),
                        Business = (from item in db.GetWCInforForOneSelling((from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault()) select item.UNIT).SingleOrDefault(),
                        FabricCode = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE).SingleOrDefault(),
                        FabricCode2 = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE_2).SingleOrDefault(),
                        Size = (from item in db.GetCTRptPlanByWO(x) select item.SIZE).SingleOrDefault(),
                        Quantity = Convert.ToDouble((from item in db.GetCTRptPlanByWO(x) select item.QUANTITY).SingleOrDefault()),
                        PlanDate = Convert.ToDateTime((from item in db.GetCTRptPlanByWO(x) select item.CUT_PLAN_DATE).SingleOrDefault()),
                        TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x) select item.STATUS).SingleOrDefault(),
                        Status = new CTRptModuleController().StatusIDtoName(Utilities.ConvertStringToInt((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault().ToString())),
                        Fabric_Recieved = Convert.ToDateTime((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x && (t.STATUS_ID == 10 || t.STATUS_ID == 11)).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        WipDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        CutDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x).OrderByDescending(t => t.TS_2) select item.TS_2).FirstOrDefault()),
                        CutBody = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x, 2) select item.TOTAL_QTY).SingleOrDefault()),
                        CutLiner = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x, 3) select item.TOTAL_QTY).SingleOrDefault()),
                        Produced = Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                        TransferWH = Convert.ToDouble((from item in db.GetCTRptWOTransfWHQty(x) select item.TOTAL_QTY).SingleOrDefault()),
                        TransferDate = Convert.ToDateTime((from item in db.TBL_CT_TRANSFER_WH.Where(t => t.WO == x).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        Discrapancy = Convert.ToDouble(Convert.ToDouble((from item in db.GetCTRptPlanByWO(x) select item.QUANTITY).SingleOrDefault())) - Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                        FullAsst = db.TBL_CT_ORDER_ST.Where(t => t.STATUS_ID == 70 && t.WO == x).SingleOrDefault() == null ? "" : "Full"
                    }).ToList();
                }
                else
                {
                    ListOutputDetail = db.GetCtRptProdByFabSprdRangeDate(Fromdate, Todate).Select(x => new ViewCTOutputDetail
                    {
                        WO = x,
                        Assortment = (from item in db.GetCTRptPlanByWO(x) select item.ASST).SingleOrDefault(),
                        MfgStyle = (from item in db.GetCTRptPlanByWO(x) select item.MFG_STYLE).SingleOrDefault(),
                        SellingStyle = (from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault(),
                        Business = (from item in db.GetWCInforForOneSelling((from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault()) select item.UNIT).SingleOrDefault(),
                        Garment = (from item in db.GetCTRptPlanByWO(x) select item.GARMENT_STYLE).SingleOrDefault(),
                        GarmentColor = (from item in db.GetCTRptPlanByWO(x) select item.GARMENT_COLOR).SingleOrDefault(),
                        FabricCode = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE).SingleOrDefault(),
                        FabricCode2 = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE_2).SingleOrDefault(),
                        Size = (from item in db.GetCTRptPlanByWO(x) select item.SIZE).SingleOrDefault(),
                        Quantity = Convert.ToDouble((from item in db.GetCTRptPlanByWO(x) select item.QUANTITY).SingleOrDefault()),
                        PlanDate = Convert.ToDateTime((from item in db.GetCTRptPlanByWO(x) select item.CUT_PLAN_DATE).SingleOrDefault()),
                        TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x) select item.STATUS).SingleOrDefault(),
                        Status = new CTRptModuleController().StatusIDtoName(Utilities.ConvertStringToInt((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault().ToString())),
                        Fabric_Recieved = Convert.ToDateTime((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x && (t.STATUS_ID == 10 || t.STATUS_ID == 11)).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        WipDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        CutDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x).OrderByDescending(t => t.TS_2) select item.TS_2).FirstOrDefault()),
                        CutBody = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x, 2) select item.TOTAL_QTY).SingleOrDefault()),
                        CutLiner = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x, 3) select item.TOTAL_QTY).SingleOrDefault()),
                        Produced = Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                        TransferWH = Convert.ToDouble((from item in db.GetCTRptWOTransfWHQty(x) select item.TOTAL_QTY).SingleOrDefault()),
                        TransferDate = Convert.ToDateTime((from item in db.TBL_CT_TRANSFER_WH.Where(t => t.WO == x).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        Discrapancy = Convert.ToDouble(Convert.ToDouble((from item in db.GetCTRptPlanByWO(x) select item.QUANTITY).SingleOrDefault())) - Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                        FullAsst = db.TBL_CT_ORDER_ST.Where(t => t.STATUS_ID == 70 && t.WO == x).SingleOrDefault() == null ? "" : "Full"
                    }).ToList();
                }
            }

            ListOutputDetail = ListOutputDetail.OrderBy(o => o.WO).ToList();
            workSheet.Cells[1, 1].LoadFromCollection(ListOutputDetail, true);

            using (ExcelRange col = workSheet.Cells[1, 1, ListOutputDetail.Count + 1, 24])
            {
                col.AutoFitColumns();
            }

            using (ExcelRange col = workSheet.Cells[2, 12, ListOutputDetail.Count + 1, 12])
            {
                col.Style.Numberformat.Format = "dd/MM";
                col.AutoFitColumns();
                //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            using (ExcelRange col = workSheet.Cells[2, 14, ListOutputDetail.Count + 1, 14])
            {
                col.Style.Numberformat.Format = "dd/MM HH:mm";
                col.AutoFitColumns();
                //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            using (ExcelRange col = workSheet.Cells[2, 15, ListOutputDetail.Count + 1, 15])
            {
                col.Style.Numberformat.Format = "dd/MM HH:mm";
                col.AutoFitColumns();
                //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            using (ExcelRange col = workSheet.Cells[2, 16, ListOutputDetail.Count + 1, 16])
            {
                col.Style.Numberformat.Format = "dd/MM HH:mm";
                col.AutoFitColumns();
                //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            using (ExcelRange col = workSheet.Cells[2, 21, ListOutputDetail.Count + 1, 21])
            {
                col.Style.Numberformat.Format = "dd/MM HH:mm";
                col.AutoFitColumns();
                //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            workSheet.Cells["A1:X1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:X1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet.Cells["A1:X1"].Style.Font.Bold = true;
            workSheet.Cells["A1:X1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet.Cells["A1:X1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + DateTime.Now.ToLongTimeString() + ".xlsx");

                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return RedirectToAction("index");
        }
    }
}
