using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class INGException
    {
        [JsonProperty(PropertyName = "errorDetails")]
        public string ErrorDetails { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "request UUID")]
        public string RequestUUID { get; set; }
    }
}
