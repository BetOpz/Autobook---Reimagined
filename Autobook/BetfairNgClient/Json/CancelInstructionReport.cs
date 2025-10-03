using System;
using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class CancelInstructionReport
    {
        [JsonProperty(PropertyName = "status")]
        public InstructionReportStatusEnum Status { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public InstructionReportErrorCodeEnum ErrorCode { get; set; }

        [JsonProperty(PropertyName = "instruction")]
        public CancelInstruction Instruction { get; set; }

        [JsonProperty(PropertyName = "sizeCancelled")]
        public double SizeCancelled { get; set; }

        [JsonProperty(PropertyName = "cancelledDate")]
        public DateTime CancelledDate { get; set; }

    }
}
