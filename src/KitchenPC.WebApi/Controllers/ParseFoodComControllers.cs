using System;
using System.Collections.Generic;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.DB;
using KitchenPC.DB.Provisioning;
using KitchenPC.Ingredients;
using KitchenPC.Parser;
using KitchenPC.Recipes;
using KitchenPC.WebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace KitchenPC.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParseFoodComController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(FoodComRequest request)
        {
            try
            {
                var context = new DataBaseConnection(new AuthIdentity("systemUser", "")).Context.Context;
                var result = ParserFactory.StartParse(new Uri(request.Url), context);
                if (result.Result == ParserResult.Status.Success)
                {
                    var r = result.Recipe;
                    var recipe = context.Recipes.Create
                        .WithCredit(r.Credit)
                        .WithDescription(r.Description)
                        .WithMethod(r.Method)
                        .WithRating(r.UserRating)
                        .WithTags(r.Tags == null ? RecipeTags.From(RecipeTag.Quick) : r.Tags)
                        .WithTitle(r.Title)
                        .WithDateEntered(r.DateEntered)
                        .WithServingSize(r.ServingSize)
                        .WithIngredients(x => setAdder(x, r.Ingredients, context))
                        .WithCredit(r.Credit);

                    if (r.ImageUrl != null)
                    {
                        recipe.WithImage(new Uri(r.ImageUrl));
                    }

                    if (r.CreditUrl != null)
                    {
                        recipe.WithCreditUrl(new Uri(r.CreditUrl));
                    }

                    if (r.CookTime != null)
                    {
                        recipe.WithCookTime((short) r.CookTime);
                    }

                    if (r.PrepTime != null)
                    {
                        recipe.WithPrepTime((short) r.PrepTime);
                    }

                    recipe.Commit();
                    return Ok();
                }
                else
                {
                    return NotFound("invalid Recipe");
                }
            } 
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        public static IngredientAdder setAdder(IngredientAdder adder, IngredientUsage[] ingredientUsages, DBContext context)
        {
            var ingredientDto = new List<Data.DTO.Ingredients>();
            
           

            foreach (var ingredient in ingredientUsages)
            {
                Guid id;

                try
                {
                    id = context.ReadIngredient(ingredient.Ingredient.Name).Id;
                }
                catch (Exception e)
                {
                    id = Guid.NewGuid();
                    var ing = new Data.DTO.Ingredients();
                    ing.DisplayName = ingredient.Ingredient.Name;
                    ing.ConversionType = ingredient.Ingredient.ConversionType;
                    ing.IngredientId = id;
                    ing.ManufacturerName = ingredient.Ingredient.Name;
                    ing.UnitName = ingredient.Ingredient.UnitName;
                    ing.UnitWeight = ingredient.Ingredient.UnitWeight;
                    ingredientDto.Add(ing);
                }
                
                adder.AddIngredient(new Ingredient(id, ingredient.Ingredient.Name));
            }

            if (context.Adapter is DatabaseAdapter databaseAdapter)
            {
                using (var importer = new DatabaseImporter(databaseAdapter.GetSession()))
                {
                    importer.Import(ingredientDto);
                }
            }

            return adder;
        }
    }
}