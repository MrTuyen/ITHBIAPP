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
    
    public partial class HR_Tran_Request
    {
        public int ID { get; set; }
        public string EmpID { get; set; }
        public string FullName { get; set; }
        public string RequestEmail { get; set; }
        public Nullable<int> Dept { get; set; }
        public string UsageDate { get; set; }
        public string Purposes { get; set; }
        public string DepartureTime { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string MgrEmail { get; set; }
        public string MrgName { get; set; }
        public Nullable<int> MgrApproved { get; set; }
        public string MgrApprovedDate { get; set; }
        public string HRName { get; set; }
        public string HREmail { get; set; }
        public Nullable<int> HRApproved { get; set; }
        public string HRApprovedDate { get; set; }
        public Nullable<int> Van { get; set; }
        public string RequestDate { get; set; }
        public Nullable<bool> Active { get; set; }
        public string Reason { get; set; }
    
        public virtual TBL_DEPARTMENT_MST TBL_DEPARTMENT_MST { get; set; }
    }
}
