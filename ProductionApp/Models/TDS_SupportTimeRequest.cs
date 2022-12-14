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
    
    public partial class TDS_SupportTimeRequest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TDS_SupportTimeRequest()
        {
            this.TDS_SupportTimeItems = new HashSet<TDS_SupportTimeItems>();
        }
    
        public int ID { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public string RequestBy { get; set; }
        public string Superintendent { get; set; }
        public string SuperintendentMail { get; set; }
        public Nullable<System.DateTime> SuperintendentDate { get; set; }
        public string LMs { get; set; }
        public string LMsMail { get; set; }
        public Nullable<System.DateTime> LMsDate { get; set; }
        public string Ops { get; set; }
        public string OpsMail { get; set; }
        public Nullable<System.DateTime> OpsDate { get; set; }
        public string HRCB { get; set; }
        public string HRCBMail { get; set; }
        public Nullable<System.DateTime> HRCBDate { get; set; }
        public string HRMgr { get; set; }
        public string HRMgrMail { get; set; }
        public Nullable<System.DateTime> HRMgrDate { get; set; }
        public string PayrollSup { get; set; }
        public string PayrollSupMail { get; set; }
        public Nullable<System.DateTime> PayrollSupDate { get; set; }
        public string ReasonReject { get; set; }
        public Nullable<int> Status { get; set; }
    
        public virtual TBL_USERS_MST TBL_USERS_MST { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TDS_SupportTimeItems> TDS_SupportTimeItems { get; set; }
    }
}
