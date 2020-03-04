using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Context;
using Microsoft.AspNetCore.Mvc;
using KitchenPC.Context.Fluent;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
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
        private ShoppingListAdder CreateShoppingListAdder(DBContext cont, ShoppingListEntity res)
        {

            var recipes = cont.Recipes.Load(Recipe.FromId(res.Data.New.Recipeid)).WithMethod.WithUserRating.List();

            if (res.Data.New.Servings != 0)
            {
                foreach (var r in recipes)
                {
                    if (r.ServingSize != res.Data.New.Servings)
                    {
                        
                        foreach (var ingredient in r.Ingredients)
                        {
                            ingredient.Amount.SizeHigh = (ingredient.Amount.SizeHigh * res.Data.New.Servings) / r.ServingSize;
                        }
                    }
                }
            }

            return new ShoppingListAdder
            {
                Recipes = recipes
            };
        }
        
        private ShoppingListUpdater CreateShoppingListItemUpdater(IEnumerator<ShoppingListItem> query, DBContext cont, ShoppingListUpdater shoppingListUpdater, ShoppingListEntity res)
        {
            if (res.Data.Old.Servings == 0)
            {
                var recipes = cont.Recipes.Load(Recipe.FromId(res.Data.New.Recipeid)).WithMethod.WithUserRating.List().First();
                res.Data.Old.Servings = recipes.ServingSize;
            }
            
            while (query.MoveNext())
            {
                ShoppingListItem item = query.Current;
                
                if (item.Amount != null && item.Recipe != null && item.Recipe.Id == res.Data.New.Recipeid && res.Data.Old.Servings != res.Data.New.Servings)
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
            try
            {
                var context = new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""))
                    .Context.Context;
                var sList = context.ShoppingLists.Load(new ShoppingList(null, request.Event.Data.New.Planid)).WithItems
                    .List();

                if (!sList.Any())
                {
                    context.ShoppingLists.Create
                        .WithName(request.Event.Data.New.Planid.ToString())
                        .WithPlan(request.Event.Data.New.Planid)
                        .AddItems(CreateShoppingListAdder(context, request.Event)).Commit();
                }
                else
                {
                    var shopping = sList.First();

                    if (request.Event.Op == "INSERT")
                    {
                        context.ShoppingLists.Update(shopping)
                            .AddItems(CreateShoppingListAdder(context, request.Event)).Commit();
                    }
                    else if (request.Event.Op == "UPDATE" && request.Event.Data.New.Servings != 0 && request.Event.Data.New.Servings != request.Event.Data.Old.Servings)
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        CreateShoppingListItemUpdater(shopping.GetEnumerator(), context, shoppingListUpdater, request.Event).Commit();
                    }
                    else if (request.Event.Op == "DELETE")
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        SetItemToRemove(shopping.GetEnumerator(), request.Event.Data.New.Recipeid, shoppingListUpdater)?.Commit();
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