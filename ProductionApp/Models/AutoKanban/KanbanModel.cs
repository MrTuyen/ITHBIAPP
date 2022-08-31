using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models.AutoKanban
{
    public class KanbanModel
    {
        [JsonProperty("assWo")]
        public string AssWo { get; set; }
        [JsonProperty("actionType")]
        public int ActionType { get; set; }
        [JsonProperty("callDate")]
        public string CallDate { get; set; }
        [JsonProperty("locationId")]
        public int LocationId { get; set; }
        [JsonProperty("locationName")]
        public string LocationName { get; set; }
        [JsonProperty("newestAssWo")]
        public string NewestAssWo { get; set; }
    }
}