using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models.ITAsset
{
    public class FormITAssetSearch
    {
        public int DIVISION { get; set; }
        public int DEPT { get; set; }
        public string USER { get; set; }
        public string TAG { get; set; }
        public string SERIAL { get; set; }
        public string YEAR { get; set; }
    }
}