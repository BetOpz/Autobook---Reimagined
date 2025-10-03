using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    class JsonConvert
    {
        public static JsonResponse<T> Import<T>(TextReader reader)
        {
            var jsonResponse = reader.ReadToEnd();
            return Deserialize<JsonResponse<T>>(jsonResponse);
        }

        public static LoginResponse Import(TextReader reader)
        {
            var jsonResponse = reader.ReadToEnd();
            return Deserialize<LoginResponse>(jsonResponse);
        }

        public static EventsMenuResponse ImportMenu(TextReader reader)
        {
            var jsonresponse = reader.ReadToEnd();
            return Deserialize<EventsMenuResponse>(jsonresponse);
        }

        public static T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        //Used for json rpc calls to create a body
        public static void Export(JsonRequest request, TextWriter writer)
        {
            var json = Serialize(request);
            writer.Write(json);
        }

        public static void Export(LoginRequest request, TextWriter writer)
		{
            var json = Serialize(request);
			writer.Write(json);
		}

        public static string Serialize<T>(T value)
		{

			var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore};

			return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
		}
    }
}
