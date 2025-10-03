using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetfairNgClient.Json.LiveTennis
{
    public class UpdateContext
    {
        [JsonProperty(PropertyName = "eventTime")]
        public string EventTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty(PropertyName = "updateSequence")]
        public long UpdateSequence { get; set; }

        [JsonProperty(PropertyName = "updateType")]
        public string UpdateType { get; set; }
    }
}
