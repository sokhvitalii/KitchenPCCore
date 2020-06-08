using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using KitchenPC.WebApi.Model;

namespace KitchenPC.WebApi.Common
{
    public class ShoppingListHelper
    {
        public ShoppingListAdder CalculateServing(List<DataPlanItem> planItems, List<Recipe> recipes)
        {
            Console.WriteLine("\n CalculateServing planItems Count ======== " + planItems.Count());
            Console.WriteLine("\n CalculateServing recipes Count ======== " + recipes.Count());
            foreach (var r in recipes)
            {
                Console.WriteLine("\n CalculateServing recipes ids ======== " + r.Id);
            }
            var newRecipe = new List<Recipe>();
            foreach (var p in planItems)
            {
                Console.WriteLine("\n CalculateServing planItems Id ======== " + p.Id);
                Console.WriteLine("\n CalculateServing planItems Servings ======== " + p.Servings);
                Console.WriteLine("\n CalculateServing Dishes Count ======== " + p.Meal.Dishes.Count);
                foreach (var d in p.Meal.Dishes)
                {
                    var recipe = recipes.Find(p => p.Id == d.RecipeId);
                    recipes.Remove(recipe);
                    Console.WriteLine("\n CalculateServing Dishes id ======== " + d.Id);
                    Console.WriteLine("\n CalculateServing recipe id ======== " + recipe.Id);
                    if (recipe.ServingSize < p.Servings)
                    {
                        Console.WriteLine("\n CalculateServing recipe.ServingSize ======== " + recipe.ServingSize);
                        var newServingSize = p.Servings / recipe.ServingSize;
                        if (p.Servings % recipe.ServingSize != 0)
                            newServingSize += 1;
                        Console.WriteLine("\n CalculateServing newServingSize ======== " + newServingSize);
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            if (ingredient.Amount != null)
                                ingredient.Amount.SizeHigh = ingredient.Amount.SizeHigh * newServingSize;
                        }
                    }
                    newRecipe.Add(recipe);
                }
            }

            return new ShoppingListAdder
            {
                Recipes = newRecipe
            };
        }
        
  
    }
}