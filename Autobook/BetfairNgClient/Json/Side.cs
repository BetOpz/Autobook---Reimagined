using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SideEnum
    {
        BACK, LAY
    }
}
