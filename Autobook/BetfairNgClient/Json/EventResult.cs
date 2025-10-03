using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class EventResult
    {
        [JsonProperty(PropertyName = "event")]
        public Event Event { get; set; }

        [JsonProperty(PropertyName = "marketCount")]
        public int MarketCount { get; set; }

        public override string ToString()
        {
            return $"EventResult: {Event}: MarketCount: {MarketCount}";
        }
    }
}
