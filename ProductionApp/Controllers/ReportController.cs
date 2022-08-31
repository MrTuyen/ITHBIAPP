using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;
using OfficeOpenXml.Style;


namespace ProductionApp.Controllers {
    public class ReportController:BaseController {
        // GET: Report
        public ActionResult Index() {

            return View("Daily");
        }
        public ActionResult ShowData(string datestr) {
            List<PROC_DAILY_REPORT_Result> lstReport = null;
            if(datestr != null) {
                var dt = Convert.ToDateTime(datestr.ToString());
                lstReport = (from item in db.GetDailyReport(dt) select item).ToList();
            }
            return PartialView("_ShowData" ,lstReport);
        }

        // EXPORT TO EXCEL FILE
        //public ActionResult ExportExcel1(string datestr, int ws)
        //{
        //    DateTime dt = Convert.ToDateTime(datestr.ToString());
        //    ExcelPackage excel = new ExcelPackage();
        //    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
        //    List<ExportExcelModel> data = db.GetExportExcelReport(dt).Select(x => new ExportExcelModel()
        //    {
        //        labelID = x.LABEL_ID,
        //        Quantity = (double)x.QUANTITY,
        //        WShop = x.WSHOP_ID,
        //        UserScan = x.TS_2_USER,
        //        PlantCode = x.PLANT_ID,
        //        TimeScan = (DateTime)x.TS_2

        //    }).ToList();


        //    workSheet.Cells[1, 1].LoadFromCollection(data, true);
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("content-disposition", "attachment;  filename=" + DateTime.Now.ToLongTimeString() + ".xlsx");
        //        excel.SaveAs(memoryStream);
        //        memoryStream.WriteTo(Response.OutputStream);
        //        Response.Flush();
        //        Response.End();
        //    }
        //    return RedirectToAction("index");
        //    //return RedirectToAction("index", workSheet);
        //}

        public ActionResult ExportExcel(FormCollection fc) {
            string datestr = fc["txtDate"];
            DateTime dt = Convert.ToDateTime(datestr.ToString());
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            List<ExportExcelModel> data = db.GetExportExcelReport(dt).Select(x => new ExportExcelModel() {
                Plant_Cd = (int)x.PLANT_CODE ,
                Label_ID = x.LABEL_ID ,
                Type = x.TYPE ,
                WorkLot = x.WLOT_ID ,
                Selling_Style = x.STYLE ,
                Color = x.COLOR ,
                Size = x.SIZE ,
                Quantity = (double)x.QUANTITY ,
                Work_Shop = x.WSHOP_ID ,
                User_Scan = x.TS_2_USER ,
                User_Plant_Cd = (int)x.SCAN_PLANT_CODE ,
                Time_Scan = (DateTime)x.TS_2

            }).ToList();

            List<ExportExcelModel> data2 = db.GetExportExelReportMWL(dt).Select(x => new ExportExcelModel() {
                Plant_Cd = (int)x.PLANT_CODE ,
                Label_ID = x.LABEL_ID ,
                //Type = x.TYPE,
                WorkLot = x.WLOT_ID ,
                Selling_Style = null ,
                Color = null ,
                Size = null ,
                //Quantity = null,
                Work_Shop = x.WSHOP_ID ,
                //User_Scan = x.TS_2_USER,
                //User_Plant_Cd = (int)x.SCAN_PLANT_CODE,
                Time_Scan = (DateTime)x.TS_2

            }).ToList();

            data.AddRange(data2);
            data = data.OrderBy(o => o.Label_ID).ToList();
            workSheet.Cells[1 ,1].LoadFromCollection(data ,true);

            using(ExcelRange col = workSheet.Cells[1 ,1 ,data.Count + 1 ,12]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            workSheet.Cells["A1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:L1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet.Cells["A1:L1"].Style.Font.Bold = true;
            workSheet.Cells["A1:L1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet.Cells["A1:L1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            //workSheet.Cells["A1:F1"].Style.Font.Size  = 20;
            ////

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=" + DateTime.Now.ToLongTimeString() + ".xlsx");

                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("index");
            //return RedirectToAction("index", workSheet);
        }

        public ActionResult ExportExcelLocationRpt(FormCollection fc) {
            DateTime From_date = Convert.ToDateTime(fc["TxtFrom"]); //Convert.ToDateTime("02/26/2017");
            DateTime To_date = Convert.ToDateTime(fc["TxtTo"]); //DateTime.Now; //fc[""];
            //List<PROC_GET_TOTAL_SAH_BY_DATE_Result> TotalSah = (from item in db.GetTotalSahByDate(From_date, To_date) select item).ToList();
            ExcelPackage excel = new ExcelPackage();


            //var GroupLabor = Convert.ToDouble((from t in db.GetTargetLaborByDateGroup(From_date, To_date, x.GROUP_ID) select t.TOTAL_LABOR).SingleOrDefault());
            //var WorkCentral = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.WC_GROUP).SingleOrDefault());
            //var Construction = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.CONSTRUCTION).SingleOrDefault());
            //var HR_Labor = Convert.ToInt32((from t in db.GetHRLaborRecordByDateGroup(From_date, To_date, x.GROUP_ID) select t.LABOR).SingleOrDefault());
            //var HR_Absm_Labor = Convert.ToInt32((from t in db.GetHRLaborRecordByDateGroup(From_date, To_date, x.GROUP_ID) select t.ABSENTEEISM).SingleOrDefault());
            //var HR_OT = Convert.ToInt32((from t in db.GetHRLaborRecordByDateGroup(From_date, To_date, x.GROUP_ID) select t.OT).SingleOrDefault());


            var workSheet = excel.Workbook.Worksheets.Add("EFFICIENCY");
            List<ExportExcelLocationReportModel> data = db.GetTotalSahByDate(From_date ,To_date).Select(x => new ExportExcelLocationReportModel() {
                GroupName = x.GROUP_NAME ,
                GroupLabor = Convert.ToDouble((from t in db.GetTargetLaborByDateGroup(From_date ,To_date ,x.GROUP_ID) select t.TOTAL_LABOR).SingleOrDefault()) ,
                Style = x.STYLE ,
                SAH = x.SAH ,
                Output = Convert.ToDouble(x.TOTAL_QTY) ,
                WorkCentral = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.WC_GROUP).SingleOrDefault()) ,
                Construction = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.CONSTRUCTION).SingleOrDefault()) ,
                Unit = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.UNIT).SingleOrDefault()) ,
                HR_Labor = Convert.ToInt32((from t in db.GetHRLaborRecordByDateGroup(From_date ,To_date ,x.GROUP_ID) select t.LABOR).SingleOrDefault()) ,
                HR_Absm_Labor = Convert.ToInt32((from t in db.GetHRLaborRecordByDateGroup(From_date ,To_date ,x.GROUP_ID) select t.ABSENTEEISM).SingleOrDefault()) ,
                HR_OT = Convert.ToInt32((from t in db.GetHRLaborRecordByDateGroup(From_date ,To_date ,x.GROUP_ID) select t.OT).SingleOrDefault()) ,
            }).ToList();

            // caculate efficiency
            List<ExportExcelLocationReportModel_TMP> Eff_TMP = new List<ExportExcelLocationReportModel_TMP>();
            int count = 0;
            ExportExcelLocationReportModel_TMP tmp;
            for(int i = 0; i < data.Count; i++) {

                if(i == 0) {
                    tmp = new ExportExcelLocationReportModel_TMP();
                    tmp.GroupName = data[i].GroupName;
                    tmp.TOTAL_SAH = data[i].Output * data[i].SAH;
                    tmp.TOTAL_HOURS = ((data[i].HR_Labor - data[i].HR_Absm_Labor) * 7.5) + data[i].HR_OT;
                    Eff_TMP.Add(tmp);
                } else {
                    if(i <= data.Count && data[i - 1].GroupName != data[i].GroupName) {
                        tmp = new ExportExcelLocationReportModel_TMP();
                        Eff_TMP.Add(tmp);
                        count += 1;
                    }
                    Eff_TMP[count].GroupName = data[i].GroupName;
                    Eff_TMP[count].TOTAL_SAH += data[i].Output * data[i].SAH;
                    Eff_TMP[count].TOTAL_HOURS = ((data[i].HR_Labor - data[i].HR_Absm_Labor) * 7.5) + data[i].HR_OT;
                }
            }

            for(int i = 0; i < data.Count; i++) {
                for(int j =0; j < Eff_TMP.Count; j++) {
                    if(data[i].GroupName == Eff_TMP[j].GroupName) {
                        decimal myDec;
                        var Result = decimal.TryParse((Eff_TMP[j].TOTAL_SAH / Eff_TMP[j].TOTAL_HOURS).ToString() ,out myDec);
                        if(Result == true) {
                            data[i].Efficiency = (Math.Round(Eff_TMP[j].TOTAL_SAH / Eff_TMP[j].TOTAL_HOURS ,4) * 100).ToString() + " %";
                        } else { data[i].Efficiency = "0 %"; }
                    }
                }
            }

            //data.AddRange(data);
            data = data.OrderBy(o => o.GroupName).ToList();
            workSheet.Cells[1 ,1].LoadFromCollection(data ,true);

            //// FORMAT WORKSHEET
            using(ExcelRange col = workSheet.Cells[1 ,1 ,data.Count + 1 ,13]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            workSheet.Cells["A1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:L1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet.Cells["A1:L1"].Style.Font.Bold = true;
            workSheet.Cells["A1:L1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet.Cells["A1:L1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            //workSheet.Cells["A1:F1"].Style.Font.Size  = 20;


            // ADD NEW SHEET ACTUAL OUTPUT SHEET
            var workSheet2 = excel.Workbook.Worksheets.Add("ACTUAL_SCAN");
            // PROCESS DATA
            List<ExportExcelLocationReportModel_DetailDate> data_Output = db.GetTotalOutputByDate(From_date ,To_date).Select(x => new ExportExcelLocationReportModel_DetailDate() {
                Date = Convert.ToDateTime(x.SCAN_DATE) ,
                GroupName = x.GROUP_NAME ,
                GroupLabor = Convert.ToDouble((from t in db.GetTargetLaborByDateGroup(From_date ,To_date ,x.GROUP_ID) select t.TOTAL_LABOR).SingleOrDefault()) ,
                Style = x.STYLE ,
                Size = x.SIZE ,
                Output = Convert.ToDouble(x.TOTAL_QTY) ,
                WorkCentral = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.WC_GROUP).SingleOrDefault()) ,
                Construction = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.CONSTRUCTION).SingleOrDefault()) ,
                Unit = ((from t in db.GetWCInforForOneSelling(x.STYLE) select t.UNIT).SingleOrDefault()) ,
            }).ToList();

            //ADD DATA TO EXCEL SHEET
            data_Output = data_Output.OrderBy(o => o.Date).ToList();
            workSheet2.Cells[1 ,1].LoadFromCollection(data_Output ,true);
            //// FORMAT WORKSHEET
            using(ExcelRange col = workSheet2.Cells[1 ,1 ,data_Output.Count + 1 ,9]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            using(ExcelRange col = workSheet2.Cells[2 ,1 ,data_Output.Count + 1 ,1]) {
                col.Style.Numberformat.Format = "MM/dd/yyyy";
                col.AutoFitColumns();
                //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            workSheet2.Cells["A1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet2.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet2.Cells["A1:I1"].Style.Font.Bold = true;
            workSheet2.Cells["A1:I1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet2.Cells["A1:I1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename= LocationReport" + DateTime.Now.ToLongTimeString() + ".xlsx");

                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("index");
        }

        public ActionResult ExportExcelWorkcentralRpt(FormCollection fc) {
            DateTime From_date = Convert.ToDateTime(fc["TxtFrom"]); //Convert.ToDateTime("02/26/2017");
            DateTime To_date = Convert.ToDateTime(fc["TxtTo"]); //DateTime.Now; //fc[""];
            String LstWC = fc["TxtWc"]; //DateTime.Now; //fc[""];

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            List<ExportExcelWCentralReportModel> data = db.GetWcReportByDateWcID(From_date ,To_date ,LstWC).Select(x => new ExportExcelWCentralReportModel() {
                WorkCentral = x.WC_GROUP ,
                WorkShop = x.WS_NAME ,
                GroupName = x.GROUP_NAME ,
                Style = x.STYLE ,
                Color = x.COLOR ,
                Size = x.SIZE ,
                Worklot = x.WLOT_ID ,
                Quantity = Convert.ToDouble(x.QUANTITY)
            }).ToList();
            data = data.OrderBy(o => o.WorkCentral).ToList();
            workSheet.Cells[1 ,1].LoadFromCollection(data ,true);

            using(ExcelRange col = workSheet.Cells[1 ,1 ,data.Count + 1 ,12]) {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            workSheet.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet.Cells["A1:H1"].Style.Font.Bold = true;
            workSheet.Cells["A1:H1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet.Cells["A1:H1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            //workSheet.Cells["A1:F1"].Style.Font.Size  = 20;
            ////

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=" + DateTime.Now.ToLongTimeString() + ".xlsx");

                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("index");
        }
        // END EXPORT WORKCENTRAL REPORT TO EXCEL FILE BY DATE

        // REALTIME OLD
        //public ActionResult Realtime()
        //{

        //    DateTime CurrTime = DateTime.Now;
        //    PROC_GET_TIMEFRAME_BY_DATE_Result TimeFrRecord = (from item in db.GetTimeFrameByDate(CurrTime) select item).SingleOrDefault();
        //    PROC_GET_OUTPUT_FRAME_EXIST_Result OutputFrExist = new PROC_GET_OUTPUT_FRAME_EXIST_Result();
        //    if (TimeFrRecord != null)
        //    {
        //        OutputFrExist = (from item in db.GetOutputFrameExist(TimeFrRecord.FRAME_ID) select item).SingleOrDefault();
        //    }


        //    if (OutputFrExist == null)
        //    {
        //        List<PROC_GET_PLAN_BY_DATE_Result> PrdPlanRecord = (from item in db.GetPlanByDate(CurrTime) select item).ToList();
        //        if (PrdPlanRecord != null)
        //        {
        //            List<TmpRealtimeReport> TmpReport = new List<TmpRealtimeReport>();
        //            foreach (var item in PrdPlanRecord)
        //            {
        //                TmpRealtimeReport OneRecord = new TmpRealtimeReport();
        //                OneRecord.GroupID = item.GROUP_ID;
        //                OneRecord.GroupName = item.GROUP_NAME;
        //                OneRecord.PlantName = item.PLANT_NAME;
        //                OneRecord.Labor = Convert.ToDouble(item.LABOR);
        //                OneRecord.SAH = Convert.ToDouble(item.SAH);
        //                OneRecord.Target = Convert.ToDouble(item.TARGET_QTY);
        //                OneRecord.Style = item.STYLE;
        //                OneRecord.WorkShop = item.WS_NAME;
        //                OneRecord.ShiftID = item.SHIFT_ID;
        //                //get output from case label table
        //                double output1 = (db.GetRealtimeOutput(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First() != null ? db.GetRealtimeOutput(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First().Value : 0);
        //                double output2 = (db.GetRealtimeOutputWLLoc(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First() != null ? db.GetRealtimeOutputWLLoc(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First().Value : 0);
        //                OneRecord.Output = output1 + output2;

        //                TmpReport.Add(OneRecord);
        //            }
        //            // DISTINCT GROUP ID 
        //            List<RealtimeReport> Report = new List<RealtimeReport>();
        //            foreach (var item in TmpReport)
        //            {
        //                if (Report.Count == 0)
        //                {
        //                    RealtimeReport OneRecord = new RealtimeReport();
        //                    OneRecord.GroupID = item.GroupID;
        //                    Report.Add(OneRecord);
        //                }
        //                else
        //                {
        //                    bool exist = false;
        //                    for (int i = 0; i < Report.Count; i++)
        //                    {
        //                        if (item.GroupID == Report[i].GroupID)
        //                        {
        //                            exist = true;
        //                            break;

        //                        }
        //                    }
        //                    if (!exist)
        //                    {
        //                        RealtimeReport OneRecord = new RealtimeReport();
        //                        OneRecord.GroupID = item.GroupID;
        //                        Report.Add(OneRecord);
        //                    }
        //                }
        //            }

        //            // CACULATE DATA
        //            foreach (var R in Report)
        //            {
        //                foreach (var TmpR in TmpReport)
        //                {
        //                    if (R.GroupID == TmpR.GroupID)
        //                    {
        //                        R.GroupName = TmpR.GroupName;
        //                        R.Output = R.Output + TmpR.Output;
        //                        R.PlantName = TmpR.PlantName;
        //                        R.ShiftID = TmpR.ShiftID;
        //                        R.Target = R.Target + TmpR.Target;
        //                        R.TotalHour = R.TotalHour + (TmpR.Labor * (Convert.ToDouble(TimeFrRecord.PIECE) * 0.5));
        //                        R.TotalSAH = R.TotalSAH + (TmpR.Output * TmpR.SAH);
        //                        R.WorkShop = TmpR.WorkShop;

        //                    }
        //                }
        //            }

        //            // INSERT TO DATABASE
        //            foreach (var item in Report)
        //            {
        //                TBL_OUTPUT_UPDATE outputupdate = new TBL_OUTPUT_UPDATE();
        //                outputupdate.ACTUAL = Math.Round(item.Output, 2);
        //                outputupdate.TARGET = Math.Round(item.Target, 2);
        //                outputupdate.EFFICIENCY = Math.Round((item.TotalSAH / item.TotalHour), 2);
        //                outputupdate.FRAME_ID = TimeFrRecord.FRAME_ID;
        //                outputupdate.GROUP_NAME = item.GroupName;
        //                outputupdate.PLANT_NAME = item.PlantName;
        //                outputupdate.SHIFT_NAME = Convert.ToString(item.ShiftID);
        //                outputupdate.WS_NAME = item.WorkShop;
        //                outputupdate.TS_1 = DateTime.Now;
        //                outputupdate.PIECE = Convert.ToDouble(TimeFrRecord.PIECE);
        //                outputupdate.WL_OVER_7DAYS = db.GetNonReconcByGroup(item.GroupID).FirstOrDefault().Value;
        //                // COLOR
        //                if (TimeFrRecord.SHIFT_BEGIN == 1)
        //                    outputupdate.COLOR = "";
        //                else
        //                if (((item.Target / 15) * TimeFrRecord.PIECE) > item.Output)
        //                    outputupdate.COLOR = "orangered";
        //                else
        //                    outputupdate.COLOR = "lawngreen";
        //                db.TBL_OUTPUT_UPDATE.Add(outputupdate);
        //                db.SaveChanges();

        //            }
        //        }
        //    }
        //    List<PROC_GET_REALTIME_UPDATE_Result> realtimeUpdate = (from item in db.GetRealtimeUpdate(TimeFrRecord.FRAME_ID) select item).ToList();
        //    return View("Realtime", realtimeUpdate);
        //}

        public ActionResult Location(FormCollection fc) {
            DateTime From_date = Convert.ToDateTime(fc["TxtFrom"]); //Convert.ToDateTime("02/26/2017");
            DateTime To_date = Convert.ToDateTime(fc["TxtTo"]); //DateTime.Now; //fc[""];

            List<PROC_GET_PLAN_BY_RANGE_DATE_Result> PrdPlanRecord = (from item in db.GetPlanByRangeDate(From_date ,To_date) select item).ToList();
            List<DailyLocReport> Report = new List<DailyLocReport>();
            if(PrdPlanRecord != null) {
                List<TmpRealtimeReport> TmpReport = new List<TmpRealtimeReport>();
                foreach(var item in PrdPlanRecord) {
                    TmpRealtimeReport OneRecord = new TmpRealtimeReport();
                    OneRecord.GroupID = item.GROUP_ID;
                    OneRecord.GroupName = item.GROUP_NAME;
                    OneRecord.PlantName = item.PLANT_NAME;
                    OneRecord.Labor = Convert.ToDouble(item.LABOR);
                    OneRecord.SAH = Convert.ToDouble(item.SAH);
                    OneRecord.Target = Convert.ToDouble(item.TARGET_QTY);
                    OneRecord.Style = item.STYLE;
                    OneRecord.WorkShop = item.WS_NAME;
                    OneRecord.PlanDate = Convert.ToDateTime(item.PLAN_DATE);
                    //get output from case label table
                    double output1 = (db.GetDailyOutput(item.GROUP_ID ,item.PLAN_DATE ,item.STYLE).First() != null ? db.GetDailyOutput(item.GROUP_ID ,item.PLAN_DATE ,item.STYLE).First().Value : 0);
                    double output2 = (db.GetDailyOutputByWLLoc(item.GROUP_ID ,item.PLAN_DATE ,item.STYLE).First() != null ? db.GetDailyOutputByWLLoc(item.GROUP_ID ,item.PLAN_DATE ,item.STYLE).First().Value : 0);
                    OneRecord.Output = output1 + output2;

                    TmpReport.Add(OneRecord);
                }

                foreach(var item in TmpReport) {
                    if(Report.Count == 0) {
                        DailyLocReport OneRecord = new DailyLocReport();
                        OneRecord.GroupID = item.GroupID;
                        Report.Add(OneRecord);
                    } else {
                        bool exist = false;
                        for(int i = 0; i < Report.Count; i++) {
                            if(item.GroupID == Report[i].GroupID) {
                                exist = true;
                                break;

                            }
                        }
                        if(!exist) {
                            DailyLocReport OneRecord = new DailyLocReport();
                            OneRecord.GroupID = item.GroupID;
                            Report.Add(OneRecord);
                        }
                    }
                }

                foreach(var R in Report) {
                    foreach(var TmpR in TmpReport) {
                        if(R.GroupID == TmpR.GroupID) {
                            R.GroupName = TmpR.GroupName;
                            R.Output = R.Output + TmpR.Output;
                            R.PlantName = TmpR.PlantName;
                            R.Target = R.Target + TmpR.Target;
                            R.TotalHour = R.TotalHour + (TmpR.Labor * 7.5);
                            R.TotalSAH = R.TotalSAH + (TmpR.Output * TmpR.SAH);
                            R.WorkShop = TmpR.WorkShop;
                        }
                    }
                }
                List<PROC_ACTUAL_SCAN_OUTPUT_BY_RANGE_DATE_Result> WSOutput = (from item in db.GetActualScanOutputByRangeDate(From_date ,To_date) select item).ToList();
                ViewBag.WSOutput = WSOutput;
            }
            return View("Location" ,Report);

        }

        public ActionResult WorkCentral(FormCollection fc) {
            var fromDate = (fc["txtFrom"]);
            var toDate = (fc["txtTo"]);
            var wsID = (fc["listWC"]);
            List<PROC_GET_WC_REPORT_BY_DATE_WCID_Result> WcRecord = new List<PROC_GET_WC_REPORT_BY_DATE_WCID_Result>();
            if(fromDate != null && toDate != null && wsID != null) {
                WcRecord = (from item in db.GetWcReportByDateWcID(Convert.ToDateTime(fromDate) ,Convert.ToDateTime(toDate) ,wsID.ToString()) select item).ToList();
            }
            IEnumerable<PROC_GET_ALL_WC_Result> lsApprovers = (from item in db.GetAllWc() select item);
            IEnumerable<PROC_GET_ALL_WC_Result> listWC = (from item in db.GetAllWc() select item);
            ViewBag.listWC = new SelectList(listWC ,"ID" ,"WC_GROUP");
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.LstWCID = wsID;
            return View("WorkCentral" ,WcRecord);
        }

        public ActionResult Locationv2(FormCollection fc) {
            DateTime From_date = Convert.ToDateTime(fc["TxtFrom"]); //Convert.ToDateTime("02/26/2017");
            DateTime To_date = Convert.ToDateTime(fc["TxtTo"]); //DateTime.Now; //fc[""];
            int LaborSrc = Convert.ToInt32(fc["optLaborSrc"]);
            List<PROC_GET_TOTAL_SAH_BY_DATE_Result> TotalSah = (from item in db.GetTotalSahByDate(From_date ,To_date) select item).ToList();

            List<DailyLocReport> Report = new List<DailyLocReport>();
            // distinct group
            foreach(var item in TotalSah) {
                if(Report.Count == 0) {
                    DailyLocReport oneRow = new DailyLocReport();
                    oneRow.GroupID = item.GROUP_ID;
                    Report.Add(oneRow);
                } else {
                    bool exist = false;
                    for(int i = 0; i < Report.Count; i++) {
                        if(item.GROUP_ID == Report[i].GroupID) {
                            exist = true;
                            break;
                        }
                    }
                    if(!exist) {
                        DailyLocReport OneRecord = new DailyLocReport();
                        OneRecord.GroupID = item.GROUP_ID;
                        Report.Add(OneRecord);
                    }
                }
            }

            //CACULATE DATA TO REPORT
            foreach(var R in Report) {
                foreach(var TmpR in TotalSah) {
                    if(R.GroupID == TmpR.GROUP_ID) {
                        R.GroupName = TmpR.GROUP_NAME;
                        R.Output = R.Output + Convert.ToDouble(TmpR.TOTAL_QTY);
                        //R.PlantName = 
                        //R.Target = 
                        //R.TotalHour = 
                        R.TotalSAH = R.TotalSAH + (Convert.ToDouble(TmpR.TOTAL_QTY) * TmpR.SAH);
                        //R.WorkShop = 
                    }
                }
            }

            //ADD MORE INFORMATION
            if(LaborSrc == 1) {
                foreach(var item in Report) {
                    PROC_GET_TARGET_LABOR_BY_DATE_GROUP_Result TG_LB = (from t in db.GetTargetLaborByDateGroup(From_date ,To_date ,item.GroupID) select t).SingleOrDefault();
                    PROC_GET_WS_PLANT_BY_GROUPID_Result WS_PL = (from r in db.GetWSPlanByGroupID(item.GroupID) select r).SingleOrDefault();
                    item.PlantName = WS_PL.PL_NAME;
                    item.WorkShop = WS_PL.WS_NAME;
                    item.Target = Convert.ToDouble(TG_LB.TOTAL_TARGET);
                    item.TotalHour = Convert.ToDouble(TG_LB.TOTAL_LABOR) * 7.5;
                    item.TotalLabor = Convert.ToDouble(TG_LB.TOTAL_LABOR);
                }

            } else {
                foreach(var item in Report) {
                    PROC_GET_HR_LABOR_BY_DATE_GROUP_Result a = (from l in db.GetHrLaborByDateGroup(From_date ,To_date ,item.GroupID) select l).SingleOrDefault();
                    double HR_total_labor = Convert.ToDouble(a != null ? a.TOTAL_LABOR : 0);
                    double HR_total_OT = Convert.ToDouble(a != null ? a.TOTAL_OT : 0);
                    PROC_GET_TARGET_LABOR_BY_DATE_GROUP_Result TG_LB = (from t in db.GetTargetLaborByDateGroup(From_date ,To_date ,item.GroupID) select t).SingleOrDefault();
                    PROC_GET_WS_PLANT_BY_GROUPID_Result WS_PL = (from r in db.GetWSPlanByGroupID(item.GroupID) select r).SingleOrDefault();
                    item.PlantName = WS_PL.PL_NAME;
                    item.WorkShop = WS_PL.WS_NAME;
                    item.Target = Convert.ToDouble(TG_LB.TOTAL_TARGET);
                    item.TotalHour = (Convert.ToDouble(HR_total_labor) * 7.5) + HR_total_OT;
                    item.TotalLabor = Convert.ToDouble(HR_total_labor);
                }
            }


            /// get actual output by WS Group
            List<PROC_ACTUAL_SCAN_OUTPUT_BY_RANGE_DATE_GROUP_Result> WSOutput = (from item in db.GetActualScanOutputByRangedateGroup(From_date ,To_date) select item).ToList();
            ViewBag.WSOutput = WSOutput;
            // RECORD FROM DATE AND TO DATE

            //ViewBag.FromDate = From_date;
            //ViewBag.ToDate = To_date;
            PostbackValue PostBack = new PostbackValue();
            PostBack.TS_1 = From_date;
            PostBack.TS_2 = To_date;
            ViewBag.FromDate = From_date;
            ViewBag.ToDate = To_date;
            PostBack.num = LaborSrc;
            ViewBag.PostBack = PostBack;

            return View("LocationV2" ,Report);

        }
        //============================== END REPORT BY LOCATION V2 ==========

        // REALTIME OUTPUT NEW
        //public ActionResult Realtime()
        //{

        //    DateTime CurrTime = DateTime.Now;
        //    PROC_GET_TIMEFRAME_BY_DATE_Result TimeFrRecord = (from item in db.GetTimeFrameByDate(CurrTime) select item).SingleOrDefault();
        //    PROC_GET_OUTPUT_FRAME_EXIST_Result OutputFrExist = new PROC_GET_OUTPUT_FRAME_EXIST_Result();
        //    if (TimeFrRecord != null)
        //    {
        //        OutputFrExist = (from item in db.GetOutputFrameExist(TimeFrRecord.FRAME_ID) select item).SingleOrDefault();
        //    }

        //    if (OutputFrExist == null)
        //    {
        //        List<PROC_GET_REALTIME_OUTPUT_DATA_Result> PrdPlanRecord = (from item in db.GetRealtimeOutputData() select item).ToList();
        //        if (PrdPlanRecord != null)
        //        {
        //            List<TmpRealtimeReport> TmpReport = new List<TmpRealtimeReport>();
        //            foreach (var item in PrdPlanRecord)
        //            {
        //                TmpRealtimeReport OneRecord = new TmpRealtimeReport();
        //                OneRecord.GroupID = Convert.ToInt32( item.GROUP_ID);
        //                OneRecord.GroupName = item.GROUP_NAME;
        //                OneRecord.PlantName = item.PLANT_NAME;
        //                OneRecord.Labor = Convert.ToDouble((from tar in db.GetRealtimeLaborTargetByGroup(item.GROUP_ID) select tar.LABOR).SingleOrDefault());
        //                OneRecord.SAH = Convert.ToDouble(item.SAH);
        //                OneRecord.Target = Convert.ToDouble((from tar in db.GetRealtimeLaborTargetByGroup(item.GROUP_ID) select tar.TARGET_QTY).SingleOrDefault());
        //                OneRecord.Style = item.STYLE;
        //                OneRecord.WorkShop = item.WS_NAME;
        //                OneRecord.ShiftID = item.SHIFT_ID;
        //                //get output from case label table
        //                //double output1 = (db.GetRealtimeOutput(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First() != null ? db.GetRealtimeOutput(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First().Value : 0);
        //                //double output2 = (db.GetRealtimeOutputWLLoc(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First() != null ? db.GetRealtimeOutputWLLoc(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First().Value : 0);
        //                OneRecord.Output = Convert.ToDouble(item.TOTAL_QTY);

        //                TmpReport.Add(OneRecord);
        //            }
        //            // DISTINCT GROUP ID 
        //            List<RealtimeReport> Report = new List<RealtimeReport>();
        //            foreach (var item in TmpReport)
        //            {
        //                if (Report.Count == 0)
        //                {
        //                    RealtimeReport OneRecord = new RealtimeReport();
        //                    OneRecord.GroupID = item.GroupID;
        //                    Report.Add(OneRecord);
        //                }
        //                else
        //                {
        //                    bool exist = false;
        //                    for (int i = 0; i < Report.Count; i++)
        //                    {
        //                        if (item.GroupID == Report[i].GroupID)
        //                        {
        //                            exist = true;
        //                            break;

        //                        }
        //                    }
        //                    if (!exist)
        //                    {
        //                        RealtimeReport OneRecord = new RealtimeReport();
        //                        OneRecord.GroupID = item.GroupID;
        //                        Report.Add(OneRecord);
        //                    }
        //                }
        //            }

        //            // CACULATE DATA
        //            foreach (var R in Report)
        //            {
        //                foreach (var TmpR in TmpReport)
        //                {
        //                    if (R.GroupID == TmpR.GroupID)
        //                    {
        //                        R.GroupName = TmpR.GroupName;
        //                        R.Output = R.Output + TmpR.Output;
        //                        R.PlantName = TmpR.PlantName;
        //                        R.ShiftID = TmpR.ShiftID;

        //                        //R.Target = R.Target + TmpR.Target;

        //                        R.TotalHour = (TmpR.Labor * (Convert.ToDouble(TimeFrRecord.PIECE) * 0.5));
        //                        R.TotalSAH = R.TotalSAH + (TmpR.Output * TmpR.SAH);
        //                        R.WorkShop = TmpR.WorkShop;
        //                    }
        //                }
        //            }

        //            // ADD FOR ALL GROUP ON 28/01/2018
        //            int CurShift = (from item in db.GetCurrentShift() select item.SHIFT_ID).SingleOrDefault();
        //            List<RealtimeReportAllData> RealtimeAllDtata = db.GetAllGroup().Select(x => new RealtimeReportAllData()
        //            {
        //                PlantName = x.NAME,
        //                WorkShop = x.WS_NAME,
        //                GroupName = x.GROUP_NAME,
        //                GroupID = x.GROUP_ID,
        //                Target = Convert.ToDouble((from item in db.GetPlanByDateShiftGroup(DateTime.Now, CurShift, x.GROUP_ID) select item.TARGET_QTY).SingleOrDefault()),
        //                TotalLabor = Convert.ToDouble((from item in db.GetPlanByDateShiftGroup(DateTime.Now, CurShift, x.GROUP_ID) select item.LABOR).SingleOrDefault()),

        //            }).ToList();

        //            foreach (var item in RealtimeAllDtata)
        //            {
        //                foreach (var RealtimeUp in Report)
        //                {
        //                    if (item.GroupName == RealtimeUp.GroupName)
        //                    {
        //                        item.Output = Convert.ToDouble(RealtimeUp.Output);
        //                        item.TotalSAH = RealtimeUp.TotalSAH;
        //                        //item.TotalHour = Realtime
        //                    }
        //                }
        //            }

        //            // INSERT TO DATABASE
        //            foreach (var item in RealtimeAllDtata)
        //            {
        //                TBL_OUTPUT_UPDATE outputupdate = new TBL_OUTPUT_UPDATE();
        //                outputupdate.ACTUAL = Math.Round(item.Output, 2);
        //                outputupdate.TARGET = Math.Round(item.Target,2);
        //                double TotalHour = (item.TotalLabor * (Convert.ToDouble(TimeFrRecord.PIECE) * 0.5));

        //                var b = Convert.ToString(Math.Round((item.TotalSAH / TotalHour), 2).ToString());
        //                if(b=="Infinity" || b=="NaN")
        //                {
        //                    outputupdate.EFFICIENCY = 0;
        //                }
        //                else
        //                {
        //                outputupdate.EFFICIENCY = Convert.ToDouble(b);
        //                }
        //                //outputupdate.EFFICIENCY = (double.TryParse((Math.Round((item.TotalSAH / TotalHour), 2).ToString()), out b) == true ? Convert.ToDouble(b) : 0);

        //                outputupdate.FRAME_ID = TimeFrRecord.FRAME_ID;
        //                outputupdate.GROUP_NAME = item.GroupName;
        //                outputupdate.PLANT_NAME = item.PlantName;
        //                outputupdate.SHIFT_NAME = Convert.ToString(item.ShiftID);
        //                outputupdate.WS_NAME = item.WorkShop;

        //                outputupdate.TS_1 = DateTime.Now;
        //                outputupdate.PIECE = Convert.ToDouble(TimeFrRecord.PIECE);
        //                outputupdate.WL_OVER_7DAYS = db.GetNonReconcByGroup(item.GroupID).FirstOrDefault().Value;
        //                // COLOR
        //                if (TimeFrRecord.SHIFT_BEGIN == 1)
        //                    outputupdate.COLOR = "lawngreen";
        //                else
        //                if (((outputupdate.TARGET / 16) * TimeFrRecord.PIECE) > item.Output)
        //                    outputupdate.COLOR = "orangered";
        //                else
        //                    outputupdate.COLOR = "lawngreen";
        //                db.TBL_OUTPUT_UPDATE.Add(outputupdate);
        //                db.SaveChanges();

        //            }
        //        }
        //    }

        //    List<PROC_GET_REALTIME_UPDATE_Result> realtimeUpdate = (from item in db.GetRealtimeUpdate(TimeFrRecord.FRAME_ID) select item).ToList();




        //    //IEnumerable<PROC_GET_ALL_WS_Result> listWs = (from item in db.GetAllWS() select item);
        //    //ViewBag.listWs = new SelectList(listWs, "WSHOP_ID", "NAME");
        //    return View("Realtime", realtimeUpdate);
        //}


        public ActionResult MismatchWhProd(FormCollection fc) {
            if(fc["TxtFrom"] != null && fc["TxtTo"] != null) {
                DateTime From_date = Convert.ToDateTime(fc["TxtFrom"]); //Convert.ToDateTime("02/26/2017");
                DateTime To_date = Convert.ToDateTime(fc["TxtTo"]); //DateTime.Now; //fc[""];
                double TOTAL_PROD = (db.GetTotalProdQtyByDate(From_date ,To_date).First() != null ? db.GetTotalProdQtyByDate(From_date ,To_date).First().Value : 0);
                double TOTAL_WH = (db.GetTotalWHQtyByDate(From_date ,To_date).First() != null ? db.GetTotalWHQtyByDate(From_date ,To_date).First().Value : 0);

                List<PROC_GET_MISS_PROD_CASE_BY_DATE_Result> MisProd = (from item in db.GetMissProdCaseByDate(From_date ,To_date) select item).ToList();
                List<PROC_GET_MISS_WH_CASE_BY_DATE_Result>  MisWH = (from item in db.PROC_GET_MISS_WH_CASE_BY_DATE(From_date ,To_date) select item).ToList();

                ViewBag.TotalProd = TOTAL_PROD;
                ViewBag.TotalWH = TOTAL_WH;
                ViewBag.MisProd = MisProd;
                ViewBag.MisWH = MisWH;
            }
            return View("MismatchWhProd");
        }

        public ActionResult CaseMisSAH(FormCollection fc) {
            List<CASE_MIS_SAH> MisSHA = db.GetCaseMisSAH().Select(X => new CASE_MIS_SAH {
                LABEL_ID = X.LABEL_ID ,
                WLOT_ID = X.WLOT_ID ,
                STYLE = X.STYLE ,
                SIZE = X.SIZE ,
                COLOR = X.COLOR ,
                GROUP_NAME = (from item in db.GetWSPlanByGroupID(X.GROUP_ID) select item.GROUP_NAME).SingleOrDefault() ,
                TS_1 = X.TS_1 ,
                TS_2 = X.TS_2 ,
                TS_1_USER = X.TS_1_USER ,
                TS_2_USER = X.TS_2_USER ,
                STATUS = X.STATUS ,
                QUANTITY = X.QUANTITY

            }
            ).ToList();

            return View("CaseMisSAH" ,MisSHA);
        }

        public ActionResult StyleMisSAH(FormCollection fc) {
            List<PROC_GET_STYLE_MIS_SAH_Result> MisSHA = (from item in db.GetStyleMisSah() select item).ToList();

            return View("StyleMisSAH" ,MisSHA);
        }

        public ActionResult Realtime() {

            DateTime CurrTime = DateTime.Now;
            PROC_GET_TIMEFRAME_BY_DATE_Result TimeFrRecord = (from item in db.GetTimeFrameByDate(CurrTime) select item).SingleOrDefault();
            PROC_GET_OUTPUT_FRAME_EXIST_Result OutputFrExist = new PROC_GET_OUTPUT_FRAME_EXIST_Result();
            if(TimeFrRecord != null) {
                OutputFrExist = (from item in db.GetOutputFrameExist(TimeFrRecord.FRAME_ID) select item).SingleOrDefault();
            }
            //Nếu tìm trong bảng TBL_OUTPUT_UPDATE không thấy có dữ liệu, thì tạo Data cho time frame mới
            if(OutputFrExist == null) {
                List<PROC_GET_REALTIME_OUTPUT_DATA_Result> PrdPlanRecord = (from item in db.GetRealtimeOutputData() select item).ToList();
                if(PrdPlanRecord != null) {
                    List<TmpRealtimeReport> TmpReport = new List<TmpRealtimeReport>();
                    foreach(var item in PrdPlanRecord) {
                        TmpRealtimeReport OneRecord = new TmpRealtimeReport();
                        OneRecord.GroupID = Convert.ToInt32(item.GROUP_ID);
                        OneRecord.GroupName = item.GROUP_NAME;
                        OneRecord.PlantName = item.PLANT_NAME;
                        OneRecord.Labor = Convert.ToDouble((from tar in db.GetRealtimeLaborTargetByGroup(item.GROUP_ID) select tar.LABOR).SingleOrDefault());
                        OneRecord.SAH = Convert.ToDouble(item.SAH);
                        OneRecord.Target = Convert.ToDouble((from tar in db.GetRealtimeLaborTargetByGroup(item.GROUP_ID) select tar.TARGET_QTY).SingleOrDefault());
                        OneRecord.Style = item.STYLE;
                        OneRecord.WorkShop = item.WS_NAME;
                        OneRecord.ShiftID = item.SHIFT_ID;
                        //get output from case label table
                        //double output1 = (db.GetRealtimeOutput(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First() != null ? db.GetRealtimeOutput(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First().Value : 0);
                        //double output2 = (db.GetRealtimeOutputWLLoc(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First() != null ? db.GetRealtimeOutputWLLoc(item.GROUP_ID, item.SHIFT_ID, item.STYLE).First().Value : 0);
                        OneRecord.Output = Convert.ToDouble(item.TOTAL_QTY);

                        TmpReport.Add(OneRecord);
                    }

                    //Tạo list Report Real Time new
                    List<RealtimeReport> Report = new List<RealtimeReport>();

                    //Step 1: Add Group ID (Tên tổ)
                    foreach(var item in TmpReport) {
                        if(Report.Count == 0) {
                            RealtimeReport OneRecord = new RealtimeReport();
                            OneRecord.GroupID = item.GroupID;
                            Report.Add(OneRecord);
                        } else {
                            bool exist = false;
                            for(int i = 0; i < Report.Count; i++) {
                                if(item.GroupID == Report[i].GroupID) {
                                    exist = true;
                                    break;

                                }
                            }
                            if(!exist) {
                                RealtimeReport OneRecord = new RealtimeReport();
                                OneRecord.GroupID = item.GroupID;
                                Report.Add(OneRecord);
                            }
                        }
                    }
                    //Step 2: add nội dung, với group là tên tổ
                    foreach(var R in Report) {
                        foreach(var TmpR in TmpReport) {
                            if(R.GroupID == TmpR.GroupID) {
                                R.GroupName = TmpR.GroupName;
                                R.Output = R.Output + TmpR.Output;
                                R.PlantName = TmpR.PlantName;
                                R.ShiftID = TmpR.ShiftID;

                                //R.Target = R.Target + TmpR.Target;

                                R.TotalHour = (TmpR.Labor * (Convert.ToDouble(TimeFrRecord.PIECE) * 0.5));
                                R.TotalSAH = R.TotalSAH + (TmpR.Output * TmpR.SAH);
                                R.WorkShop = TmpR.WorkShop;
                            }
                        }
                    }

                    //Step 3: Lưu vào trong bảng TBL_OUTPUT_UPDATE
                    foreach(var item in Report) {
                        TBL_OUTPUT_UPDATE outputupdate = new TBL_OUTPUT_UPDATE();
                        outputupdate.ACTUAL = Math.Round(item.Output ,2);
                        //outputupdate.TARGET = Math.Round(item.Target, 2);
                        outputupdate.TARGET = Math.Round(Convert.ToDouble((from tar in db.GetRealtimeLaborTargetByGroup(item.GroupID) select tar.TARGET_QTY).SingleOrDefault()) ,2);
                        //Tinh toan Sah và Efficiency
                        string b = (Math.Round((item.TotalSAH / item.TotalHour) ,2).ToString());
                        if(b == "Infinity") {
                            outputupdate.EFFICIENCY = 0;
                        } else { outputupdate.EFFICIENCY = Convert.ToDouble(b); }

                        //outputupdate.EFFICIENCY = (double.TryParse((Math.Round((item.TotalSAH / item.TotalHour), 2).ToString()), out b) ==true ? Convert.ToDouble(b) : 0);
                        outputupdate.FRAME_ID = TimeFrRecord.FRAME_ID;
                        outputupdate.GROUP_NAME = item.GroupName;
                        outputupdate.PLANT_NAME = item.PlantName;
                        outputupdate.SHIFT_NAME = Convert.ToString(item.ShiftID);
                        outputupdate.WS_NAME = item.WorkShop;

                        outputupdate.TS_1 = DateTime.Now;
                        outputupdate.PIECE = Convert.ToDouble(TimeFrRecord.PIECE);
                        outputupdate.WL_OVER_7DAYS = db.GetNonReconcByGroup(item.GroupID).FirstOrDefault().Value;
                        if(TimeFrRecord.SHIFT_BEGIN == 1)
                            outputupdate.COLOR = "lawngreen";
                        else
                            if(((outputupdate.TARGET / 16) * TimeFrRecord.PIECE) > item.Output)
                                outputupdate.COLOR = "orangered";
                            else
                                outputupdate.COLOR = "lawngreen";
                        db.TBL_OUTPUT_UPDATE.Add(outputupdate);
                        db.SaveChanges();

                    }
                }
            }
            List<PROC_GET_REALTIME_UPDATE_Result> realtimeUpdate = (from item in db.GetRealtimeUpdate(TimeFrRecord.FRAME_ID) select item).ToList();
            //IEnumerable<PROC_GET_ALL_WS_Result> listWs = (from item in db.GetAllWS() select item);
            //ViewBag.listWs = new SelectList(listWs, "WSHOP_ID", "NAME");
            return View("Realtime" ,realtimeUpdate);
        }
    }
}



