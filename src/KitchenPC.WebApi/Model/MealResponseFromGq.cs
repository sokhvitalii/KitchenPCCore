using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
    public class DataDishes
    {
        [JsonPropertyName("dish_type")]
        public string DishType { get; set; }
        [JsonPropertyName("recipe_id")]
        public Guid RecipeId { get; set; }
       
        public DataDishes()
        {
        }
    }

    public class DataMeal
    {
        public List<DataDishes> Dishes { get; set; } 
        [JsonPropertyName("meal_type")]
        public string MealType { get; set; }

        public DataMeal()
        {

        }
    }

    public class MealFromGq
    {
        public List<DataMeal> Meal { get; set; }

        public MealFromGq()
        {

        }
    }
    
    public class MealResponseFromGq
    {
        public MealFromGq Data { get; set; }

        public MealResponseFromGq()
        {

        }
    }
}