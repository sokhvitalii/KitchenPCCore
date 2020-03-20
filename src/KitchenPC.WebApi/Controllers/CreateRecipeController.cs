using System;
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
                var create = context.Recipes.Create
                    .WithCredit(request.Credit)
                    .WithDescription(request.Description)
                    .WithMethod(request.Steps)
                    .WithServingSize((short) request.ServingSize)
                    .WithTitle(request.Title)
                    .WithDateEntered(request.DateEntered)
                    .WithRating(request.Rating)
                    .WithTags(tegs)
                    .WithCookTime((short) request.CookTime)
                    .WithPrepTime((short) request.PrepTime)
                    .WithIngredients(x => createRecipeHelper.setAdder(x, request.Ingredients));

                if (request.CreditUrl != null)
                    create.WithImage(new Uri(request.ImageUrl));
     
                if (request.ImageUrl != null)
                    create.WithCreditUrl(new Uri(request.CreditUrl));

                var created = create.Commit();

                if (created.RecipeCreated)
                {
                    var ids = createRecipeHelper.GetTagIds(request.Tags, jsonHelper);
                    createRecipeHelper.SendToInsertRecipeTag(ids, created.NewRecipeId.Value, jsonHelper);
                    
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