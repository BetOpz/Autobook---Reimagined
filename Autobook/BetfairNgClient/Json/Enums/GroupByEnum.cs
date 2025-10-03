using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupByEnum
    {
        EVENT_TYPE,
        EVENT,
        MARKET,
        SIDE,
        BET,
    }
}
