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
using ProductionApp.Helpers;


namespace ProductionApp.Controllers
{
    public class CTMstDataModuleController:BaseController
    {

        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            return RedirectToAction("MaintainMstData");
        }

        public ActionResult MaintainMstData()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }

            List<PROC_GET_ALL_MODULES_Result> AllModules = (from item in db.Get_All_Modules() select item).ToList();
            AllModules.RemoveAll(r => r.MO_ID > 4);
            List<PROC_GET_MODULES_BY_USER_Result> UserModules = (from item in db.GetModuleByUser(((UserModels)Session["SignedInUser"]).Username) select item).ToList();
            ViewBag.AllModules = AllModules;
            ViewBag.UserModule = UserModules;
            return View("MaintainMstData");

        }

        public ActionResult UploadData(FormCollection fc)
        {
            // CHECK PERMISSION
            var Opt = Convert.ToInt16(fc["RdoModule"]);

            // UPLOAD DATA
            if (Request != null)
            {
                int MesRow = 0;
                string OtherMes = null;
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.FirstOrDefault();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            switch (Opt)
                            {
                                case 1:
                                    // INSERT PLAN
                                    string WO, Asst, Garment, Mfg, Selling, Packing, GarColor, FabCode, FabCode2, Size, Qty, Note, CutDue, CutPlan, breakP;
                                    for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        MesRow = rowIterator;
                                        WO = (workSheet.Cells[rowIterator, 1].Value == null ? null : workSheet.Cells[rowIterator, 1].Value.ToString());
                                        Asst = (workSheet.Cells[rowIterator, 2].Value == null ? null : workSheet.Cells[rowIterator, 2].Value.ToString());
                                        Garment = (workSheet.Cells[rowIterator, 3].Value == null ? null : workSheet.Cells[rowIterator, 3].Value.ToString());
                                        Mfg = (workSheet.Cells[rowIterator, 4].Value == null ? null : workSheet.Cells[rowIterator, 4].Value.ToString());
                                        Selling = (workSheet.Cells[rowIterator, 5].Value == null ? null : workSheet.Cells[rowIterator, 5].Value.ToString());
                                        Packing = (workSheet.Cells[rowIterator, 6].Value == null ? null : workSheet.Cells[rowIterator, 6].Value.ToString());
                                        GarColor = (workSheet.Cells[rowIterator, 7].Value == null ? null : workSheet.Cells[rowIterator, 7].Value.ToString());
                                        FabCode = (workSheet.Cells[rowIterator, 8].Value == null ? null : workSheet.Cells[rowIterator, 8].Value.ToString());
                                        FabCode2 = (workSheet.Cells[rowIterator, 9].Value == null ? null : workSheet.Cells[rowIterator, 9].Value.ToString());
                                        Size = (workSheet.Cells[rowIterator, 10].Value == null ? null : workSheet.Cells[rowIterator, 10].Value.ToString());
                                        Qty = (workSheet.Cells[rowIterator, 11].Value == null ? null : workSheet.Cells[rowIterator, 11].Value.ToString());
                                        Note = (workSheet.Cells[rowIterator, 19].Value == null ? null : workSheet.Cells[rowIterator, 19].Value.ToString());
                                        CutDue = (workSheet.Cells[rowIterator, 21].Value == null ? null : workSheet.Cells[rowIterator, 21].Value.ToString());
                                        CutPlan = (workSheet.Cells[rowIterator, 22].Value == null ? null : workSheet.Cells[rowIterator, 22].Value.ToString());
                                        breakP = (workSheet.Cells[rowIterator, 27].Value == null ? null : workSheet.Cells[rowIterator, 27].Value.ToString());

                                        if (breakP == "b" && WO != null)
                                        {
                                            string tmp = Stand_WO(WO);
                                            TBL_CT_ORDER_ST CT_OrderRec = db.TBL_CT_ORDER_ST.Where(x => x.WO == tmp).FirstOrDefault();
                                            if (CT_OrderRec == null)
                                            {
                                                db.Database.ExecuteSqlCommand("delete from TBL_CT_PLAN where WO = {0}", Stand_WO(WO));
                                            }
                                        }

                                        else if (WO != null && Asst != null && Garment != null && Mfg != null && Selling != null && Packing != null && GarColor != null && FabCode != null && Size != null && Qty != null && CutDue != null && CutPlan != null)
                                        {
                                            WO = Stand_WO(WO);
                                            Asst = Stand_WO(Asst);

                                            TBL_CT_PLAN PlanRecord = db.TBL_CT_PLAN.Where(t => t.WO == WO).SingleOrDefault();
                                            if (PlanRecord == null)
                                            {
                                                var T_Year = "20" + CutDue.Substring(0, 2);
                                                var T_month = CutDue.Substring(2, 2);
                                                var T_date = CutDue.Substring(4, 2);
                                                DateTime T_Cutdue = Convert.ToDateTime(T_month + "-" + T_date + "-" + T_Year).Date;

                                                TBL_CT_PLAN TmpRecord = new TBL_CT_PLAN();
                                                TmpRecord.WO = WO;
                                                TmpRecord.ASST = Asst;
                                                TmpRecord.GARMENT_STYLE = Garment;
                                                TmpRecord.MFG_STYLE = Mfg;
                                                TmpRecord.SELLING_STYLE = Selling;
                                                TmpRecord.PACKING_STYLE = Packing;
                                                TmpRecord.GARMENT_COLOR = GarColor;
                                                TmpRecord.FABRIC_CODE = FabCode;
                                                TmpRecord.FABRIC_CODE_2 = FabCode2;
                                                TmpRecord.SIZE = Size;
                                                TmpRecord.QUANTITY = Convert.ToDouble(Qty);
                                                TmpRecord.NOTE = Note;
                                                TmpRecord.CUT_DUE_DATE = T_Cutdue;
                                                TmpRecord.CUT_PLAN_DATE = Convert.ToDateTime(CutPlan).Date;
                                                TmpRecord.TS_1_USER = (UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username;
                                                TmpRecord.TS_1 = DateTime.Now;
                                                db.TBL_CT_PLAN.Add(TmpRecord);
                                                db.SaveChanges();
                                            }
                                            else if (PlanRecord.NOTE != Note || PlanRecord.CUT_PLAN_DATE != Convert.ToDateTime(CutPlan).Date || PlanRecord.GARMENT_STYLE != Garment || PlanRecord.QUANTITY != Convert.ToDouble(Qty) || PlanRecord.ASST != Asst)
                                            {
                                                PlanRecord.CUT_PLAN_DATE = Convert.ToDateTime(CutPlan).Date;
                                                PlanRecord.NOTE = Note;
                                                PlanRecord.GARMENT_STYLE = Garment;
                                                db.SaveChanges();
                                            }

                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                    break;

                                case 2:
                                    // INSERT CUTTING SAH 
                                    string garment, mfg;
                                    double Ctex, SprdBkFt, SprdLiner, CutBkFt, CutLiner, TotalSprd, TotalCut, LegBind, WaistBind, TurBin, Lotting, SeamJoin, Total;
                                    for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        MesRow = rowIterator;
                                        garment = (workSheet.Cells[rowIterator, 2].Value == null ? null : workSheet.Cells[rowIterator, 2].Value.ToString());
                                        mfg = (workSheet.Cells[rowIterator, 3].Value == null ? null : workSheet.Cells[rowIterator, 3].Value.ToString());
                                        Ctex = Convert.ToDouble(workSheet.Cells[rowIterator, 4].Value == null ? "0" : workSheet.Cells[rowIterator, 4].Value.ToString());
                                        SprdBkFt = Convert.ToDouble(workSheet.Cells[rowIterator, 5].Value == null ? "0" : workSheet.Cells[rowIterator, 5].Value.ToString());
                                        SprdLiner = Convert.ToDouble(workSheet.Cells[rowIterator, 6].Value == null ? "0" : workSheet.Cells[rowIterator, 6].Value.ToString());
                                        CutBkFt = Convert.ToDouble(workSheet.Cells[rowIterator, 7].Value == null ? "0" : workSheet.Cells[rowIterator, 7].Value.ToString());
                                        CutLiner = Convert.ToDouble(workSheet.Cells[rowIterator, 8].Value == null ? "0" : workSheet.Cells[rowIterator, 8].Value.ToString());
                                        TotalSprd = Convert.ToDouble(workSheet.Cells[rowIterator, 9].Value == null ? "0" : workSheet.Cells[rowIterator, 9].Value.ToString());
                                        TotalCut = Convert.ToDouble(workSheet.Cells[rowIterator, 10].Value == null ? "0" : workSheet.Cells[rowIterator, 10].Value.ToString());
                                        LegBind = Convert.ToDouble(workSheet.Cells[rowIterator, 11].Value == null ? "0" : workSheet.Cells[rowIterator, 11].Value.ToString());
                                        WaistBind = Convert.ToDouble(workSheet.Cells[rowIterator, 12].Value == null ? "0" : workSheet.Cells[rowIterator, 12].Value.ToString());
                                        TurBin = Convert.ToDouble(workSheet.Cells[rowIterator, 13].Value == null ? "0" : workSheet.Cells[rowIterator, 13].Value.ToString());
                                        Lotting = Convert.ToDouble(workSheet.Cells[rowIterator, 14].Value == null ? "0" : workSheet.Cells[rowIterator, 14].Value.ToString());
                                        SeamJoin = Convert.ToDouble(workSheet.Cells[rowIterator, 15].Value == null ? "0" : workSheet.Cells[rowIterator, 15].Value.ToString());
                                        Total = Convert.ToDouble(workSheet.Cells[rowIterator, 16].Value == null ? "0" : workSheet.Cells[rowIterator, 16].Value.ToString());

                                        if (garment != null && mfg != null && TotalSprd > 0 && TotalCut > 0 && Total > 0)
                                        {
                                            TBL_CT_SAH SAHRecord = db.TBL_CT_SAH.SingleOrDefault(t => t.GARMENT_STYLE == garment && t.MFG_STYLE == mfg);
                                            if (SAHRecord == null)
                                            {
                                                TBL_CT_SAH TmpRecord = new TBL_CT_SAH();
                                                TmpRecord.GARMENT_STYLE = garment;
                                                TmpRecord.MFG_STYLE = mfg;
                                                TmpRecord.CTEX = float.Parse(Ctex.ToString());
                                                TmpRecord.SPRD_BK_FT = float.Parse(SprdBkFt.ToString());
                                                TmpRecord.SPRD_LINER = float.Parse(SprdLiner.ToString());
                                                TmpRecord.CUT_BK_FT = float.Parse(CutBkFt.ToString());
                                                TmpRecord.CUT_LINER = float.Parse(CutLiner.ToString());
                                                TmpRecord.TOTAL_SPRD = float.Parse(TotalSprd.ToString());
                                                TmpRecord.TOTAL_CUT = float.Parse(TotalCut.ToString());
                                                TmpRecord.LEG_BINDING = float.Parse(LegBind.ToString());
                                                TmpRecord.WASTE_BINDING = float.Parse(WaistBind.ToString());
                                                TmpRecord.TUBING = float.Parse(TurBin.ToString());
                                                TmpRecord.LOTTING = float.Parse(Lotting.ToString());
                                                TmpRecord.SEAM_JONT = float.Parse(SeamJoin.ToString());
                                                TmpRecord.TOTAL = float.Parse(Total.ToString());
                                                TmpRecord.TS_1_USER = (UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username;
                                                TmpRecord.TS_1 = DateTime.Now;
                                                db.TBL_CT_SAH.Add(TmpRecord);
                                                db.SaveChanges();
                                            }
                                            else if (SAHRecord.CTEX != Ctex || SAHRecord.SPRD_BK_FT != SprdBkFt || SAHRecord.SPRD_LINER != SprdLiner || SAHRecord.CUT_BK_FT != CutBkFt
                                                || SAHRecord.CUT_LINER != CutLiner || SAHRecord.TOTAL_SPRD != TotalSprd || SAHRecord.TOTAL_CUT != TotalCut || SAHRecord.LEG_BINDING != LegBind
                                                    || SAHRecord.WASTE_BINDING != WaistBind || SAHRecord.TUBING != TurBin || SAHRecord.LOTTING != Lotting || SAHRecord.SEAM_JONT != SeamJoin
                                                    || SAHRecord.TOTAL != Total)
                                            {
                                                SAHRecord.CTEX = float.Parse(Ctex.ToString());
                                                SAHRecord.SPRD_BK_FT = float.Parse(SprdBkFt.ToString());
                                                SAHRecord.SPRD_LINER = float.Parse(SprdLiner.ToString());
                                                SAHRecord.CUT_BK_FT = float.Parse(CutBkFt.ToString());
                                                SAHRecord.CUT_LINER = float.Parse(CutLiner.ToString());
                                                SAHRecord.TOTAL_SPRD = float.Parse(TotalSprd.ToString());
                                                SAHRecord.TOTAL_CUT = float.Parse(TotalCut.ToString());
                                                SAHRecord.LEG_BINDING = float.Parse(LegBind.ToString());
                                                SAHRecord.WASTE_BINDING = float.Parse(WaistBind.ToString());
                                                SAHRecord.TUBING = float.Parse(TurBin.ToString());
                                                SAHRecord.LOTTING = float.Parse(Lotting.ToString());
                                                SAHRecord.SEAM_JONT = float.Parse(SeamJoin.ToString());
                                                SAHRecord.TOTAL = float.Parse(Total.ToString());

                                            }
                                        }
                                        else
                                        {
                                            throw new Exception();
                                            //HandleError(new Exception("AnyCondition is not true"));
                                        }
                                    }

                                    break;


                                case 3:
                                    // CUTTING COMPONENT
                                    string GarmentCom;
                                    int Front, Back, FrontPnl, FrontSide, Croth, CrothLiner, LeBind, WaBind;
                                    for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        MesRow = rowIterator;
                                        GarmentCom = (workSheet.Cells[rowIterator, 1].Value == null ? null : workSheet.Cells[rowIterator, 1].Value.ToString());
                                        Front = Convert.ToInt16(workSheet.Cells[rowIterator, 2].Value == null ? "0" : workSheet.Cells[rowIterator, 2].Value.ToString());
                                        Back = Convert.ToInt16(workSheet.Cells[rowIterator, 3].Value == null ? "0" : workSheet.Cells[rowIterator, 3].Value.ToString());
                                        FrontPnl = Convert.ToInt16(workSheet.Cells[rowIterator, 4].Value == null ? "0" : workSheet.Cells[rowIterator, 4].Value.ToString());
                                        FrontSide = Convert.ToInt16(workSheet.Cells[rowIterator, 5].Value == null ? "0" : workSheet.Cells[rowIterator, 5].Value.ToString());
                                        Croth = Convert.ToInt16(workSheet.Cells[rowIterator, 6].Value == null ? "0" : workSheet.Cells[rowIterator, 6].Value.ToString());
                                        CrothLiner = Convert.ToInt16(workSheet.Cells[rowIterator, 7].Value == null ? "0" : workSheet.Cells[rowIterator, 7].Value.ToString());
                                        LeBind = Convert.ToInt16(workSheet.Cells[rowIterator, 8].Value == null ? "0" : workSheet.Cells[rowIterator, 8].Value.ToString());
                                        WaBind = Convert.ToInt16(workSheet.Cells[rowIterator, 9].Value == null ? "0" : workSheet.Cells[rowIterator, 9].Value.ToString());

                                        if (GarmentCom != null)
                                        {
                                            db.Database.ExecuteSqlCommand("delete from TBL_CT_GARMENT_CMPNT where GARMENT_STYLE = {0}", GarmentCom);

                                            if (Front != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 1;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = Front;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }

                                            if (Back != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 2;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = Back;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }
                                            if (FrontPnl != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 3;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = FrontPnl;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }

                                            if (FrontSide != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 4;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = FrontSide;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }
                                            if (Croth != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 5;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = Croth;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }
                                            if (CrothLiner != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 6;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = CrothLiner;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }

                                            if (LeBind != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 7;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = LeBind;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }
                                            if (WaBind != 0)
                                            {
                                                TBL_CT_GARMENT_CMPNT GarCmpntRecord = new TBL_CT_GARMENT_CMPNT();
                                                GarCmpntRecord.CMPNT_ID = 8;
                                                GarCmpntRecord.GARMENT_STYLE = GarmentCom;
                                                GarCmpntRecord.QUANTITY = WaBind;
                                                db.TBL_CT_GARMENT_CMPNT.Add(GarCmpntRecord);
                                            }
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                    break;

                                case 4:
                                    // INSERT TTS ORDER
                                    string TTSWo, TTSAsst, TTSStatus, TTSCutDue, TTSSttDate;
                                    for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        MesRow = rowIterator;
                                        TTSWo = (workSheet.Cells[rowIterator, 1].Value == null ? null : workSheet.Cells[rowIterator, 1].Value.ToString());
                                        TTSAsst = (workSheet.Cells[rowIterator, 2].Value == null ? null : workSheet.Cells[rowIterator, 2].Value.ToString());
                                        TTSStatus = (workSheet.Cells[rowIterator, 3].Value == null ? null : workSheet.Cells[rowIterator, 3].Value.ToString());
                                        TTSCutDue = (workSheet.Cells[rowIterator, 4].Value == null ? null : workSheet.Cells[rowIterator, 4].Value.ToString());
                                        TTSSttDate = (workSheet.Cells[rowIterator, 5].Value == null ? null : workSheet.Cells[rowIterator, 5].Value.ToString());

                                        if (TTSWo != null && TTSStatus != null && TTSCutDue != null && TTSSttDate != null)
                                        {
                                            TTSWo = Stand_WO(TTSWo);
                                            TTSAsst = Stand_WO(TTSAsst);
                                            TBL_TTS_ORDER_ST OrderRecord = db.TBL_TTS_ORDER_ST.SingleOrDefault(t => t.WO == TTSWo);
                                            if (OrderRecord == null)
                                            {
                                                TBL_TTS_ORDER_ST TmpRecord = new TBL_TTS_ORDER_ST();
                                                TmpRecord.WO = TTSWo;
                                                TmpRecord.ASST = TTSAsst;
                                                TmpRecord.STATUS = TTSStatus;
                                                //TmpRecord.CUTDUE = Convert.ToDateTime(TTSCutDue);
                                               // TmpRecord.ST_DATE = Convert.ToDateTime(TTSSttDate);

                                                db.TBL_TTS_ORDER_ST.Add(TmpRecord);
                                                db.SaveChanges();
                                            }


                                            //else if (OrderRecord.STATUS != TTSStatus || Convert.ToDateTime(OrderRecord.CUTDUE) != Convert.ToDateTime(TTSCutDue)
                                            //    || Convert.ToDateTime(OrderRecord.ST_DATE) != Convert.ToDateTime(TTSSttDate))
                                            //{
                                            //    OrderRecord.STATUS = TTSStatus;
                                            //    OrderRecord.CUTDUE = Convert.ToDateTime(TTSCutDue).Date;
                                            //    OrderRecord.ST_DATE = Convert.ToDateTime(TTSSttDate).Date;
                                            //    db.SaveChanges();
                                            //}
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                    TBL_TTS_ORDER_UPD_HST UpdateRecord = new TBL_TTS_ORDER_UPD_HST();
                                    UpdateRecord.TS_1 = DateTime.Now;
                                    UpdateRecord.TS_1_USER = ((UserModels)Session["SignedInUser"]) == null ? null : ((UserModels)Session["SignedInUser"]).Username;
                                    db.TBL_TTS_ORDER_UPD_HST.Add(UpdateRecord);
                                    db.SaveChanges();
                                    break;
                            }

                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Status = "Error, Data is invalid. " + "Row No " + Convert.ToString(MesRow) + " - " + OtherMes + e.Message;
                    Utilities.WriteLogException(e, "CTMstDataModule/UploadData");
                }
            }

            //ADD DATA FOR COMBO BOX
            List<PROC_GET_ALL_MODULES_Result> AllModules = (from item in db.Get_All_Modules() select item).ToList();
            AllModules.RemoveAll(r => r.MO_ID > 4);

            ViewBag.AllModules = AllModules;


            return View("MaintainMstData");
        }

        public string Stand_WO(string str)
        {
            while (str.Length < 6)
            {
                str = "0" + str;
            }
            return str;
        }
    }
}
