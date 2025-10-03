using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketProjectionEnum
    {
        COMPETITION,
        EVENT,
        EVENT_TYPE,
        MARKET_START_TIME,
        MARKET_DESCRIPTION,
        RUNNER_DESCRIPTION,
        RUNNER_METADATA
    }
}
