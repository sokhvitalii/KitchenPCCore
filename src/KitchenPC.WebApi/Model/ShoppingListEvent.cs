using System;
using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
    public class Diff
    {
     
        [JsonPropertyName("meal_id")]
        public Guid MealId { get; set; }
        public Guid Id { get; set; }
        [JsonPropertyName("plan_id")]
        public Guid Planid { get; set; }
        public int Servings { get; set; }
        public Diff()
        {
            
        }
    }
    
    public class PlanData
    {
        public Diff Old { get; set; }
        public Diff New { get; set; }
        public PlanData()
        {
            
        }
    } 
    
    public class SessionVariables
    {
        [JsonPropertyName("x-hasura-role")]
        public string HasuraRole { get; set; }
        [JsonPropertyName("x-hasura-user-id")]
        public string HasuraUserId { get; set; }
        public SessionVariables()
        {
            
        }
    }

    public class ShoppingListEntity
    {
        [JsonPropertyName("session_variables")]
        public SessionVariables SessionVariables { get; set; }
        public string Op { get; set; }
        public PlanData Data { get; set; }
        
        public ShoppingListEntity()
        {
            
        }
    }
    
    public class ShoppingListEvent
    {
        public ShoppingListEntity Event { get; set; }
        
        public ShoppingListEvent()
        {
            
        }
    }
}