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
    
    public partial class TBL_KANBAN
    {
        public string WLChild { get; set; }
        public string AsstWO { get; set; }
        public Nullable<int> Qty { get; set; }
        public string Priority { get; set; }
        public Nullable<int> Location { get; set; }
        public string SewDate { get; set; }
        public string SellingStyle { get; set; }
        public string Size { get; set; }
        public string MnfColor { get; set; }
        public string MnfStyle { get; set; }
        public string CallDate { get; set; }
        public string CallBy { get; set; }
        public string CPSendDate { get; set; }
        public string CPSendBy { get; set; }
        public Nullable<int> CPTime { get; set; }
        public string SPSendDate { get; set; }
        public string SPSendBy { get; set; }
        public Nullable<int> SPTime { get; set; }
        public string PrdComDate { get; set; }
        public string PrdComBy { get; set; }
        public string CancelDate { get; set; }
        public string CancelBy { get; set; }
        public string UploadDate { get; set; }
        public string UploadBy { get; set; }
        public string CancelReason { get; set; }
    
        public virtual TBL_USERS_MST TBL_USERS_MST { get; set; }
        public virtual TBL_USERS_MST TBL_USERS_MST1 { get; set; }
        public virtual TBL_USERS_MST TBL_USERS_MST2 { get; set; }
        public virtual TBL_USERS_MST TBL_USERS_MST3 { get; set; }
        public virtual TBL_USERS_MST TBL_USERS_MST4 { get; set; }
        public virtual TBL_GROUP_MST TBL_GROUP_MST { get; set; }
    }
}
