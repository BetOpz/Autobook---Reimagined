using System;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoginResponse
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
        [JsonIgnore]
        public bool HasError
        {
            get { return !String.IsNullOrEmpty(Error ); }
        }
    }
}
