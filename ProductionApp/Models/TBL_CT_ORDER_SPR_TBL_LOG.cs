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
    
    public partial class TBL_CT_ORDER_SPR_TBL_LOG
    {
        public System.DateTime DateDelete { get; set; }
        public int ORDER_TBL_ID { get; set; }
        public string WO { get; set; }
        public int TBL_NO { get; set; }
        public Nullable<double> QUANTITY { get; set; }
        public Nullable<int> CUTTING_TYPE { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public string TS_1_USER { get; set; }
    }
}