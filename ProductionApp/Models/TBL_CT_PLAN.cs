//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProductionApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TBL_CT_PLAN
    {
        public string WO { get; set; }
        public string ASST { get; set; }
        public string GARMENT_STYLE { get; set; }
        public string MFG_STYLE { get; set; }
        public string SELLING_STYLE { get; set; }
        public string PACKING_STYLE { get; set; }
        public string GARMENT_COLOR { get; set; }
        public string FABRIC_CODE { get; set; }
        public string FABRIC_CODE_2 { get; set; }
        public string SIZE { get; set; }
        public Nullable<double> QUANTITY { get; set; }
        public string NOTE { get; set; }
        public Nullable<System.DateTime> CUT_DUE_DATE { get; set; }
        public Nullable<System.DateTime> CUT_PLAN_DATE { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public string TS_1_USER { get; set; }
    }
}
