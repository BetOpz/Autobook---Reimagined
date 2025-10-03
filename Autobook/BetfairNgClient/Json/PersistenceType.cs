using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersistenceTypeEnum
    {
        LAPSE,				// lapse the order at turn-in-play
        PERSIST,			// put the order into the auction (SP) at turn-in-play
        MARKET_ON_CLOSE,	// put the order into the auction (SP) at turn-in-play
    }
}
