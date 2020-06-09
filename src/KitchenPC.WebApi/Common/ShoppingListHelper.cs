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
            var newRecipe = new List<Recipe>();
            foreach (var r in recipes)
            {
                Console.WriteLine("\n CalculateServing planItems Id ======== " + r.Id);
                var dataPlanItem = planItems
                    .Where(p => p.Meal.Dishes.Exists(x => x.RecipeId == r.Id))
                    .ToList();
                Console.WriteLine("\n CalculateServing dataPlanItem.Count ======== " + dataPlanItem.Count);
                var allServings = dataPlanItem.Aggregate(0, (acc, item) => acc + item.Servings);
                Console.WriteLine("\n CalculateServing recipe.ServingSize ======== " + r.ServingSize);
                Console.WriteLine("\n CalculateServing allServings ======== " +allServings);
                if (r.ServingSize < allServings)
                {
                    var newServingSize = allServings / r.ServingSize;
                    if (allServings % r.ServingSize != 0)
                        newServingSize += 1;
                    Console.WriteLine("\n CalculateServing newServingSize ======== " + newServingSize);

                    foreach (var ingredient in r.Ingredients)
                    {
                        if (ingredient.Amount != null)
                            ingredient.Amount.SizeHigh = ingredient.Amount.SizeHigh * newServingSize;
                    }
                }
                newRecipe.Add(r);
            }
            //
            // foreach (var p in planItems)
            // {
            //     Console.WriteLine("\n CalculateServing planItems Id ======== " + p.Id);
            //     Console.WriteLine("\n CalculateServing planItems Servings ======== " + p.Servings);
            //     Console.WriteLine("\n CalculateServing Dishes Count ======== " + p.Meal.Dishes.Count);
            //     foreach (var d in p.Meal.Dishes)
            //     {
            //         var recipe = recipes.Find(p => p.Id == d.RecipeId);
            //         recipes.Remove(recipe);
            //         Console.WriteLine("\n CalculateServing Dishes id ======== " + d.Id);
            //         Console.WriteLine("\n CalculateServing recipe id ======== " + recipe.Id);
            //         if (recipe.ServingSize < p.Servings)
            //         {
            //             Console.WriteLine("\n CalculateServing recipe.ServingSize ======== " + recipe.ServingSize);
            //             var newServingSize = p.Servings / recipe.ServingSize;
            //             if (p.Servings % recipe.ServingSize != 0)
            //                 newServingSize += 1;
            //             Console.WriteLine("\n CalculateServing newServingSize ======== " + newServingSize);
            //             foreach (var ingredient in recipe.Ingredients)
            //             {
            //                 if (ingredient.Amount != null)
            //                     ingredient.Amount.SizeHigh = ingredient.Amount.SizeHigh * newServingSize;
            //             }
            //         }
            //         newRecipe.Add(recipe);
            //     }
            // }

            return new ShoppingListAdder
            {
                Recipes = newRecipe
            };
        }
        
  
    }
}