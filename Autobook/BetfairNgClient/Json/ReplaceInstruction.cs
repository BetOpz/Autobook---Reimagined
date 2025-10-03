using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class ReplaceInstruction
    {
        [JsonProperty(PropertyName = "betId")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "newPrice")]
        public double NewPrice { get; set; }

        [JsonProperty(PropertyName = "newSize")]
        public double? NewSize { get; set; }
    }
}