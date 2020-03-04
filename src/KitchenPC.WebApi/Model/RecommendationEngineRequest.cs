using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
    public class RecommendationEngineRequest
    {
        [JsonPropertyName("session_variables")]
        public SessionVariables SessionVariables { get; set; }
        
        public RecommendationEngineRequest()
        {
            
        }
    }
}