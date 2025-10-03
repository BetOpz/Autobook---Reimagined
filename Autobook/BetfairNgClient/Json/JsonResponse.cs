using System;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonResponse<T>
    {
        [JsonProperty(PropertyName = "jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Ignore)]
        public T Result { get; set; }

        [JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }

        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }

        [JsonIgnore]
        public bool HasError
        {
            get { return Error != null; }
        }
    }
}

/*

{"jsonrpc":"2.0","error":{"code":-32601,"message":"DSC-0021"},"id":1}

{   "jsonrpc":"2.0",
	"error":{"code":-32099,"message":"SANGX-0004",
	"data":{
		"exceptionname":"APINGException",
		"APINGException":{
			"errorDetails":"",
			"errorCode":"INVALID_APP_KEY",
			"requestUUID":"null"}}},
	"id":1}

*/