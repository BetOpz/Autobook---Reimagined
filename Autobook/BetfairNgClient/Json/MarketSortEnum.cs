using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketSortEnum
    {
        MINIMUM_TRADED, MAXIMUM_TRADED, MINIMUM_AVAILABLE, MAXIMUM_AVAILABLE, FIRST_TO_START, LAST_TO_START,
    }
}
