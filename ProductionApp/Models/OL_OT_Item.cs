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
    
    public partial class OL_OT_Item
    {
        public int ID { get; set; }
        public int OT_ID { get; set; }
        public string OTCD { get; set; }
        public string EmpID { get; set; }
        public Nullable<System.DateTime> OTDate { get; set; }
        public Nullable<System.DateTime> OTDate1 { get; set; }
        public string OTTimeIn { get; set; }
        public string OTTimeOut { get; set; }
        public Nullable<double> OTNo { get; set; }
        public string WWork { get; set; }
        public Nullable<int> Van { get; set; }
        public string VanToAdd { get; set; }
        public string Reason { get; set; }
    
        public virtual OL_OT_Details OL_OT_Details { get; set; }
        public virtual OL_OTCode OL_OTCode { get; set; }
        public virtual OL_User_Approver OL_User_Approver { get; set; }
    }
}