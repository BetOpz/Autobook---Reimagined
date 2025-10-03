using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class RunnerDescription
    {
        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }

        [JsonProperty(PropertyName = "runnerName")]
        public string RunnerName { get; set; }

        [JsonProperty(PropertyName = "handicap")]
        public double Handicap { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public override string ToString()
        {
            return $"RunnerDescription: SelectionId: {SelectionId}, RunnerName: {RunnerName}, Handicap: {Handicap}, Metadata: {Metadata}";
        }
    }
}
