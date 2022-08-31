using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Controllers
{
    public class LeaveExportRequestPending
    {
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string LeaveCD { get; set; }
        public string EmpSubmit { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string LeaveInMorning { get; set; }
        public double LeaveNo { get; set; }
        public string Note { get; set; }
        public string ApproverName { get; set; }
        public string ApproverEmail { get; set; }
        public string status { get; set; }
        public string cr { get; set; }
    }
    public class LeaveExportRequest
    {
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string LeaveCD { get; set; }
        public string EmpSubmit { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string LeaveInMorning { get; set; }
        public double LeaveNo { get; set; }
        public string Note { get; set; }
        public string cr { get; set; }
    }
}