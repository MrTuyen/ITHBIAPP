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
using System.Linq.Dynamic;
using System.Data.Entity.SqlServer;
using System.Data.Objects;
using OfficeOpenXml.Style;

namespace ProductionApp.Controllers
{
    public class WIPControlController:BaseController
    {
        public ActionResult WIPControl(FormCollection fc)
        {
            var Date = (fc["txtDate"]);
            var wsID = (fc["listWC"]);
            List<PROC_GET_WIP_CONTROL_BY_GROUP_DATE_Result> WIPControl = new List<PROC_GET_WIP_CONTROL_BY_GROUP_DATE_Result>();
            if (Date != null && wsID != null)
            {
                WIPControl = (from item in db.GetWIPControlByGroupDate(Convert.ToDateTime(Date), wsID.ToString()) select item).ToList();
            }

            IEnumerable<PROC_GET_ALL_GROUP_2_Result> lsApprovers = (from item in db.GetAllGroup2() select item);
            IEnumerable<PROC_GET_ALL_GROUP_2_Result> listWC = (from item in db.GetAllGroup2() select item);
            ViewBag.listWC = new SelectList(listWC, "GROUP_ID", "GROUP_NAME");
            ViewBag.Date = Date;
            ViewBag.LstWCID = wsID;
            return View("WIPControl", WIPControl);
        }
        public ActionResult ExportExcelWIPControlRpt(FormCollection fc)
        {
            DateTime Date = Convert.ToDateTime(fc["txtDate"]); //Convert.ToDateTime("02/26/2017");
            String LstWC = fc["TxtWc"]; //DateTime.Now; //fc[""];

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            List<ExportExcelWIPControlModel> data = db.PROC_GET_WIP_CONTROL_REPORT_BY_GROUP_DATE(Date ,LstWC).Select(x => new ExportExcelWIPControlModel()
            {
                Line = x.LINE,
                SellingStyle = x.SellingStyle,
                WLOT_ID = x.WLOT_ID,
                LABEL_ID = x.LABEL_ID,
                QUANTITY = Convert.ToDouble(x.QUANTITY),
                DATE = Convert.ToDateTime(x.TS_1)
            }).ToList();
            data = data.OrderBy(o => o.Line).ToList();
            workSheet.Cells[1, 1].LoadFromCollection(data, true);

            using (ExcelRange col = workSheet.Cells[1, 1, data.Count + 1, 12])
            //{
            //    col.AutoFitColumns();
            //    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            //    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //}
            {
                col.AutoFitColumns();
                col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            //using (ExcelRange col = workSheet.Cells[2, 6, data.Count + 1, 6])
            //{
            //    col.Style.Numberformat.Format = "MM/dd/yyyy HH:mm:ss";
            //    col.AutoFitColumns();
            //}
            using (ExcelRange col = workSheet.Cells[2, 6, data.Count + 1, 6])
            {
                col.Style.Numberformat.Format = "MM/dd/yyyy HH:mm:ss";
                col.AutoFitColumns();
            }



            workSheet.Cells["A1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet.Cells["A1:G1"].Style.Font.Bold = true;
            workSheet.Cells["A1:G1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet.Cells["A1:G1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

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