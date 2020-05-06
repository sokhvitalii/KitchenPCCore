using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Context;
using Microsoft.AspNetCore.Mvc;
using KitchenPC.Context.Fluent;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using KitchenPC.WebApi.Common;
using KitchenPC.WebApi.Model;

namespace KitchenPC.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return Response.StatusCode.ToString();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ShoppingListController : ControllerBase
    {
        private ShoppingListAdder CreateShoppingListAdder(DBContext cont, int servings, List<Recipe> recipes)
        {
            
            if (servings != 0)
            {
                foreach (var r in recipes)
                {
                    if (r.ServingSize != servings)
                    {
                        
                        foreach (var ingredient in r.Ingredients)
                        {
                            ingredient.Amount.SizeHigh = (ingredient.Amount.SizeHigh * servings) / r.ServingSize;
                        }
                    }
                }
            }

            return new ShoppingListAdder
            {
                Recipes = recipes
            };
        }
        
        private ShoppingListUpdater CreateShoppingListItemUpdater(
            IEnumerator<ShoppingListItem> query, 
            DBContext cont, 
            ShoppingListUpdater shoppingListUpdater, 
            ShoppingListEntity res,
            List<Recipe> recipes)
        {
            if (res.Data.Old.Servings == 0)
            {
                res.Data.Old.Servings = recipes.First().ServingSize;
            }
            
            while (query.MoveNext())
            {
                ShoppingListItem item = query.Current;
                
                if (item.Amount != null && item.Recipe != null && item.Recipe.Id == recipes.First().Id && res.Data.Old.Servings != res.Data.New.Servings)
                {
                    var amount = item.Amount;
                    amount.SizeHigh = (amount.SizeHigh * res.Data.New.Servings) / res.Data.Old.Servings;
                    
                    shoppingListUpdater.UpdateItem(item, x => x.NewAmount(amount));
                }
            }

            return shoppingListUpdater;
        }
        
        private ShoppingListUpdater SetItemToRemove(IEnumerator<ShoppingListItem> query, Guid recipeId,
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

        [HttpPost]
        public IActionResult Post(ShoppingListEvent request)
        {
            var jsonHelper = new JsonHelper();
            try
            {
                
                var context = new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""), jsonHelper)
                    .Context.Context;
                var sList = context.ShoppingLists.Load(new ShoppingList(null, request.Event.Data.New.MealId)).WithItems
                    .List();
                
                var createRecipeHelper = new CreateRecipeHelper(context);
                var recipesFromGq = createRecipeHelper.GetMeal(request.Event.Data.New.MealId, jsonHelper);
                var recipeIds = recipesFromGq.Data.Meal.SelectMany(x => x.Dishes.Select(d => d.RecipeId));

                if (!sList.Any())
                {
                    var recipes = recipeIds.SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                    context.ShoppingLists.Create
                        .WithName(request.Event.Data.New.MealId.ToString())
                        .WithPlan(request.Event.Data.New.MealId)
                        .AddItems(CreateShoppingListAdder(context, request.Event.Data.New.Servings, recipes)).Commit();
                }
                else
                {
                    var shopping = sList.First();

                    if (request.Event.Op == "INSERT")
                    {
                        var recipes = recipeIds.SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                        context.ShoppingLists.Update(shopping)
                            .AddItems(CreateShoppingListAdder(context, request.Event.Data.New.Servings, recipes)).Commit();
                    }
                    else if (request.Event.Op == "UPDATE" && request.Event.Data.New.Servings != 0 && request.Event.Data.New.Servings != request.Event.Data.Old.Servings)
                    {
                        var recipes = recipeIds.SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        CreateShoppingListItemUpdater(shopping.GetEnumerator(), context, shoppingListUpdater, request.Event, recipes).Commit();
                    }
                    else if (request.Event.Op == "DELETE")
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        foreach (var r in recipeIds)
                        {
                            SetItemToRemove(shopping.GetEnumerator(), r, shoppingListUpdater);
                        }
                        shoppingListUpdater.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException?.Message ?? "");
                return BadRequest("invalid request");
            }

            return Ok();
        }
    }
}