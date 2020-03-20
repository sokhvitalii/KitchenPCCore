using System;
using System.Collections.Generic;
using KitchenPC.Recipes;

namespace KitchenPC.WebApi.Model
{

    public class QuantityRequest
    {
        public string Unit { get; set; }
        public float Size { get; set; }
        
        public QuantityRequest()
        {
        }
    }

    public class IngredientsRequest
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public int Id { get; set; }
        public string Aisle { get; set; }
        public QuantityRequest Quantity { get; set; }
        public string[] PossibleUnits { get; set; }
        
        public IngredientsRequest()
        {
        }
    }

    public class MainIngredientRequest
    {
        public string Name { get; set; }
        public int Id { get; set; }
        
        public MainIngredientRequest()
        {
        }
    }

    public class CreateRecipeRequest
    {
        
        public IngredientsRequest[] Ingredients { get; set; }
        
        public MainIngredientRequest MainIngredient { get; set; }
        public int CookTime { get; set; }
        public string Steps { get; set; }
        public int PrepTime { get; set; }
        public Rating Rating { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Credit { get; set; }
        public string CreditUrl { get; set; }
        public DateTime DateEntered { get; set; }
        public int ServingSize { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tags { get; set; }
        
        public CreateRecipeRequest()
        {
        }
    }
}