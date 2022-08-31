using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models.Recut
{
    public class RecutModel
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("actionType")]
        public int ActionType { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
    }
}