using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class OtExportRequestPending
    {
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string Line { get; set; }
        public string EmpSubmit { get; set; }
        public string OTDate { get; set; }
        public DateTime OTTimeIn { get; set; }
        public string OTDate1 { get; set; }
        public DateTime OTTimeOut { get; set; }
        public string OTCD { get; set; }
        public double OTNo { get; set; }
        public string Reason {  get;  set;  }
        public int? Van  {  get;  set;  }
        public string VanToAdd { get; set; }  
        public string ApproverName { get; set; }
        public string ApproverEmail { get; set; }
        public string status { get; set; } 
        public string cr { get; set; }
    } public class OtExportRequest
    {
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string Line { get; set; }
        public string EmpSubmit { get; set; }
        public string OTDate { get; set; }
        public DateTime OTTimeIn { get; set; }
        public string OTDate1 { get; set; }
        public DateTime OTTimeOut { get; set; }
        public string OTCD { get; set; }
        public double OTNo { get; set; }
        public string Reason { get; set; }
        public int? Van { get; set; }
        public string VanToAdd { get; set; }
        public string cr {  get;  set;  }
    }
}