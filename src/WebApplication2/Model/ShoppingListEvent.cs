using System;
using System.Text.Json.Serialization;

namespace WebApplication2.Model
{
    public class Diff
    {
        [JsonPropertyName("recipe_id")]
        public Guid Recipeid { get; set; }
        public string Meal { get; set; }
        public int Id { get; set; }
        public int Planid { get; set; }
        public string Servings { get; set; }
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