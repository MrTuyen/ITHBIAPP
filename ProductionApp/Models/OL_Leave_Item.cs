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
    
    public partial class OL_Leave_Item
    {
        public int ID { get; set; }
        public int LeaveID { get; set; }
        public string LeaveCD { get; set; }
        public string EmpID { get; set; }
        public Nullable<System.DateTime> FromDate { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }
        public Nullable<double> LeaveNo { get; set; }
        public string LeaveInMorning { get; set; }
        public string Reason { get; set; }
    
        public virtual OL_LeaveBalance OL_LeaveBalance { get; set; }
        public virtual OL_LeaveCode OL_LeaveCode { get; set; }
        public virtual OL_LeaveDetails OL_LeaveDetails { get; set; }
        public virtual OL_User_Approver OL_User_Approver { get; set; }
    }
}