using System;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class Event
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "timezone")]
        public string Timezone { get; set; }

        [JsonProperty(PropertyName = "venue")]
        public string Venue { get; set; }

        [JsonProperty(PropertyName = "openDate")]
        public DateTime? OpenDate { get; set; }

        public override string ToString()
        {
            return $"Event: Id: {Id}, Name: {Name}, CountryCode: {CountryCode}, Venue: {Venue}, Timezone={Timezone}, OpenDate={OpenDate}";
        }
    }
}
