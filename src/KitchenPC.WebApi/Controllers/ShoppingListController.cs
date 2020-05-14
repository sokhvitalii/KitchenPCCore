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
    public class ShoppingListDishController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(ShoppingListEventDish request)
        { 
            Console.WriteLine("ShoppingListEventDish" + request);
            var jsonHelper = new JsonHelper();
            try
            {
                var context = new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""), jsonHelper)
                    .Context.Context;
                var createRecipeHelper = new CreateRecipeHelper(context);
                var recipesFromGq = createRecipeHelper.GetPlanItems(request.Event.Data.New.MealId, jsonHelper);

                if (recipesFromGq.Data.PlanItems.Count > 0)
                {
                    var helper = new ShoppingListHelper();
                    var serving = recipesFromGq.Data.PlanItems.Aggregate(0, (acc, x) => acc + x.Servings);

                    var sList = context.ShoppingLists.Load(new ShoppingList(null, recipesFromGq.Data.PlanItems.First().PlanId))
                        .WithItems
                        .List();
                    var recipes = context.Recipes
                        .Load(Recipe.FromId(request.Event.Data.New.RecipeId)).WithMethod.WithUserRating
                        .List()
                        .ToList();
                    if (!sList.Any())
                    {
                        context.ShoppingLists.Create
                            .WithName(request.Event.Data.New.MealId.ToString() + recipesFromGq.Data.PlanItems.First().PlanId)
                            .WithPlan(recipesFromGq.Data.PlanItems.First().PlanId)
                            .AddItems(helper.CreateShoppingListAdder(context, serving, recipes)).Commit();
                    }
                    else
                    {
                        var shopping = sList.First();
                        if (request.Event.Op == "INSERT")
                        {
                            context.ShoppingLists.Update(shopping)
                                .AddItems(helper.CreateShoppingListAdder(context, serving, recipes)).Commit();
                        }
                        else if (request.Event.Op == "DELETE")
                        {
                            var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                            helper.SetItemToRemove(shopping.GetEnumerator(), request.Event.Data.New.RecipeId, shoppingListUpdater);
                            shoppingListUpdater.Commit();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
                return BadRequest("invalid request");
            }
            

            return Ok();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ShoppingListPlanItemController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(ShoppingListtPlanItemEvent request)
        {
            Console.WriteLine("ShoppingListtPlanItemEvent" + request);
            var jsonHelper = new JsonHelper();
            try
            {
                var helper = new ShoppingListHelper();
                var context = new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""), jsonHelper)
                    .Context.Context;
               
                var sList = context.ShoppingLists.Load(new ShoppingList(null, request.Event.Data.New.Planid)).WithItems
                    .List();
                
                var createRecipeHelper = new CreateRecipeHelper(context);
                var recipesFromGq = createRecipeHelper.GetDishe(request.Event.Data.New.MealId, jsonHelper);
                var recipeIds = recipesFromGq.Data.Dish.Select(d => d.RecipeId).ToList();
                var recipes = recipeIds.SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                
                if (!sList.Any())
                {
                     context.ShoppingLists.Create
                        .WithName(request.Event.Data.New.MealId.ToString() + request.Event.Data.New.Planid)
                        .WithPlan(request.Event.Data.New.Planid)
                        .AddItems(helper.CreateShoppingListAdder(context, request.Event.Data.New.Servings, recipes)).Commit();
                }
                else
                {
                    var shopping = sList.First();
                    if (request.Event.Op == "INSERT")
                    {
                        context.ShoppingLists.Update(shopping)
                            .AddItems(helper.CreateShoppingListAdder(context, request.Event.Data.New.Servings, recipes)).Commit();
                    }
                    else if (request.Event.Op == "UPDATE" && request.Event.Data.New.Servings != 0 && request.Event.Data.New.Servings != request.Event.Data.Old.Servings)
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        helper.CreateShoppingListItemUpdater(shopping.GetEnumerator(), shoppingListUpdater, request.Event, recipes).Commit();
                    }
                    else if (request.Event.Op == "DELETE")
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        foreach (var r in recipeIds)
                        {
                            helper.SetItemToRemove(shopping.GetEnumerator(), r, shoppingListUpdater);
                        }
                        shoppingListUpdater.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("eeeeeeeeeeee");
                Console.WriteLine(e.InnerException?.Message ?? "");
                return BadRequest("invalid request");
            }

            return Ok();
        }
    }
}