using System;
using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class RaceDetails
    {
        [JsonProperty(PropertyName = "meetingId")]
        public string MeetingId { get; set; }

        [JsonProperty(PropertyName = "raceId")]
        public string RaceId { get; set; }

        [JsonProperty(PropertyName = "raceStatus")]
        public RaceStatusEnum RaceStatus { get; set; }

        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty(PropertyName = "responseCode")]
        public ResponseCodeEnum ResponseCode { get; set; }

        public override string ToString()
        {
            return $"RaceDetails: RaceId: {RaceId}, RaceStatus: {RaceStatus}, ResponseCode: {ResponseCode}";
        }
    }
}
