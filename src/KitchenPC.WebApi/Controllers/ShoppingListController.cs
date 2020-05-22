using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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
                var mealId = request.Event.Data.New?.MealId ?? request.Event.Data.Old.MealId;
                var recipesFromGq = createRecipeHelper.GetPlanItems(mealId, jsonHelper);

                if (recipesFromGq.Data.PlanItems.Count > 0)
                {
                    var helper = new ShoppingListHelper();
                    var serving = recipesFromGq.Data.PlanItems.Aggregate(0, (acc, x) => acc + x.Servings);

                    var sList = context.ShoppingLists.Load(new ShoppingList(null, recipesFromGq.Data.PlanItems.First().PlanId))
                        .WithItems
                        .List();
                    var recipeId = request.Event.Data.New?.RecipeId ?? request.Event.Data.Old.RecipeId;
                  
                    if (!sList.Any())
                    {
                        var recipes = 
                            context.Recipes.Load(Recipe.FromId(recipeId)).WithMethod.WithUserRating.List().ToList();
                        context.ShoppingLists.Create
                            .WithName(mealId.ToString() + recipesFromGq.Data.PlanItems.First().PlanId)
                            .WithPlan(recipesFromGq.Data.PlanItems.First().PlanId)
                            .AddItems(helper.CreateShoppingListAdder(serving, recipes)).Commit();
                    }
                    else
                    {
                        var shopping = sList.First();
                        if (request.Event.Op == "INSERT")
                        {
                            var recipes = 
                                context.Recipes.Load(Recipe.FromId(recipeId)).WithMethod.WithUserRating.List().ToList();
                            context.ShoppingLists.Update(shopping)
                                .AddItems(helper.CreateShoppingListAdder(serving, recipes)).Commit();
                        }
                        else if (request.Event.Op == "DELETE")
                        {
                            var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                            helper.SetItemToRemove(shopping.GetEnumerator(), recipeId, shoppingListUpdater);
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
                
                var mealId = request.Event.Data.New?.MealId ?? request.Event.Data.Old.MealId;
                var planId = request.Event.Data.New?.Planid ?? request.Event.Data.Old.Planid;
                
                var sList = context.ShoppingLists.Load(new ShoppingList(null, planId)).WithItems
                    .List();
                
                var createRecipeHelper = new CreateRecipeHelper(context);
                var recipesFromGq = createRecipeHelper.GetDishe(mealId, jsonHelper);
                var recipeIds = recipesFromGq.Data.Dish.Select(d => d.RecipeId).Distinct().ToList();
                var recipes = recipeIds.SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                var servings = request.Event.Data.New?.Servings ?? request.Event.Data.Old.Servings;
                
                if (!sList.Any())
                {
                     context.ShoppingLists.Create
                        .WithName(mealId.ToString() + planId)
                        .WithPlan(planId)
                        .AddItems(helper.CreateShoppingListAdder(servings, recipes)).Commit();
                }
                else
                {
                    var shopping = sList.First();
                    if (request.Event.Op == "INSERT")
                    {
                        context.ShoppingLists.Update(shopping)
                            .AddItems(helper.CreateShoppingListAdder(servings, recipes)).Commit();
                    }
                    else if (request.Event.Op == "UPDATE" && servings != 0 && request.Event.Data.New?.Servings != request.Event.Data.Old?.Servings)
                    {
                        var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                        helper.CreateShoppingListItemUpdater(shopping.ToList(), shoppingListUpdater, request.Event, recipes).Commit();
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
                Console.WriteLine(e);
                return BadRequest("invalid request");
            }

            return Ok();
        }
    }
}