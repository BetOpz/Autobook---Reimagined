using System;
using System.Collections.Generic;
using System.Text;
using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class EventsMenuResponse
    {
        [JsonProperty(PropertyName = "children")]
        public List<EventsMenuResponse> Children { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }
        [JsonProperty(PropertyName = "startTime")]
        public DateTime StartTime { get; set; }
        [JsonProperty(PropertyName = "venue")]
        public string Venue { get; set; }
        [JsonProperty(PropertyName = "marketStartTime")]
        public DateTime MarketStartTime { get; set; }
    }
}
