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
    
    public partial class TBL_CT_QC_FINAL_CONF_DEFECT
    {
        public int ID { get; set; }
        public int FIN_CONF_STT_ID { get; set; }
        public int COMPNT_TYPE { get; set; }
        public Nullable<double> QUANTITY { get; set; }
        public string DEFECT_CODE { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public string TS_1_USER { get; set; }
    }
}
