using System.Text.Json;

namespace KitchenPC.WebApi.Common
{
    public class JsonHelper
    {
        public static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
}