using System;
using System.Linq;
using System.Text.Json;
using KitchenPC.Recipes;
using KitchenPC.WebApi.Common;
using KitchenPC.WebApi.Model;
using KitchenPC.WebApi.Model.error;
using Microsoft.AspNetCore.Mvc;

namespace KitchenPC.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateRecipeController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(CreateRecipeRequest request)
        {
            var jsonHelper = new JsonHelper();
            try
            {
                Console.WriteLine("request = " + request);
                var context = new DataBaseConnection(new AuthIdentity("systemUser", ""), jsonHelper).Context.Context;
                var createRecipeHelper = new CreateRecipeHelper(context);

                RecipeTags tegs = RecipeTag.Easy | RecipeTag.Quick;
                try
                {
                    tegs = RecipeTags.Parse(string.Join(",", request.Tags));
                }  
                catch (Exception e)
                {
                    Console.WriteLine("error = " + e);
                }

                var ingredients = createRecipeHelper.GetIngredients(request);
                
                if (request.RecipeStep == null || request.RecipeStep.Length == 0)
                    throw new ResponseError("required field recipe step");
                
                var create = context.Recipes.Create
                    .WithCredit(request.Credit)
                    .WithDescription(request.Description)
                    .WithMethod(request.Steps)
                    .WithServingSize((short) request.ServingSize)
                    .WithTitle(request.Title)
                    .WithDateEntered(request.DateEntered ?? DateTime.Now)
                    .WithRating(request.Rating)
                    .WithTags(tegs)
                    .WithCookTime((short) request.CookTime)
                    .WithPrepTime((short) request.PrepTime)
                    .WithIngredients(x => createRecipeHelper.setAdder(x, ingredients));

                if (request.ImageUrl != null)
                    create.WithImage(new Uri(request.ImageUrl));
                
                if (request.UserChefId != null)
                    create.WithUserChef(request.UserChefId); 
                
                if (request.UserUpdatedId != null)
                    create.WithUserUpdated(request.UserUpdatedId);
     
                if (request.CreditUrl != null)
                    create.WithCreditUrl(new Uri(request.CreditUrl));

                var created = create.Commit();

                if (created.RecipeCreated)
                {
                    var ingredient = ingredients.SingleOrDefault(x => x.Ingredient.Name == request.MainIngredient.Name);
                    if (ingredient?.Ingredient?.Name == null)
                        throw new ResponseError("ingredients does not contains main ingredient");  
                    
                    var mainId = createRecipeHelper.SendToInsertMainIngredient(ingredient.Ingredient, created.NewRecipeId.Value, jsonHelper);

                    if (mainId == 0)
                        throw new ResponseError("main ingredient was not save, name: " + ingredient.Ingredient.Name);
                    
                    request.Tags.Add(request.Difficulty);
                    var ids = createRecipeHelper.GetTagIds(request.Tags, jsonHelper);
                    ids.Data.Tag.Add(new TagsGQ(mainId));
                    createRecipeHelper.SendToInsertRecipeTag(ids, created.NewRecipeId.Value, jsonHelper);
                    createRecipeHelper.SendRecipeStep(request.RecipeStep, created.NewRecipeId.Value, jsonHelper);
                    
                    return Ok(JsonSerializer.Serialize(new CreateRecipeResponse(created.NewRecipe),
                        jsonHelper.Options));
                }

                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine("error = " + e);
                return BadRequest(JsonSerializer.Serialize(new ResponseError(e.Message), jsonHelper.Options)); 
            }
        }
    }
}