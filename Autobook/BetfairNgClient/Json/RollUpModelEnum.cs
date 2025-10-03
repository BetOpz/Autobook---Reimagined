using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RollUpModelEnum
    {
        STAKE, PAYOUT, MANAGED_LIABILITY, NONE
    }
}
