using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionApp.Controllers;

namespace ProductionApp.Models
{
    public class FabricReceivedModel
    {
        public string WO { get; set; }
        public string Status { get; set; }
    }

    public class FabricSpreadModel
    {
        public string WO { get; set; }
        public string MARKER { get; set; }
        public string Status { get; set; }
        public double Qty { get; set; }
        public int Type { get; set; }
    }
    public class ViewCTTableMonitoringModel
    {
        public String WO { get; set; }
        public string MARKER { get; set; }
        public int TableNo { get; set; }
        public int OrderTblID { get; set; }
        public double Quantity { get; set; }
        public string color { get; set; }
        public int type { get; set; }
        public int Status { get; set; }
        public int Process { get; set; }
        public int Shift { get; set; }
    }

    public class ViewCTComponetInspModel
    {
        public string MARKER { get; set; }
     //   public String WO { get; set; }
        public string Garment { get; set; }
       // public int TableNo { get; set; }
     //   public int OrderTblID { get; set; }
        public double Quantity { get; set; }
        public string color { get; set; }
        public int type { get; set; }
        public int Status { get; set; }
        public int Process { get; set; }
        public string ProcessName { get; set; }
        public long AQL { get; set; }
        public int MaxDefect { get; set; }
        public List<PROC_GET_CT_CMPNT_BY_WO_Result> Cmpnt { get; set; }

    }

    public class ViewLastSttComponentModel
    {
        public string Marker { get; set; }
        public int ComponentID { get; set; }
        public string Color { get; set; }
    }

    public class ViewFinConfModel
    {
        public string WO { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }

    public class ViewTransWHModel
    {
        public string WO { get; set; }
        public double Quantity { get; set; }
        public string Status { get; set; }
    }

    public class ViewCTOutputDetail
    {
        public string WO { get; set; }
        public string Assortment { get; set; }
        public string MfgStyle { get; set; }
        public string SellingStyle { get; set; }
        public string Garment { get; set; }
        public string GarmentColor { get; set; }
        public string Business { get; set; }
        public string FabricCode { get; set; }
        public string FabricCode2 { get; set; }
        public string Size { get; set; }
        public Double Quantity { get; set; }
        public DateTime PlanDate { get; set; }
        public string Status { get; set; }
        public DateTime Fabric_Recieved { get; set; }
        public DateTime WipDate { get; set; }
        public DateTime CutDate { get; set; }
        public double CutBody { get; set; }
        public double CutLiner { get; set; }
        public double Produced { get; set; }
        public double TransferWH { get; set; }
        public DateTime TransferDate { get; set; }
        public double Discrapancy { get; set; }
        public string TTS_STT { get; set; }
        public string color { get; set; }
        public string FullAsst { get; set; }

    }

    public class ViewCTOutputTotal
    {
        public double TotalType1 { get; set; }
        public double TotalType2 { get; set; }
        public double TotalProduced { get; set; }
        public double TotalTransWH { get; set; }
        public double TotalMismatch { get; set; }
        public double TotalPlan { get; set; }
        public double TotalFabRcv { get; set; }
    }

    public class ViewTableDefectModel
    {
        public int type { get; set; }
        public int F { get; set; }
        public int M { get; set; }
        public int L { get; set; }
        public int F_Qty { get; set; }
        public int M_Qty { get; set; }
        public int L_Qty { get; set; }
        public DateTime Date { get; set; }
    }

    public class ViewFinalInspDefModel
    {
        public double Qty { get; set; }
        public string DefCode { get; set; }
        public DateTime Date { get; set; }
    }


    public class ViewQCDefTrackingModel
    {
        public string WO { get; set; }
        public DateTime SpreadDate { get; set; }
        public List<ViewTableDefectModel> TblDefect { get; set; }
        public List<ViewFinalInspDefModel> FinalInsp { get; set; }
    }

    public class ViewQCDetailComponentStt
    {
        public string WO { get; set; }
        public int TBL_ID { get; set; }
        public int TBL_NO { get; set; }
        public int Type { get; set; }
        public double Qty { get; set; }
        public DateTime SpreadDate { get; set; }
        public string PartName { get; set; }
        public string PartName_VNM { get; set; }
        public int Status_ID { get; set; }
        public string Status_Name { get; set; }
        public DateTime QC_Checking_Date { get; set; }
        public string QC_User { get; set; }
    }

    public class ViewQCOverview
    {
        public string Business { get; set; }
        public int TotalSample { get; set; }
        public int TBL_NO { get; set; }
        public int Type { get; set; }
        public double Qty { get; set; }
        public DateTime SpreadDate { get; set; }
        public string PartName { get; set; }
        public string PartName_VNM { get; set; }
        public int Status_ID { get; set; }
        public string Status_Name { get; set; }
        public DateTime QC_Checking_Date { get; set; }
        public string QC_User { get; set; }
    }

    public class CTPlantOverview
    {
        public string Business { get; set; }
        public long PlanQty { get; set; }
        public long FabricRcv { get; set; }
        public long SpreadBody { get; set; }
        public long SpreadLiner { get; set; }
        public long Produced { get; set; }
        public long Reject { get; set; }
        public long TransferWH { get; set; }
    }

    public class CTPlantOverviewByShift
    {
        public int Shift { get; set; }
        public string Business { get; set; }
        public long PlanQty { get; set; }
        public long FabricRcv { get; set; }
        public long SpreadBody { get; set; }
        public long SpreadLiner { get; set; }
        public long Produced { get; set; }
        public long Reject { get; set; }
        public long TransferWH { get; set; }
    }

}