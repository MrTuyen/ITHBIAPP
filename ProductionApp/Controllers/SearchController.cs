using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;
using ProductionApp.Controllers;

namespace ProductionApp.Controllers
{
    public class SearchController:BaseController
    {
        // GET: Report
        public ActionResult Index()
        {
            return View("Search");
        }
        public ActionResult Search(FormCollection fc)
        {
            string data = fc["txtSearch"];
            if (data != null)
            {
                if(data.Trim().Length == 10)
                {
                    List<PROC_GET_OUTPUT_BY_WL_Result> WLCase = (from item in db.GetOutputByWL(data) select item).ToList();
                    return View("Worklot", WLCase);
                }
                else if(data.Trim().Length <=6)
                {
                    List<ViewCTOutputDetail> ListOutputDetail = db.GetCTRptPlanByWO(new CTModuleController().Stand_WO(data)).Select(x => new ViewCTOutputDetail
                    {
                        WO = x.WO,
                        Assortment = x.ASST,
                        MfgStyle = x.MFG_STYLE,
                        SellingStyle = x.SELLING_STYLE,
                        Garment = x.GARMENT_STYLE,
                        GarmentColor = x.GARMENT_COLOR,
                        Business = (from item in db.GetWCInforForOneSelling(x.SELLING_STYLE) select item.UNIT).SingleOrDefault(),
                        FabricCode = x.FABRIC_CODE,
                        FabricCode2 = x.FABRIC_CODE_2,
                        Size = x.SIZE,
                        Quantity = Convert.ToDouble(x.QUANTITY),
                        PlanDate = Convert.ToDateTime(x.CUT_PLAN_DATE),
                        TTS_STT = (from item in db.TBL_TTS_ORDER_ST.Where(t => t.WO == x.WO) select item.STATUS).SingleOrDefault(),
                        Status = new CTRptModuleController().StatusIDtoName(Convert.ToInt16((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x.WO).OrderByDescending(t => t.TS_1) select item.STATUS_ID).FirstOrDefault())),
                        Fabric_Recieved = Convert.ToDateTime((from item in db.TBL_CT_ORDER_ST.Where(t => t.WO == x.WO && t.STATUS_ID == 10).OrderBy(t => t.TS_1) select item.TS_1).FirstOrDefault()),
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

                    string WO = new CTModuleController().Stand_WO(data);
                    List<ViewQCDetailComponentStt> ListComponentStt = new List<ViewQCDetailComponentStt>();
                    List<TBL_CT_ORDER_SPR_TBL> ListTbl = (from item in db.TBL_CT_ORDER_SPR_TBL.Where(t => t.WO == WO) select item).ToList();
                    foreach(var item in ListTbl)
                    {

                        List<PROC_GET_CT_DETAIL_CMPNT_STT_BY_SPRDTBLID_Result> ListCmpnt = db.PROC_GET_CT_DETAIL_CMPNT_STT_BY_SPRDTBLID(item.ORDER_TBL_ID).OrderBy(t => t.COMPNT_ID).ThenByDescending(t => t.TS_1).ToList();
                        ViewQCDetailComponentStt TblInfor = new ViewQCDetailComponentStt();
                        TblInfor.TBL_ID = item.ORDER_TBL_ID;
                        TblInfor.TBL_NO = item.TBL_NO;
                        TblInfor.Type = Convert.ToInt16(item.CUTTING_TYPE);
                        TblInfor.Qty = Convert.ToDouble(item.QUANTITY);
                        TblInfor.SpreadDate = Convert.ToDateTime(item.TS_1);
                        ListComponentStt.Add(TblInfor);
                        foreach (var Cmpnt in ListCmpnt)
                        {
                            ViewQCDetailComponentStt CmpntInfor = new ViewQCDetailComponentStt();
                            CmpntInfor.TBL_ID = Cmpnt.ORDER_TBL_ID;
                            CmpntInfor.TBL_NO = Cmpnt.TBL_NO;
                            CmpntInfor.Type = Convert.ToInt16(Cmpnt.CUTTING_TYPE);
                            CmpntInfor.Qty = Convert.ToDouble(Cmpnt.QUANTITY);
                            CmpntInfor.SpreadDate = Convert.ToDateTime(Cmpnt.SPRD_TS);
                            CmpntInfor.PartName = Cmpnt.NAME_EN;
                            CmpntInfor.PartName_VNM = Cmpnt.NAME_VNM;
                            CmpntInfor.Status_Name = Cmpnt.STATUS_ID == 50? "Pass": "Reject";
                            CmpntInfor.QC_Checking_Date = Convert.ToDateTime(Cmpnt.TS_1);
                            CmpntInfor.QC_User = Cmpnt.TS_1_USER;
                            ListComponentStt.Add(CmpntInfor);
                        }
                    }
                    ViewBag.ListComponentStt = ListComponentStt;
                    return View("WO", ListOutputDetail);
                }
                else
                {
                    PROC_GET_ONE_CASE_INFOR_Result oneCase = (from item in db.GetOneCaseInfor(data) select item).SingleOrDefault();
                    return View("Case", oneCase);
                }
            }
            return View("Case");
        }
    }
}
