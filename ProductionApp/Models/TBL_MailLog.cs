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
    
    public partial class TBL_MailLog
    {
        public int id { get; set; }
        public string mTitle { get; set; }
        public string mFrom { get; set; }
        public string mTo { get; set; }
        public string mCC { get; set; }
        public Nullable<System.DateTime> mDate { get; set; }
        public int mstatus { get; set; }
        public string mContent { get; set; }
    }
}