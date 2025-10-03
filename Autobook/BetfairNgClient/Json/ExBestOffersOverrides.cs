using System;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class ExBestOffersOverrides
    {
        [JsonProperty(PropertyName = "bestPricesDepth")]
        public int BestPricesDepth { get; set; }

        [JsonProperty(PropertyName = "rollupModel")]
        public RollUpModelEnum RollUpModel { get; set; }

        [JsonProperty(PropertyName = "rollupLimit")]
        public int RollUpLimit { get; set; }

        [JsonProperty(PropertyName = "rollupLiabilityThreshold")]
        public Double RollUpLiabilityThreshold { get; set; }

        [JsonProperty(PropertyName = "rollupLiabilityFactor")]
        public int RollUpLiabilityFactor { get; set; }
    }
}
