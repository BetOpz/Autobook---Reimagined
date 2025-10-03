using BetfairNgClient.Json.Enums;
using BetfairNgClient.Json.LiveTennis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetfairNgClient.Json
{
    public class Score
    {
        [JsonProperty(PropertyName = "eventId")]
        public string EventId { get; set; }

        [JsonProperty(PropertyName = "eventTypeId")]
        public string EventTypeId { get; set; }

        [JsonProperty(PropertyName = "eventStatus")]
        public EventStatusEnum EventStatus { get; set; }

        [JsonProperty(PropertyName = "responseCode")]
        public ResponseCodeEnum ResponseCode { get; set; }

        [JsonProperty(PropertyName = "updateContext")]
        public UpdateContext UpdateContext { get; set; }

        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, string> Values { get; set; }
    }
}
