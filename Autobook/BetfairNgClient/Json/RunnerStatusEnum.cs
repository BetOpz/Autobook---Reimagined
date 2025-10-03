using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RunnerStatusEnum
    {
        ACTIVE, WINNER, LOSER, REMOVED_VACANT, REMOVED
    }

}
