using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceDataEnum
    {
        SP_AVAILABLE,
        SP_TRADED,
        EX_BEST_OFFERS,
        EX_ALL_OFFERS,
        EX_TRADED,
    }
}
