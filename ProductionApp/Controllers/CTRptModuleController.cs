using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;
using System.Globalization;


namespace ProductionApp.Controllers
{
    public class CTRptModuleController:BaseController
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OutputDetail(FormCollection fc)
        {
            List<ViewCTOutputDetail> ListOutputDetail = new List<ViewCTOutputDetail>();
            var FromDate = fc["txtFrom"];
            var ToDate = fc["txtTo"];
            var Criterial = fc["RdoCriterial"];
            var OptType = fc["OptType"];

            if (FromDate != null && ToDate != null && Criterial!= null)
            {
                DateTime From = Convert.ToDateTime(FromDate);
                DateTime To = Convert.ToDateTime(ToDate);
                // sort by Plan
                if (Criterial=="1")
                {
                    ListOutputDetail = db.GetCTRptPlanByRangeDate(From, To).Select(x => new ViewCTOutputDetail
                    {
                        WO = x.WO,
                        Assortment = x.ASST,
                        MfgStyle = x.MFG_STYLE,
                        SellingStyle = x.SELLING_STYLE,
                        Business = (from item in db.GetWCInforForOneSelling(x.SELLING_STYLE) select item.UNIT).SingleOrDefault(),
                        FabricCode = x.FABRIC_CODE,
                        FabricCode2 = x.FABRIC_CODE_2,
                        Size = x.SIZE,
                        Quantity = Convert.ToDouble(x.QUANTITY),
                        PlanDate = Convert.ToDateTime(x.CUT_PLAN_DATE),
                        TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x.WO) select item.STATUS).SingleOrDefault(),
                        Status = StatusIDtoName( Convert.ToInt16((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault() )),
                        Fabric_Recieved = Convert.ToDateTime((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x.WO && (t.STATUS_ID == 10 || t.STATUS_ID == 11)).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        WipDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                        CutDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_2) select item.TS_2).FirstOrDefault()),
                        CutBody = Convert.ToDouble((from item in db.GetCTRptSprdQtyByWoCtype(x.WO, 2) select item.TOTAL_QTY).SingleOrDefault()),
                        CutLiner = Convert.ToDouble( (from item in db.GetCTRptSprdQtyByWoCtype(x.WO, 3) select item.TOTAL_QTY).SingleOrDefault()),
                        Produced = Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x.WO && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                        TransferWH = Convert.ToDouble((from item in db.GetCTRptWOTransfWHQty(x.WO) select item.TOTAL_QTY).SingleOrDefault()),
                        TransferDate = Convert.ToDateTime((from item in db.TBL_CT_TRANSFER_WH.Where(t => t.WO == x.WO).OrderBy(t=> t.TS_1) select item.TS_1).FirstOrDefault()),
                        Discrapancy = Convert.ToDouble(x.QUANTITY) - Convert.ToDouble((from item in db.TBL_CT_QC_FINAL_CONF_STT.Where(t => t.WO == x.WO && t.STATUS_ID == 50) select item.QUANTITY).FirstOrDefault()),
                        FullAsst = db.TBL_CT_ORDER_ST.Where(t => t.STATUS_ID == 70 && t.WO == x.WO).SingleOrDefault() == null ? "" : "Full"
                    }).ToList();
                }
                // sort by production
                else
                {
                    if (OptType == "1")
                    {
                        ListOutputDetail = db.GetCTRptProdByRangeDate(From, To).Select(x => new ViewCTOutputDetail
                        {
                            WO = x,
                            Assortment = (from item in db.GetCTRptPlanByWO(x) select item.ASST).SingleOrDefault(),
                            MfgStyle = (from item in db.GetCTRptPlanByWO(x) select item.MFG_STYLE).SingleOrDefault(),
                            SellingStyle = (from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault(),
                            Business = (from item in db.GetWCInforForOneSelling((from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault()) select item.UNIT).SingleOrDefault(),
                            FabricCode = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE).SingleOrDefault(),
                            FabricCode2 = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE_2).SingleOrDefault(),
                            Size = (from item in db.GetCTRptPlanByWO(x) select item.SIZE).SingleOrDefault(),
                            Quantity = Convert.ToDouble((from item in db.GetCTRptPlanByWO(x) select item.QUANTITY).SingleOrDefault()),
                            PlanDate = Convert.ToDateTime((from item in db.GetCTRptPlanByWO(x) select item.CUT_PLAN_DATE).SingleOrDefault()),
                            TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x) select item.STATUS).SingleOrDefault(),
                            Status = StatusIDtoName(Convert.ToInt16((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault())),
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
                        ListOutputDetail = db.GetCtRptProdByFabSprdRangeDate(From, To).Select(x => new ViewCTOutputDetail
                        {
                            WO = x,
                            Assortment = (from item in db.GetCTRptPlanByWO(x) select item.ASST).SingleOrDefault(),
                            MfgStyle = (from item in db.GetCTRptPlanByWO(x) select item.MFG_STYLE).SingleOrDefault(),
                            SellingStyle = (from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault(),
                            Business = (from item in db.GetWCInforForOneSelling((from item in db.GetCTRptPlanByWO(x) select item.SELLING_STYLE).SingleOrDefault()) select item.UNIT).SingleOrDefault(),
                            FabricCode = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE).SingleOrDefault(),
                            FabricCode2 = (from item in db.GetCTRptPlanByWO(x) select item.FABRIC_CODE_2).SingleOrDefault(),
                            Size = (from item in db.GetCTRptPlanByWO(x) select item.SIZE).SingleOrDefault(),
                            Quantity = Convert.ToDouble((from item in db.GetCTRptPlanByWO(x) select item.QUANTITY).SingleOrDefault()),
                            PlanDate = Convert.ToDateTime((from item in db.GetCTRptPlanByWO(x) select item.CUT_PLAN_DATE).SingleOrDefault()),
                            TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x) select item.STATUS).SingleOrDefault(),
                            Status = StatusIDtoName(Convert.ToInt16((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault())),
                            Fabric_Recieved = Convert.ToDateTime((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x && (t.STATUS_ID == 10|| t.STATUS_ID == 11)).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
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
            }
            // CACULATE TOTAL TO SHOW
            ViewCTOutputTotal OutputTotal = new ViewCTOutputTotal();
            foreach (var item in ListOutputDetail)
            {
                    OutputTotal.TotalProduced += item.Produced;
                    OutputTotal.TotalTransWH += item.TransferWH;
                    OutputTotal.TotalType1 += item.CutBody;
                    OutputTotal.TotalType2 += item.CutLiner;
                    OutputTotal.TotalMismatch += item.Discrapancy;
            }
            if(FromDate != null && ToDate != null)
            {
                DateTime From = Convert.ToDateTime(FromDate);
                DateTime To = Convert.ToDateTime(ToDate);
                OutputTotal.TotalPlan = Convert.ToDouble(db.GetCtRptPlanQtyByRangeDate(From, To).FirstOrDefault());
                OutputTotal.TotalFabRcv = Convert.ToDouble(db.GetCtRptFabrcvQtyByRangeDate(From, To).FirstOrDefault());
            }
            ViewBag.OutputTotal = OutputTotal;

            PostbackValue PostBack = new PostbackValue();
            PostBack.num = Convert.ToInt16(Criterial)==0?2: Convert.ToInt16(Criterial);
            PostBack.num2 = Convert.ToInt16(OptType);
            ViewBag.PostBack = PostBack;

            PROC_GET_LAST_TTS_UPDATE_Result LastTTS = db.GetLastTTSUpdate().SingleOrDefault();
            ViewBag.LastTTSUpdate = LastTTS;
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
            ViewBag.Cri = Criterial;
            ViewBag.Opt = OptType;

            return View("OutputDetail", ListOutputDetail);
        }

        public ActionResult DefectTracking(FormCollection fc)
        {
            List<ViewQCDefTrackingModel> ListDefectTracking = new List<ViewQCDefTrackingModel>();
            var FromDate = fc["txtFrom"];
            var ToDate = fc["txtTo"];

            if (FromDate == null || ToDate == null)
                return View("DefectTracking");

            DateTime From = Convert.ToDateTime(FromDate);
            DateTime To = Convert.ToDateTime(ToDate);

            ListDefectTracking = db.GetCTRptProdByRangeDate(From, To).Select(x => new ViewQCDefTrackingModel {
                WO = x,
                SpreadDate = Convert.ToDateTime((from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == x).OrderByDescending(t => t.TS_1) select item.TS_1).FirstOrDefault()),
                FinalInsp = db.GetCTRptFinConfDefectByOrder(x).Select(f => new ViewFinalInspDefModel { Qty = Convert.ToDouble(f.QUANTITY), Date = Convert.ToDateTime(f.TS_1), DefCode = f.DEFECT_CODE}).ToList(),
                TblDefect = db.GetCTRptCmpntDefectByOrder(x).Select(f => new ViewTableDefectModel {  type = Convert.ToInt16(f.CUTTING_TYPE), Date = Convert.ToDateTime(f.TS_1), F = Convert.ToInt16(f.F), M = Convert.ToInt16(f.M), L = Convert.ToInt16(f.L), F_Qty = Convert.ToInt16(f.F_QTY), M_Qty = Convert.ToInt16(f.M_QTY), L_Qty = Convert.ToInt16(f.L_QTY) }).ToList(),
            }).ToList();

            var Removeitem = ListDefectTracking.Where(t => t.FinalInsp.Count == 0 & t.TblDefect.Count == 0).ToList();
            foreach(var item in Removeitem)
            { ListDefectTracking.Remove(item); }
            
         return View("DefectTracking", ListDefectTracking);
        }

        public ActionResult QCOverview(FormCollection fc)
        {
            if (fc["txtFrom"] == null) return View("QCOverview");
            DateTime from_date = Convert.ToDateTime(fc["txtFrom"]).Date;
            DateTime to_date = Convert.ToDateTime(fc["txtTo"]).Date;

            List<EndlineTotalView> EnlineTotal = db.Get_All_Business().Select(x => new EndlineTotalView()
            {
                Business = x.BIZ_NAME,
                Total_Defect = Convert.ToInt64((from item in db.GetCTQCBizTotalDefect(from_date, to_date, x.ID) select item.TOTAL_QTY).SingleOrDefault()),
                Total_Sample = Convert.ToInt64((from item in db.GetCTQCBizTotalSample(from_date, to_date, x.ID) select item.TOTAL_QTY).SingleOrDefault()),
                
                target = Convert.ToDouble((from item in db.TBL_QC_BUSINESS_TARGET.Where(t => t.BIZ_ID == x.ID) select item.TARGET_2).SingleOrDefault()),

            }).ToList();

            List<CTQCEndlineFMLDefectCodeQty> AllBusinessTop5 = new List<CTQCEndlineFMLDefectCodeQty>();
            List<PROC_GET_ALL_BISINESS_Result> AllBusiness = db.Get_All_Business().ToList();
            foreach(var item in AllBusiness)
            {
                List<PROC_GET_CT_QC_BIZ_F_DEFECT_CODE_QTY_Result> AllF = db.GetCTQCBizFDefectCodeQty(from_date, to_date, item.ID).ToList();
                List<PROC_GET_CT_QC_BIZ_M_DEFECT_CODE_QTY_Result> AllM = db.GetCTQCBizMDefectCodeQty(from_date, to_date, item.ID).ToList();
                List<PROC_GET_CT_QC_BIZ_L_DEFECT_CODE_QTY_Result> AllL = db.GetCTQCBizLDefectCodeQty(from_date, to_date, item.ID).ToList();
                List<CTQCEndlineFMLDefectCodeQty> AllDefectFML = new List<CTQCEndlineFMLDefectCodeQty>();
                foreach(var f in AllF)
                {
                    CTQCEndlineFMLDefectCodeQty DefectFML = new CTQCEndlineFMLDefectCodeQty();
                    DefectFML.Business = f.BIZ_NAME;
                    DefectFML.BusinessID = Convert.ToInt16(f.ID);
                    DefectFML.Defect_Code = Convert.ToInt16(f.F);
                    DefectFML.Qty = Convert.ToInt16(f.TOTAL_QTY);
                    AllDefectFML.Add(DefectFML);
                }
                foreach (var m in AllM)
                {
                    CTQCEndlineFMLDefectCodeQty DefectFML = new CTQCEndlineFMLDefectCodeQty();
                    DefectFML.Business = m.BIZ_NAME;
                    DefectFML.BusinessID = m.ID;
                    DefectFML.Defect_Code = Convert.ToInt16(m.M);
                    DefectFML.Qty = Convert.ToInt16(m.TOTAL_QTY);
                    AllDefectFML.Add(DefectFML);
                }
                foreach (var L in AllL)
                {
                    CTQCEndlineFMLDefectCodeQty DefectFML = new CTQCEndlineFMLDefectCodeQty();
                    DefectFML.Business = L.BIZ_NAME;
                    DefectFML.BusinessID = L.ID;
                    DefectFML.Defect_Code = Convert.ToInt16(L.L);
                    DefectFML.Qty = Convert.ToInt16(L.TOTAL_QTY);
                    AllDefectFML.Add(DefectFML);
                }
                List<CTQCEndlineFMLDefectCodeQty> TOP5 = AllDefectFML.AsEnumerable().GroupBy(x => x.Defect_Code).Select(t => new CTQCEndlineFMLDefectCodeQty {

                    Business = t.First().Business,
                    BusinessID = t.First().BusinessID,
                    Defect_Code = t.First().Defect_Code,
                    Qty = t.Sum(s=> s.Qty)
                }).ToList();
                TOP5.OrderByDescending(t => t.Qty);
                TOP5 = (from t in TOP5 where t.Defect_Code != 0 select t).Take(5).ToList();
                AllBusinessTop5.AddRange(TOP5);
            }

            foreach(var item2 in AllBusinessTop5)
            {
                item2.Business_Total = Convert.ToInt16((from item in db.GetCTQCBizTotalDefect(from_date, to_date, item2.BusinessID) select item.TOTAL_QTY).SingleOrDefault());
                item2.Rate = (Convert.ToDouble(item2.Qty) / Convert.ToDouble(item2.Business_Total)) * 100;
                var code = Convert.ToString(item2.Defect_Code);
                item2.Defect_Name = (from item in db.TBL_CT_QC_DEFECT_MST.Where(t => t.DEFECT_ID == code) select item.DEFECT_NAME).SingleOrDefault();
            }
            ViewBag.AllBusinessTop5 = AllBusinessTop5;

            //List<ResultByType> result = dt.AsEnumerable().GroupBy(s => s.Field<string>("Type")).Select(sg => new ResultByType
            //{
            //    Type = sg.First().Field<string>("Type"),
            //    TotalScore2015 = sg.Sum(s => s.Field<int>("Score2015")),
            //    TotalScore2016 = sg.Sum(s => s.Field<int>("Score2016"))
            //}).ToList();

            List<EndlineWarningDefectBiz> Warning = db.GetQcDefectBiz(from_date, to_date).Select(x => new EndlineWarningDefectBiz()
            {
                Defect_Name = x.DEFECT_NAME,
                Business = x.BIZ_NAME,
                Rate = (Convert.ToDouble(x.TOTAL_QUANTITY) / Convert.ToDouble((from item in db.Get_QC_BIZ_Total_Defect_Sample(from_date, to_date, x.ID) select item.TOTAL_QTY).SingleOrDefault()) * 100),
            }).ToList();
            //for(int i = 0; i < Warning.Count; i++)
            //{
            //    if (Warning[i].Rate < 2)
            //    {
            //        Warning.RemoveAt(i);
            //    }
            //}
            Warning.OrderBy(t => t.Business).ThenBy(t => t.Rate);
            ViewBag.Warning = Warning;
            return View("QCOverview", EnlineTotal);
        }

        public ActionResult PlantOverview(FormCollection fc)
        {
            try
            {
                var abc = fc["txtFrom"].ToString();
                var abc1 = fc["txtFrom"];
            } catch{}
            
            if (fc["txtFrom"] == null) return View("PlantOverview");
            //fc.AllKeys[0] (fc["txtFrom"] == null)
            DateTime from_date = Convert.ToDateTime(fc["txtFrom"]);
            DateTime to_date = Convert.ToDateTime(fc["txtTo"]);
            //DateTime from_date = Convert.ToDateTime(fc["txtFrom"]).Date;
            //DateTime to_date = Convert.ToDateTime(fc["txtTo"]).Date;

            List<CTPlantOverview> PlanningOverview = db.Get_All_Business().Select(x => new CTPlantOverview()
            {
                Business = x.BIZ_NAME,
                PlanQty = Convert.ToInt64((from item in db.GetCTRptPlanQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.PLAN_QTY).SingleOrDefault()),
                FabricRcv = Convert.ToInt64((from item in db.GetCTRptPlanFabRcvQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.PLAN_QTY).SingleOrDefault()),
                SpreadBody = Convert.ToInt64((from item in db.GetCTRptPlanSprdQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.PLAN_QTY).SingleOrDefault()),
                Produced = Convert.ToInt64((from item in db.GetCTRptPlanProducedQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.PLAN_QTY).SingleOrDefault()),
                Reject = Convert.ToInt64((from item in db.GetCTRptPlanRejectQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.REJECT_QTY).SingleOrDefault()),
                TransferWH = Convert.ToInt64((from item in db.GetCTRptPlanTranfWHQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.TRANSFWH_QTY).SingleOrDefault()),
            }).ToList();

            List<CTPlantOverview> ProductionOverview = db.Get_All_Business().Select(x => new CTPlantOverview()
            {
                Business = x.BIZ_NAME,
                PlanQty = Convert.ToInt64((from item in db.GetCTRptPlanQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.PLAN_QTY).SingleOrDefault()),
                FabricRcv = Convert.ToInt64((from item in db.GetCTRptProdFabRcvQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.FABRCV_QTY).SingleOrDefault()),
                SpreadBody = Convert.ToInt64((from item in db.GetCTRptProdSpreadQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.SPREAD_QTY).SingleOrDefault()),
                SpreadLiner = Convert.ToInt64((from item in db.GetCTRptProdSpreadLnQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.SPREAD_QTY).SingleOrDefault()),
                Produced = Convert.ToInt64((from item in db.GetCTRptProdProducedQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.PRODUCED_QTY).SingleOrDefault()),
                Reject = Convert.ToInt64((from item in db.GetCTRptProdRejectQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.TRANSFWH_QTY).SingleOrDefault()),
                TransferWH = Convert.ToInt64((from item in db.GetCTRptProdTranfWHQtyByRangeDateBusiness(from_date, to_date, x.ID) select item.TRANSFWH_QTY).SingleOrDefault()),
            }).ToList();

            List<CTPlantOverviewByShift> Production1ShiftOverview = db.Get_All_Business().Select(x => new CTPlantOverviewByShift()
            {
                Shift = 1,
                Business = x.BIZ_NAME,
                FabricRcv = Convert.ToInt64((from item in db.GetCTRptProdShiftFabRcvQty(from_date, to_date, x.ID, 1) select item.QUANTITY).SingleOrDefault()),
                SpreadBody = Convert.ToInt64((from item in db.GetCTRptProdShiftSpreadQty(from_date, to_date, x.ID, 1) select item.QUANTITY).SingleOrDefault()),
                SpreadLiner = Convert.ToInt64((from item in db.GetCTRptProdShiftSpreadLnQty(from_date, to_date, x.ID, 1) select item.QUANTITY).SingleOrDefault()),
                Produced = Convert.ToInt64((from item in db.GetCTRptProdShiftProducedQty(from_date, to_date, x.ID, 1) select item.QUANTITY).SingleOrDefault()),
                Reject = Convert.ToInt64((from item in db.GetCTRptProdShiftRejectQty(from_date, to_date, x.ID, 1) select item.QUANTITY).SingleOrDefault()),
                TransferWH = Convert.ToInt64((from item in db.GetCTRptProdShiftTransfWHQty(from_date, to_date, x.ID, 1) select item.QUANTITY).SingleOrDefault()),
            }).ToList();

            List<CTPlantOverviewByShift> Production2ShiftOverview = db.Get_All_Business().Select(x => new CTPlantOverviewByShift()
            {
                Shift = 2,
                Business = x.BIZ_NAME,
                FabricRcv = Convert.ToInt64((from item in db.GetCTRptProdShiftFabRcvQty(from_date, to_date, x.ID, 2) select item.QUANTITY).SingleOrDefault()),
                SpreadBody = Convert.ToInt64((from item in db.GetCTRptProdShiftSpreadQty(from_date, to_date, x.ID, 2) select item.QUANTITY).SingleOrDefault()),
                SpreadLiner = Convert.ToInt64((from item in db.GetCTRptProdShiftSpreadLnQty(from_date, to_date, x.ID, 2) select item.QUANTITY).SingleOrDefault()),
                Produced = Convert.ToInt64((from item in db.GetCTRptProdShiftProducedQty(from_date, to_date, x.ID, 2) select item.QUANTITY).SingleOrDefault()),
                Reject = Convert.ToInt64((from item in db.GetCTRptProdShiftRejectQty(from_date, to_date, x.ID, 2) select item.QUANTITY).SingleOrDefault()),
                TransferWH = Convert.ToInt64((from item in db.GetCTRptProdShiftTransfWHQty(from_date, to_date, x.ID, 2) select item.QUANTITY).SingleOrDefault()),
            }).ToList();

            List<CTPlantOverviewByShift> Production3ShiftOverview = db.Get_All_Business().Select(x => new CTPlantOverviewByShift()
            {
                Shift = 3,
                Business = x.BIZ_NAME,
                FabricRcv = Convert.ToInt64((from item in db.GetCTRptProdShiftFabRcvQty(from_date, to_date, x.ID, 3) select item.QUANTITY).SingleOrDefault()),
                SpreadBody = Convert.ToInt64((from item in db.GetCTRptProdShiftSpreadQty(from_date, to_date, x.ID, 3) select item.QUANTITY).SingleOrDefault()),
                SpreadLiner = Convert.ToInt64((from item in db.GetCTRptProdShiftSpreadLnQty(from_date, to_date, x.ID, 3) select item.QUANTITY).SingleOrDefault()),
                Produced = Convert.ToInt64((from item in db.GetCTRptProdShiftProducedQty(from_date, to_date, x.ID, 3) select item.QUANTITY).SingleOrDefault()),
                Reject = Convert.ToInt64((from item in db.GetCTRptProdShiftRejectQty(from_date, to_date, x.ID, 3) select item.QUANTITY).SingleOrDefault()),
                TransferWH = Convert.ToInt64((from item in db.GetCTRptProdShiftTransfWHQty(from_date, to_date, x.ID, 3) select item.QUANTITY).SingleOrDefault()),
            }).ToList();

             CTPlantOverview ProductionOverview3shifts = new CTPlantOverview();
            ProductionOverview3shifts.FabricRcv = Production1ShiftOverview.Where(x => x.Business == "").Select(x => x.FabricRcv).Sum() + Production2ShiftOverview.Sum(x => x.FabricRcv) + Production3ShiftOverview.Sum(x => x.FabricRcv);
            ProductionOverview3shifts.SpreadBody = Production1ShiftOverview.Sum(x => x.SpreadBody) + Production2ShiftOverview.Sum(x => x.SpreadBody) + Production3ShiftOverview.Sum(x => x.SpreadBody);
            ProductionOverview3shifts.SpreadLiner = Production1ShiftOverview.Sum(x => x.SpreadLiner) + Production2ShiftOverview.Sum(x => x.SpreadLiner) + Production3ShiftOverview.Sum(x => x.SpreadLiner);
            ProductionOverview3shifts.Produced = Production1ShiftOverview.Sum(x => x.Produced) + Production2ShiftOverview.Sum(x => x.Produced) + Production3ShiftOverview.Sum(x => x.Produced);
            ProductionOverview3shifts.Reject = Production1ShiftOverview.Sum(x => x.Reject) + Production2ShiftOverview.Sum(x => x.Reject) + Production3ShiftOverview.Sum(x => x.Reject);
            ProductionOverview3shifts.TransferWH = Production1ShiftOverview.Sum(x => x.TransferWH) + Production2ShiftOverview.Sum(x => x.TransferWH) + Production3ShiftOverview.Sum(x => x.TransferWH);

            CTPlantOverview PlanningOverviewTotal = new CTPlantOverview();
            PlanningOverviewTotal.FabricRcv = PlanningOverview.Sum(x => x.FabricRcv);
            PlanningOverviewTotal.SpreadBody = PlanningOverview.Sum(x => x.SpreadBody);
            PlanningOverviewTotal.Produced = PlanningOverview.Sum(x => x.Produced);
            PlanningOverviewTotal.Reject = PlanningOverview.Sum(x => x.Reject);
            PlanningOverviewTotal.TransferWH = PlanningOverview.Sum(x => x.TransferWH);
            PlanningOverviewTotal.PlanQty = PlanningOverview.Sum(x => x.PlanQty);

            CTPlantOverview ProductionOverviewTotal = new CTPlantOverview();
            ProductionOverviewTotal.FabricRcv = ProductionOverview.Sum(x => x.FabricRcv);
            ProductionOverviewTotal.SpreadBody = ProductionOverview.Sum(x => x.SpreadBody);
            ProductionOverviewTotal.SpreadLiner = ProductionOverview.Sum(x => x.SpreadLiner);
            ProductionOverviewTotal.Produced = ProductionOverview.Sum(x => x.Produced);
            ProductionOverviewTotal.Reject = ProductionOverview.Sum(x => x.Reject);
            ProductionOverviewTotal.TransferWH = ProductionOverview.Sum(x => x.TransferWH);
            ProductionOverviewTotal.PlanQty = ProductionOverview.Sum(x => x.PlanQty);

            CTPlantOverviewByShift Production1ShiftOverviewTotal = new CTPlantOverviewByShift();
            Production1ShiftOverviewTotal.FabricRcv = Production1ShiftOverview.Sum(x => x.FabricRcv);
            Production1ShiftOverviewTotal.SpreadBody = Production1ShiftOverview.Sum(x => x.SpreadBody);
            Production1ShiftOverviewTotal.SpreadLiner = Production1ShiftOverview.Sum(x => x.SpreadLiner);
            Production1ShiftOverviewTotal.Produced = Production1ShiftOverview.Sum(x => x.Produced);
            Production1ShiftOverviewTotal.Reject = Production1ShiftOverview.Sum(x => x.Reject);
            Production1ShiftOverviewTotal.TransferWH = Production1ShiftOverview.Sum(x => x.TransferWH);
            Production1ShiftOverviewTotal.PlanQty = Production1ShiftOverview.Sum(x => x.PlanQty);

            CTPlantOverviewByShift Production2ShiftOverviewTotal = new CTPlantOverviewByShift();
            Production2ShiftOverviewTotal.FabricRcv = Production2ShiftOverview.Sum(x => x.FabricRcv);
            Production2ShiftOverviewTotal.SpreadBody = Production2ShiftOverview.Sum(x => x.SpreadBody);
            Production2ShiftOverviewTotal.SpreadLiner = Production2ShiftOverview.Sum(x => x.SpreadLiner);
            Production2ShiftOverviewTotal.Produced = Production2ShiftOverview.Sum(x => x.Produced);
            Production2ShiftOverviewTotal.Reject = Production2ShiftOverview.Sum(x => x.Reject);
            Production2ShiftOverviewTotal.TransferWH = Production2ShiftOverview.Sum(x => x.TransferWH);
            Production2ShiftOverviewTotal.PlanQty = Production2ShiftOverview.Sum(x => x.PlanQty);


            CTPlantOverviewByShift Production3ShiftOverviewTotal = new CTPlantOverviewByShift();
            Production3ShiftOverviewTotal.FabricRcv = Production3ShiftOverview.Sum(x => x.FabricRcv);
            Production3ShiftOverviewTotal.SpreadBody = Production3ShiftOverview.Sum(x => x.SpreadBody);
            Production3ShiftOverviewTotal.SpreadLiner = Production3ShiftOverview.Sum(x => x.SpreadLiner);
            Production3ShiftOverviewTotal.Produced = Production3ShiftOverview.Sum(x => x.Produced);
            Production3ShiftOverviewTotal.Reject = Production3ShiftOverview.Sum(x => x.Reject);
            Production3ShiftOverviewTotal.TransferWH = Production3ShiftOverview.Sum(x => x.TransferWH);
            Production3ShiftOverviewTotal.PlanQty = Production3ShiftOverview.Sum(x => x.PlanQty);



            ViewBag.ProductionOverview = ProductionOverview;
            ViewBag.Production1ShiftOverview = Production1ShiftOverview;
            ViewBag.Production2ShiftOverview = Production2ShiftOverview;
            ViewBag.Production3ShiftOverview = Production3ShiftOverview;

            ViewBag.PlanningOverviewTotal = PlanningOverviewTotal;
            ViewBag.ProductionOverviewTotal = ProductionOverviewTotal;
            ViewBag.Production1ShiftOverviewTotal = Production1ShiftOverviewTotal;
            ViewBag.Production2ShiftOverviewTotal = Production2ShiftOverviewTotal;
            ViewBag.Production3ShiftOverviewTotal = Production3ShiftOverviewTotal;

            return View("PlantOverview", PlanningOverview);
        }

        public string StatusIDtoName(int statusID)
        {
            return (from item in db.TBL_CT_ORDER_ST_MST.Where(t => t.STATUS_ID == statusID) select item.STATUS_NAME).SingleOrDefault();
        }

    }
}
