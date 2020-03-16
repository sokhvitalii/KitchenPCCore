using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentNHibernate.Conventions;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.DB;
using KitchenPC.DB.Provisioning;
using KitchenPC.Ingredients;
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
        private static Units getUnit(string units)
        {
            if (Enum.TryParse(typeof(Units), units, true, out var possible))
                return (Units) possible;
            
            return Units.Unit;
        }


        public static IngredientAdder setAdder(IngredientAdder adder, IngredientsRequest[] ingredients,
            DBContext context)
        {
            
            var ingredientDto = new List<Data.DTO.Ingredients>();
            foreach (var ingredient in ingredients)
            {
                var unitType = getUnit(ingredient.Quantity.Unit);
                var pResult = context.Parser.Parse(ingredient.Name.Trim());
                Amount amount = new Amount();
                amount.SizeHigh = ingredient.Quantity.Size;
                amount.Unit = unitType;
                
                Guid id;
               
                try
                {
                    id = context.ReadIngredient(pResult.Usage.Ingredient.Name).Id;
                }
                catch (Exception e)
                {
                    id = Guid.NewGuid();
                    var ing = new Data.DTO.Ingredients();
                    ing.DisplayName = pResult.Usage.Ingredient.Name;
                    ing.ConversionType = (UnitType)unitType;
                    ing.IngredientId = id;
                    ing.ManufacturerName = ingredient.Name;
                    ing.UnitName = ingredient.Aisle;
                    ingredientDto.Add(ing);
                }

                adder.AddIngredient(new Ingredient(id, ingredient.Name), amount);
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


        [HttpPost]
        public IActionResult Post(CreateRecipeRequest request)
        {
            try
            {
                var context = new DataBaseConnection(new AuthIdentity("systemUser", "")).Context.Context;
                var create = context.Recipes.Create
                    .WithCredit(request.Credit)
                    .WithDescription(request.Description)
                    .WithMethod(request.Steps)
                    .WithServingSize((short) request.ServingSize)
                    .WithTitle(request.Title)
                    .WithDateEntered(request.DateEntered)
                    .WithRating(request.Rating)
                    .WithTags(RecipeTags.Parse(string.Join(",", request.Tags)))
                    .WithCookTime((short) request.CookTime)
                    .WithPrepTime((short) request.PrepTime)
                    .WithIngredients(x => setAdder(x, request.Ingredients, context));

                if (request.CreditUrl != null)
                    create.WithImage(new Uri(request.ImageUrl));
     
                if (request.ImageUrl != null)
                    create.WithCreditUrl(new Uri(request.CreditUrl));

                var created = create.Commit();

                if (created.RecipeCreated)
                {
                    return Ok(JsonSerializer.Serialize(new CreateRecipeResponse(created.NewRecipe),
                        JsonHelper.Options));
                }

                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine("error = " + e.Message);
                return BadRequest(JsonSerializer.Serialize(new ResponseError(e.Message), JsonHelper.Options)); 
            }
        }
    }
}