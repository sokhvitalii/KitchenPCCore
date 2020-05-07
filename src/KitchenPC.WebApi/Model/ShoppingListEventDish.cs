using System;
using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
    public class DiffDish
    {
     
        [JsonPropertyName("meal_id")]
        public Guid MealId { get; set; }
        public Guid Id { get; set; }
        [JsonPropertyName("recipe_id")]
        public Guid RecipeId { get; set; }
        public string DishType { get; set; }
        public DiffDish()
        {
            
        }
    }
    
    public class PlanDataDish
    {
        public DiffDish Old { get; set; }
        public DiffDish New { get; set; }
        public PlanDataDish()
        {
            
        }
    } 
    
    public class SessionVariablesDish
    {
        [JsonPropertyName("x-hasura-role")]
        public string HasuraRole { get; set; }
        [JsonPropertyName("x-hasura-user-id")]
        public string HasuraUserId { get; set; }
        public SessionVariablesDish()
        {
            
        }
    }

    public class ShoppingListEntityDish
    {
        [JsonPropertyName("session_variables")]
        public SessionVariablesDish SessionVariables { get; set; }
        public string Op { get; set; }
        public PlanDataDish Data { get; set; }
        
        public ShoppingListEntityDish()
        {
            
        }
    }
    
    public class ShoppingListEventDish
    {
        public ShoppingListEntityDish Event { get; set; }
        
        public ShoppingListEventDish()
        {
            
        }
    }
}