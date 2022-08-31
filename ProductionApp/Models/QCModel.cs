using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class EndlineDefects
    {
        public int QCID { get; set; }
        public string Business { get; set; }
        public int EmployeeID { get; set; }
        public int WorkerID { get; set; }
        public string LineID { get; set; }
        public string DefectID { get; set; }
        public int ProcessID { get; set; }
        public int Quantity { get; set; }
        public int totalSamp { get; set; }
        public string IsRework { get; set; }
        public DateTime TS_1 { get; set; }
        public DateTime TS_2 { get; set; }
        public string TS_1_User { get; set; }
        public string TS_2_User { get; set; }
    }

    public class EndlineTotalView
    {
        public string Business { get; set; }
        public long? Total_Sample { get; set; }
        public long? Total_Defect { get; set; }
        public double? OTFQ { get; set; }
        public double DPM { get; set; }
        public double target { get; set; }

    }

    public class EndlineTOP5GroupView
    {
        public string Line_Name { get; set; }
        public string Top5_Defect { get; set; }
        public string Top5_Process { get; set; }
        public double Rate { get; set; }
        public double Audit_Score { get; set; }
    }
    public class EndlineWarningDefectBiz
    {
        public string Business { get; set; }
        public String Defect_Name { get; set; }
        public double Rate { get; set; }
    }

    public class CTQCEndlineFMLDefectCodeQty
    {
        public string Business { get; set; }
        public int BusinessID { get; set; }
        public int Defect_Code { get; set; }
        public string Defect_Name { get; set; }
        public int Qty { get; set; }
        public int Business_Total { get; set; }
        public double Rate { get; set; }
    }

    public class DefectByEmployeeView
    {
        public Int64 WorkerID { get; set; }
        public String FullName { get; set; }
        public string Group { get; set; }
        public int Defect_Qty { get; set; }
        public int Total_Sample { get; set; }
        public double Rate { get; set; }
        public int Rework_Qty { get; set; }
        public int Rework_Samp { get; set; }
        public double Rework_Rate { get; set; }
        public double PAudit_Qty { get; set; }
        public double Efficiency { get; set; }
    }

    public class DefectByWorkLotView
    {
        public string WL { get; set; }
        public string Selling_Style { get; set; }
        public string Group { get; set; }
        public string Defect_ID { get; set; }
        public long Defect_Qty { get; set; }
        public long Total_sample { get; set; }
        public double Rate { get; set; }
        public long Rework_Qty { get; set; }
        public long Rework_Samp { get; set; }
        public double Rework_Rate { get; set; }
        public string Date { get; set; }
    }
}