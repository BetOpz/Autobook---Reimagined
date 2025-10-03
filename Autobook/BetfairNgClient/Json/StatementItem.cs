using System;
using System.Collections.Generic;
using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class StatementItem
    {
        [JsonProperty(PropertyName = "refId")]
        public string RefId { get; set; }

        [JsonProperty(PropertyName = "itemDate")]
        public DateTime ItemDate { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public double Balance { get; set; }

        [JsonProperty(PropertyName = "itemClass")]
        public ItemClassEnum ItemClass { get; set; }

        [JsonProperty(PropertyName = "itemClassData")]
        public IDictionary<string, string> ItemClassData { get; set; }

        [JsonProperty(PropertyName = "legacyData")]
        public StatementLegacyData LegacyData { get; set; }
    }
}
