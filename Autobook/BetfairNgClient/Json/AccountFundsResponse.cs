using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class AccountFundsResponse
    {
        /// <summary>
        /// Amount available to bet.
        /// </summary>
        [JsonProperty(PropertyName = "availableToBetBalance")]
        public double AvailableToBetBalance { get; set; }

        /// <summary>
        /// Current exposure.
        /// </summary>
        [JsonProperty(PropertyName = "exposure")]
        public double Exposure { get; set; }

        /// <summary>
        /// Sum of retained commission.
        /// </summary>
        [JsonProperty(PropertyName = "retainedCommission")]
        public double RetainedCommission { get; set; }

        /// <summary>
        /// Exposure limit.
        /// </summary>
        [JsonProperty(PropertyName = "exposureLimit")]
        public string ExposureLimit { get; set; }

        /// <summary>
        /// User Discount Rate.
        /// </summary>
        [JsonProperty(PropertyName = "discountRate")]
        public double DiscountRate { get; set; }

        /// <summary>
        /// The Betfair points balance
        /// </summary>
        [JsonProperty(PropertyName = "pointsBalance")]
        public int PointsBalance { get; set; }
    }
}
