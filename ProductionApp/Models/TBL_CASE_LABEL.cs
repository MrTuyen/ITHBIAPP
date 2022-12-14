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
    
    public partial class TBL_CASE_LABEL
    {
        public string LABEL_ID { get; set; }
        public string WLOT_ID { get; set; }
        public Nullable<double> QUANTITY { get; set; }
        public string TYPE { get; set; }
        public string PkgStyle { get; set; }
        public string COLOR { get; set; }
        public string SIZE { get; set; }
        public Nullable<int> STATUS { get; set; }
        public Nullable<int> GROUP_ID { get; set; }
        public Nullable<int> SHIFT { get; set; }
        public string LINE { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public Nullable<System.DateTime> TS_2 { get; set; }
        public string TS_1_USER { get; set; }
        public string TS_2_USER { get; set; }
        public Nullable<int> PLANT_CODE { get; set; }
        public string MnfStyle { get; set; }
        public string SellingStyle { get; set; }
        public string PalletID { get; set; }
        public string WK { get; set; }
    
        public virtual TBL_SHIFT_MST TBL_SHIFT_MST { get; set; }
        public virtual TBL_GROUP_MST TBL_GROUP_MST { get; set; }
    }
}
