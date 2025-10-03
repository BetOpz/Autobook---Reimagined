using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class ErrorData
    {
        [JsonProperty(PropertyName = "exceptionname")]
        public string ExceptionName { get; set; }

        [JsonProperty(PropertyName = "AP INGException")]
        public INGException IngException { get; set; }
    }
}
