using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetfairNgClient.Json.LiveTennis
{
    public class AvailableEvent
    {
        [JsonProperty(PropertyName = "eventId")]
        public string EventId { get; set; }

        [JsonProperty(PropertyName = "eventTypeId")]
        public string EventTypeId { get; set; }

        [JsonProperty(PropertyName = "eventStatus")]
        public EventStatusEnum EventStatus { get; set; }
    }
}
