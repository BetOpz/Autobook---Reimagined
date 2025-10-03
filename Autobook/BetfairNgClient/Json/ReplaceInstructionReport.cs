using System;
using System.Text;
using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class ReplaceInstructionReport
    {
        [JsonProperty(PropertyName = "status")]
        public InstructionReportStatusEnum Status { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public InstructionReportErrorCodeEnum ErrorCode { get; set; }

        [JsonProperty(PropertyName = "cancelInstructionReport")]
        public CancelInstructionReport CancelInstructionReport { get; set; }

        [JsonProperty(PropertyName = "placeInstructionReport")]
        public PlaceInstructionReport PlaceInstructionReport { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                        .AppendFormat("Status={0}", Status)
                        .AppendFormat(" : ErrorCode={0}", ErrorCode)
                        .AppendFormat(" : CancelInstructionReport={{{0}}}", CancelInstructionReport)
                        .AppendFormat(" : PlaceInstructionReport={{{0}}}", PlaceInstructionReport)
                        .ToString();
        }
    }
}