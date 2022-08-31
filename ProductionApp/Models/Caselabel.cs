using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class Caselabel
    {
    }
    public class ShowScanCase
    {
        public string CaseID { get; set; }
        public string BusinessName { get; set; }
        public string GroupName { get; set; }
        public int count { get; set; }
        public double Qty { get; set; }
    }
    public class GroupModel
    {
        public string GroupName { get; set; }
        public int GroupID { get; set; }
    }
}