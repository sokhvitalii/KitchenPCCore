using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Context;
using Microsoft.AspNetCore.Mvc;
using KitchenPC.Context.Fluent;
using KitchenPC.Modeler;
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
        private ShoppingListAdder CreateShoppingListAdder(DBContext cont, Guid id)
        {
            return new ShoppingListAdder
            {
                Recipes = cont.Recipes.Load(Recipe.FromId(id)).WithMethod.WithUserRating.List()
            };
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
                    context.ShoppingLists.Create.WithName(request.Event.Data.New.Planid.ToString()).WithPlan(request.Event.Data.New.Planid)
                        .AddItems(CreateShoppingListAdder(context, request.Event.Data.New.Recipeid)).Commit();
                }
                else
                {
                    var shopping = sList.First();

                    if (request.Event.Op == "INSERT")
                    {
                        context.ShoppingLists.Update(shopping)
                            .AddItems(CreateShoppingListAdder(context, request.Event.Data.New.Recipeid)).Commit();
                    }
                    else if (request.Event.Op == "DELETE")
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        SetItemToRemove(shopping.GetEnumerator(), request.Event.Data.New.Recipeid, shoppingListUpdater)
                            ?.Commit();
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