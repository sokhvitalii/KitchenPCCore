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
                var context = 
                    new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""), jsonHelper)
                    .Context.Context;
                var createRecipeHelper = new CreateRecipeHelper(context);
                var mealId = request.Event.Data.New?.MealId ?? request.Event.Data.Old.MealId;
                var items = createRecipeHelper.GetPlanItems(mealId, jsonHelper);
                var planItems = items.Data.Meal.SelectMany(x => x.PlanItems).ToList();

                if (planItems.Count > 0)
                {
                    var recipesFromGq = createRecipeHelper.GetPlan(mealId, jsonHelper);
                    var recipeIds = recipesFromGq.Data.Plan
                        .SelectMany(d => d.PlanItems.SelectMany(x => x.Meal.Dishes.Select(x => x.RecipeId)))
                        .ToList();
                    
                    var helper = new ShoppingListHelper();
                    var planId = planItems.First().PlanId;
                    var sList = context.ShoppingLists.Load(new ShoppingList(null, planId)).WithItems.List();
                   
                    var recipes = recipeIds
                        .SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                    var listPlanItems = recipesFromGq.Data.Plan.SelectMany(x => x.PlanItems).ToList();
                    if (!sList.Any())
                    {
                        context.ShoppingLists.Create
                            .WithName(mealId.ToString() + planId)
                            .WithPlan(planId)
                            .AddItems(helper.CalculateServing(listPlanItems, recipes)).Commit();
                    }
                    else
                    {
                        var shopping = sList.First();
                        context.ShoppingLists.Delete(shopping).Commit();
                        Console.WriteLine("\n ShoppingLists deleted ======== ");
                        context.ShoppingLists.Create
                            .WithName(mealId.ToString() + planId)
                            .WithPlan(planId)
                            .AddItems(helper.CalculateServing(listPlanItems, recipes)).Commit();
                        Console.WriteLine("\n ShoppingLists Created ======== ");
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
            Console.WriteLine("\n ShoppingListtPlanItemEvent New ======== " + request.Event.Data.New);
            Console.WriteLine("\n  ShoppingListtPlanItemEvent Old ======== " + request.Event.Data.Old);
            var jsonHelper = new JsonHelper();
            try
            {
                var context = 
                    new DataBaseConnection(new AuthIdentity(request.Event.SessionVariables.HasuraUserId, ""), jsonHelper)
                    .Context.Context;

                var mealId = request.Event.Data.New?.MealId ?? request.Event.Data.Old.MealId;
                var planId = request.Event.Data.New?.Planid ?? request.Event.Data.Old.Planid;

                var sList = context.ShoppingLists.Load(new ShoppingList(null, planId)).WithItems.List();
                var createRecipeHelper = new CreateRecipeHelper(context);
                var recipesFromGq = createRecipeHelper.GetPlanById(mealId, jsonHelper);

                var recipeIds = recipesFromGq.Data.Plan
                    .SelectMany(d => d.PlanItems.SelectMany(x => x.Meal.Dishes.Select(x => x.RecipeId)))
                    .ToList();

                var listPlanItems = recipesFromGq.Data.Plan.SelectMany(x => x.PlanItems).ToList();
                var recipes = recipeIds
                    .SelectMany(r => context.Recipes.Load(Recipe.FromId(r)).WithMethod.WithUserRating.List()).ToList();
                
                var helper = new ShoppingListHelper();
                if (!sList.Any())
                {
                    Console.WriteLine("\n ShoppingLists Create ======== ");
                    context.ShoppingLists.Create
                        .WithName(mealId.ToString() + planId)
                        .WithPlan(planId)
                        .AddItems(helper.CalculateServing(listPlanItems, recipes)).Commit();
                }
                else
                {
                    var shopping = sList.First();
                    context.ShoppingLists.Delete(shopping).Commit();
                    Console.WriteLine("\n ShoppingLists deleted ======== ");
                    context.ShoppingLists.Create
                        .WithName(mealId.ToString() + planId)
                        .WithPlan(planId)
                        .AddItems(helper.CalculateServing(listPlanItems, recipes)).Commit();
                    Console.WriteLine("\n ShoppingLists Created ======== ");


                    // if (request.Event.Op == "INSERT")
                    // {
                    //     Console.WriteLine("\n ShoppingLists INSERT ======== ");
                    //     context.ShoppingLists.Update(shopping)
                    //         .AddItems(helper.CreateShoppingListAdder(servings, recipes)).Commit();
                    // }
                    // else if (request.Event.Op == "UPDATE" && servings != 0 &&
                    //          request.Event.Data.New?.Servings != request.Event.Data.Old?.Servings)
                    // {
                    //     Console.WriteLine("\n ShoppingLists UPDATE Servings ======== ");
                    //     var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                    //     helper.CreateShoppingListItemUpdater(shopping.ToList(), shoppingListUpdater, request.Event,
                    //         recipes).Commit();
                    // }
                    // else if (request.Event.Op == "DELETE")
                    // {
                    //     Console.WriteLine("\n ShoppingLists DELETE ======== ");
                    //     var shoppingListUpdater = context.ShoppingLists.Update(shopping);
                    //     foreach (var r in recipeIds)
                    //     {
                    //         helper.SetItemToRemove(shopping.GetEnumerator(), r, shoppingListUpdater);
                    //     }
                    //
                    //     shoppingListUpdater.Commit();
                    // }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("eeeeeeeeeeee");
                Console.WriteLine(e);
                return BadRequest("invalid request");
            }

            Console.WriteLine("\n request finished  ========------------------------------------- \n ");
            return Ok();
        }
    }
}