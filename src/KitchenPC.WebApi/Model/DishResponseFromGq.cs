using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
    public class PlanItems
    {
        [JsonPropertyName("recipe_id")]
        public Guid RecipeId { get; set; }
        public Guid Id { get; set; }
        public PlanItems()
        {
        }
    }
    
    public class Meal
    {
        public Guid Id { get; set; }
        public List<PlanItems> Dishes { get; set; }
        public Meal()
        {
        }
    }
    
    public class DataPlanItem
    {
        public Guid Id { get; set; }
        public Meal Meal { get; set; }
        public int Servings { get; set; }
        [JsonPropertyName("meal_type")]
        public string MealType { get; set; }
        public DataPlanItem()
        {
        }
    }
    
    public class PlanFromGq
    {
        [JsonPropertyName("plan_items")]
        public List<DataPlanItem> PlanItems { get; set; }
        public Guid Id { get; set; }
        public PlanFromGq()
        {

        }
    }
    public class DataPlanFromGq
    {
        public List<PlanFromGq> Plan { get; set; }
       
        public DataPlanFromGq()
        {

        }
    }

    public class PlanResponseFromGq
    {
        public DataPlanFromGq Data { get; set; }

        public PlanResponseFromGq()
        {

        }
    }
}