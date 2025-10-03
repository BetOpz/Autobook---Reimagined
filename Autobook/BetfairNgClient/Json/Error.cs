using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class Error
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public ErrorData Data { get; set; }
    }
}
