using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RescriptRequest
    {

        [JsonProperty(PropertyName = "")]
        public IDictionary<string, object> args { get; set; }

        public RescriptRequest(IDictionary<string, object> args)
        {
            this.args = args;
        }
    }
}
