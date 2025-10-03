using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetfairNgClient.Json.LiveTennis
{
    public class UpdateKey
    {
        [JsonProperty(PropertyName = "eventId")]
        public string EventId { get; set; }

        [JsonProperty(PropertyName="lastUpdateSequenceProcessed")]
        public long LastUpdateSequenceProcessed { get; set; }
    }
}
