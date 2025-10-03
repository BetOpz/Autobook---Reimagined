using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchProjectionEnum
    {
        NO_ROLLUP, ROLLED_UP_BY_PRICE, ROLLED_UP_BY_AVG_PRICE
    }
}
