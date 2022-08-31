using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models.ITAsset
{
    public class CountsheetModel
    {
        public int FirstNumber { get; set; }
        public int SecondNumber { get; set; }
    }

    public class Error_Asset
    {
        public string Tag { get; set; }
        public string Serial { get; set; }
        public string Reason { get; set; }
    }
}