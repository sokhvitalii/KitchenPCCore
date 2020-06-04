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
            var newRecipe = new List<Recipe>();
            foreach (var r in recipes)
            {
                var dataPlanItem = 
                    planItems.Find(p => p.Meal.Dishes.Exists(x => x.RecipeId == r.Id));

                if (r.ServingSize < dataPlanItem.Servings)
                {
                    var newServingSize = dataPlanItem.Servings / r.ServingSize;
                    if (dataPlanItem.Servings % r.ServingSize != 0)
                        newServingSize += 1;

                    foreach (var ingredient in r.Ingredients)
                    {
                        if (ingredient.Amount != null)
                            ingredient.Amount.SizeHigh = ingredient.Amount.SizeHigh * newServingSize;
                    }
                }
                newRecipe.Add(r);
                
            }

            return new ShoppingListAdder
            {
                Recipes = newRecipe
            };
        }

        public ShoppingListAdder CreateShoppingListAdder(int servings, List<Recipe> recipes)
        {
            if (servings > 1)
            {
                foreach (var r in recipes)
                {
                    if (r.ServingSize < servings)
                    {
                        var newServingSize = servings / r.ServingSize;
                        if (servings % r.ServingSize != 0)
                            newServingSize += 1;

                        foreach (var ingredient in r.Ingredients)
                        {
                            if (ingredient.Amount != null)
                                ingredient.Amount.SizeHigh = ingredient.Amount.SizeHigh * newServingSize;
                        }
                    }
                }
            }

            return new ShoppingListAdder
            {
                Recipes = recipes
            };
        }

        public ShoppingListUpdater CreateShoppingListItemUpdater(
            List<ShoppingListItem> query,
            ShoppingListUpdater shoppingListUpdater,
            ShoppingListEntity res,
            List<Recipe> recipes)
        {
            var grouped = query.GroupBy(x => x.Recipe.Id);
            foreach (var r in grouped)
            {
                Console.WriteLine("\n CreateShoppingListItemUpdater Servings grouped Count ======== " + r.Count());
                Console.WriteLine("\n CreateShoppingListItemUpdater Servings grouped r?.Key ======== " + r?.Key);
                var recipe = recipes.SingleOrDefault(x => x.Id == r?.Key);
                Console.WriteLine("\n CreateShoppingListItemUpdater recipe recipe  ======== " +
                                  recipe?.Ingredients.Length);
                if (recipe != null)
                {
                    foreach (var list in r.ToList())
                    {
                        Console.WriteLine("\n CreateShoppingListItemUpdater list.Amount ======== " +
                                          list?.Amount.SizeHigh);
                        if (list?.Amount != null)
                        {
                            Console.WriteLine("\n CreateShoppingListItemUpdater res.Data.Old.Servings ======== " +
                                              res.Data.Old.Servings);
                            Console.WriteLine("\n CreateShoppingListItemUpdater recipe.ServingSize ======== " +
                                              recipe.ServingSize);
                            var oldServingSize = res.Data.Old.Servings / recipe.ServingSize;
                            if (res.Data.Old.Servings % recipe.ServingSize != 0)
                                oldServingSize += 1;
                            Console.WriteLine("\n CreateShoppingListItemUpdater oldServingSize ======== " +
                                              oldServingSize);
                            var newServingSize = res.Data.New.Servings / recipe.ServingSize;
                            if (res.Data.New.Servings % recipe.ServingSize != 0)
                                newServingSize += 1;
                            Console.WriteLine("\n CreateShoppingListItemUpdater newServingSize ======== " +
                                              newServingSize);

                            var amount = list.Amount;
                            var count = amount.SizeHigh / oldServingSize;
                            amount.SizeHigh = count * newServingSize;
                            Console.WriteLine("\n amount ======== " + amount.SizeHigh);
                            shoppingListUpdater.UpdateItem(list, x => x.NewAmount(amount));
                        }

                        Console.WriteLine("\n foreach finished ======== ");
                    }
                }
            }

            return shoppingListUpdater;
        }

        public ShoppingListUpdater SetItemToRemove(IEnumerator<ShoppingListItem> query, Guid recipeId,
            ShoppingListUpdater shoppingListUpdater)
        {
            while (query.MoveNext())
            {
                ShoppingListItem item = query.Current;
                if (item != null && item.Recipe != null && item.Recipe.Id == recipeId)
                    shoppingListUpdater.RemoveItem(item);
            }

            return shoppingListUpdater;
        }
    }
}