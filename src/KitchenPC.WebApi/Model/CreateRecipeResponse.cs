using System;
using System.Linq;
using KitchenPC.Recipes;

namespace KitchenPC.WebApi.Model
{
    /*public class IngredientsRequest
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Id { get; set; }
        public string Aisle { get; set; }
        public string[] PossibleUnits { get; set; }
        
        public IngredientsRequest()
        {
        }
    }*/

    public class CreateRecipeResponse
    {
        // public IngredientsRequest[] Ingredients { get; set; }
        public int CookTime { get; }
        public string Steps { get; }
        public int PrepTime { get; }
        public Rating Rating { get; }
        public string Description { get; }
        public string Title { get; }
        public string Credit { get; }
        public string CreditUrl { get; }
        public DateTime DateEntered { get; }
        public int ServingSize { get; }
        public string ImageUrl { get; }
        public string[] Tags { get; }

        public CreateRecipeResponse(Recipe recipe)
        {
            if (recipe.CookTime != null) CookTime = (int) recipe.CookTime;
            Steps = recipe.Method;
            if (recipe.PrepTime != null) PrepTime = (int) recipe.PrepTime;
            Rating = recipe.UserRating;
            Description = recipe.Description;
            Title = recipe.Title;
            Credit = recipe.Credit;
            CreditUrl = recipe.CreditUrl;
            DateEntered = recipe.DateEntered;
            ServingSize = recipe.ServingSize;
            ImageUrl = recipe.ImageUrl;
            Tags = recipe.Tags.Select(x => x.ToString()).ToArray();
        }

        public CreateRecipeResponse()
        {
        }
    }
}