using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class ExportExcelModel
    {
        public int Plant_Cd { get; set; }
        public int Work_Shop { get; set; }
        public string Label_ID { get; set; }
        public string Type { get; set; }
        public double Quantity { get; set; }
        public string WorkLot { get; set; }
        public string Selling_Style { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string User_Scan { get; set; }
        public int User_Plant_Cd { get; set; }
        public DateTime Time_Scan { get; set; }
    }

    public class ExportExcelLocationReportModel
    {
        public string GroupName { get; set; }
        public double GroupLabor { get; set; }
        public double Output { get; set; }
        public string Style { get; set; }
        public double SAH { get; set; }
        public string WorkCentral { get; set; }
        public string Construction { get; set; }
        public string Unit { get; set; }
        public int HR_Labor { get; set; }
        public int HR_Absm_Labor { get; set; }
        public int HR_OT { get; set; }
        public string Efficiency { get; set; }
    }
    public class ExportExcelLocationReportModel_DetailDate
    {
        public DateTime Date { get; set; }
        public string GroupName { get; set; }
        public double GroupLabor { get; set; }
        public double Output { get; set; }
        public string Style { get; set; }
        public string Size { get; set; }
        public string WorkCentral { get; set; }
        public string Construction { get; set; }
        public string Unit { get; set; }
    }

    public class ExportExcelLocationReportModel_TMP
    {
        public string GroupName { get; set; }
        public double TOTAL_SAH { get; set; }
        public double TOTAL_HOURS { get; set; }
        public double Efficiency { get; set; }
    }

    public class ExportExcelWCentralReportModel
    {
        public string WorkCentral { get; set; }
        public string WorkShop { get; set; }
        public string GroupName { get; set; }
        public string Style { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Worklot { get; set; }
        public double Quantity { get; set; }
    }
    public class ExportExcelWIPControlModel
    {
        public string Line { get; set; }
        public string SellingStyle { get; set; }
        public string WLOT_ID { get; set; }
        public string LABEL_ID { get; set; }
        public double QUANTITY { get; set; }
        public DateTime DATE { get; set; }
        public string Priority { get; set; }
    }
}