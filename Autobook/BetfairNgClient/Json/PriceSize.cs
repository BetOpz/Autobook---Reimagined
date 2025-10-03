using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class PriceSize
    {
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "size")]
        public double Size { get; set; }

        public override string ToString()
        {
            return $"{Size}@{Price}";
        }
    }
}
