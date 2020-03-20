using System.Text.Json;

namespace KitchenPC.WebApi.Common
{
    public class JsonHelper
    {
        public string DBHost;
        public string HasuraHost;
        
        public JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public JsonHelper()
        {
            DBHost = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            HasuraHost = System.Configuration.ConfigurationManager.ConnectionStrings["HasuraConnection"].ConnectionString;
            
        }

    }
}