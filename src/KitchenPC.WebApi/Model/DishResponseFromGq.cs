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
        public Guid Id { get; set; }
        public DataDishes()
        {
        }
    }


    public class DishFromGq
    {
        public List<DataDishes> Dish { get; set; }

        public DishFromGq()
        {

        }
    }
    
    public class DishResponseFromGq
    {
        public DishFromGq Data { get; set; }

        public DishResponseFromGq()
        {

        }
    }
}