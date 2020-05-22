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
                var recipe = recipes.Find(x => x.Id == r.Key);
                foreach (var list in r)
                {
                    if (list?.Amount != null && recipe != null)
                    {
                        var oldServingSize = res.Data.Old.Servings / recipe.ServingSize;
                        if (res.Data.Old.Servings % recipe.ServingSize != 0)
                            oldServingSize += 1;
                        var newServingSize = res.Data.New.Servings / recipe.ServingSize;
                        if (res.Data.New.Servings % recipe.ServingSize != 0)
                            newServingSize += 1;
                        
                        var amount = list.Amount;
                        var count = amount.SizeHigh / oldServingSize;
                        amount.SizeHigh = count * newServingSize;
                        
                        shoppingListUpdater.UpdateItem(list, x => x.NewAmount(amount));
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