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
    
    public partial class HR_Travel_Request
    {
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string EmpEmail { get; set; }
        public string Name { get; set; }
        public Nullable<int> Position { get; set; }
        public Nullable<int> Department { get; set; }
        public string Purpose { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureFrom { get; set; }
        public string DepartureTo { get; set; }
        public string ReturnDate { get; set; }
        public string ReturnFrom { get; set; }
        public string ReturnTo { get; set; }
        public string ManagerMail { get; set; }
        public Nullable<int> ManagerApproved { get; set; }
        public string ManagerApprovedDate { get; set; }
        public string SManagerMail { get; set; }
        public Nullable<int> SManagerApproved { get; set; }
        public string SManagerApprovedDate { get; set; }
        public string HREmail { get; set; }
        public Nullable<int> HRApproved { get; set; }
        public string HRApprovedDate { get; set; }
        public string HotelLink { get; set; }
        public string AirTicketLink { get; set; }
        public string Note { get; set; }
        public string RequestDate { get; set; }
        public Nullable<bool> Active { get; set; }
    
        public virtual TBL_DEPARTMENT_MST TBL_DEPARTMENT_MST { get; set; }
        public virtual TBL_Positions_MST TBL_Positions_MST { get; set; }
    }
}
