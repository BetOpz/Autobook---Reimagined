using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoginRequest
    {
        [JsonProperty(PropertyName = "username")]
        public object Username { get; set; }
        [JsonProperty(PropertyName = "password")]
        public object Password { get; set; }
    }
}
