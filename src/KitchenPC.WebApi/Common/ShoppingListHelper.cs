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
            
            /*
            if (servings > 1)
            {
                foreach (var r in recipes)
                {
                    if (r.ServingSize != servings)
                    {
                        
                        foreach (var ingredient in r.Ingredients)
                        {
                         if (ingredient.Amount != null)   
                            ingredient.Amount.SizeHigh = (ingredient.Amount.SizeHigh * servings) / r.ServingSize;
                        }
                    }
                }
            }
            */

            return new ShoppingListAdder
            {
                Recipes = recipes
            };
        }
        
         public ShoppingListUpdater CreateShoppingListItemUpdater(
            IEnumerator<ShoppingListItem> query,
            ShoppingListUpdater shoppingListUpdater, 
            ShoppingListEntity res,
            List<Recipe> recipes)
        {
            if (res.Data.Old.Servings == 0)
            {
                res.Data.Old.Servings = 1;
            }
            
            while (query.MoveNext())
            {
                ShoppingListItem item = query.Current;

                foreach (var r in recipes)
                {
                    if (
                        item?.Amount != null && 
                        item.Recipe != null && 
                        item.Recipe.Id == r.Id 
                        && res.Data.Old.Servings != res.Data.New.Servings)
                    {
                        var amount = item.Amount;
                        amount.SizeHigh = (amount.SizeHigh * res.Data.New.Servings) / res.Data.Old.Servings;
                    
                        shoppingListUpdater.UpdateItem(item, x => x.NewAmount(amount));
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