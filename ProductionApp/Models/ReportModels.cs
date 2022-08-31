using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class ReportModels
    {
    }
    public class DailyReport
    {
        public int Plant { set; get; }
        public int WorkShop { set; get; }
        public int TotalCase { set; get; }
        public double TotalQty { set; get; }
    }
    public class TmpRealtimeReport
    {
        public string PlantName { set; get; }
        public string WorkShop { set; get; }
        public int GroupID { set; get; }
        public string GroupName { set; get; }
        public int ShiftID { set; get; }
        public double SAH { set; get; }
        public double Target { set; get; }
        public double Labor { set; get; }
        public string Style { set; get; }
        public DateTime PlanDate { set; get; }
        public double Output { set; get; }

    }

    public partial class CASE_MIS_SAH
    {
        public string WLOT_ID { get; set; }
        public Nullable<double> QUANTITY { get; set; }
        public string TYPE { get; set; }
        public string STYLE { get; set; }
        public string COLOR { get; set; }
        public string SIZE { get; set; }
        public Nullable<int> STATUS { get; set; }
        public Nullable<int> GROUP_ID { get; set; }
        public Nullable<int> SHIFT_ID { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public Nullable<System.DateTime> TS_2 { get; set; }
        public string TS_1_USER { get; set; }
        public string TS_2_USER { get; set; }
        public string LABEL_ID { get; set; }
        public string GROUP_NAME { get; set; }
        public int WSHOP_ID { get; set; }
    }


    public class RealtimeReport
    {
        public string PlantName { set; get; }
        public string WorkShop { set; get; }
        public int GroupID { set; get; }
        public string GroupName { set; get; }
        public int ShiftID { set; get; }
        public double Target { set; get; }
        public double Output { set; get; }
        public double TotalSAH { set; get; }
        public double TotalHour { set; get; }
    }

    public class RealtimeReportAllData
    {
        public string PlantName { set; get; }
        public string WorkShop { set; get; }
        public int GroupID { set; get; }
        public string GroupName { set; get; }
        public int ShiftID { set; get; }
        public double Target { set; get; }
        public double Output { set; get; }
        public double TotalSAH { set; get; }
        public double TotalHour { set; get; }
        public double TotalLabor { set; get; }
        public double DPM { set; get; }
        public double LastTarget { set; get; }
        public double LastOutput { set; get; }
        public double Last_totalSAH { set; get; }
        public double Last_totalHour { set; get; }
        public double LastTotalLabor { set; get; }
        public double LastDMP { set; get; }


    }

    public class DailyLocReport
    {
        public string PlantName { set; get; }
        public string WorkShop { set; get; }
        public int GroupID { set; get; }
        public string GroupName { set; get; }
        public double Output_Scanner { set; get; }
        public double Target { set; get; }
        public double Output { set; get; }
        public double TotalSAH { set; get; }
        public double TotalHour { set; get; }
        public double TotalLabor { set; get; }
    }
    public class PostbackValue
    {
        public string str { set; get; }
        public int num { set; get; }
        public int num2 { set; get; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public Nullable<System.DateTime> TS_2 { get; set; }
    }
}