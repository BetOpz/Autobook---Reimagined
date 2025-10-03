using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemClassEnum
    {
        UNKNOWN,
    }
}
