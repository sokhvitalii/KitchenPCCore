using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConsoleAppData.Helper;
using FluentNHibernate.Cfg.Db;
using KitchenPC;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.Data.DTO;
using KitchenPC.DB;
using KitchenPC.DB.Provisioning;
using KitchenPC.DB.Search;
using KitchenPC.Ingredients;
using KitchenPC.Recipes;
using NHibernate.Criterion;
using NHibernate.Mapping;
using Parser.Model;
using Parser.Parser;

namespace ConsoleAppData
{
    class Program
    {
        static void Main(string[] args)
        {
           DataBaseHelper.SaveInitData();
            var dbConfig = DataBaseHelper.DBConnector();
            var objRecipes = new ParseJson();
            var recipesFromJson = objRecipes.RecipesFromJson[1];
            
     var recipes = dbConfig.Context.Recipes
         .Load(Recipe.FromId(new Guid("96f00d3f-c6c9-49e4-8f5b-357b6b635e78")))
         .Load(Recipe.FromId(new Guid("0ce9cc8f-ed2b-4ad2-936d-8fbe697ceaff")))
         .WithMethod
         .WithUserRating
         .List();

     /*
     var created = dbConfig.Context.Recipes.Create
         .WithImage(new Uri(recipesFromJson.Image))
         .WithTitle(recipesFromJson.Label)
         .WithCookTime(Convert.ToInt16(recipesFromJson.TotalTime))
         .WithIngredients(x => setAdder(x, recipesFromJson, dbConfig.Context))
         .WithTags(RecipeTag.Breakfast | RecipeTag.Dinner)
         .WithMethod(recipesFromJson.Instructions);
        
     
     var res = created.Commit().NewRecipe;
*/
            //var created2 = dbConfig.Context.ShoppingLists.Create.AddItems(imem => imem.AddRecipe(recipes[0])).Commit();
            var foo = new ShoppingListAdder
            {
                Recipes = new List<Recipe>(recipes),
                Ingredients = recipes.SelectMany(x => x.Ingredients.Select(j => j.Ingredient)).ToList(),
                Usages = recipes.SelectMany(x => x.Ingredients).ToList()
            };

            var created2 = dbConfig.Context.ShoppingLists.Create.AddItems(foo).Commit();
            Console.WriteLine("111111111111111111111111 = " + JsonSerializer.Serialize(objRecipes.RecipesFromJson, objRecipes.Options));
        }
        
        public static IngredientAdder setAdder(IngredientAdder adder, RecipesFromJson js, DBContext context)
        {
            
            var ingredientDto = new List<Ingredients>();
            
            foreach (var ingredient in js.Ingredients)
            {
                if (!ingredientDto.Any(x => x.DisplayName == ingredient.Food))
               {
                   var id = Guid.NewGuid();
                   ingredientDto.Add(new Ingredients
                   {
                       IngredientId = id,
                       ConversionType = (UnitType)Enum.Parse(typeof(Units), ingredient.Measure, true),
                       UnitWeight = Convert.ToInt32(ingredient.Weight),
                       DisplayName = ingredient.Food
                   
                   });

                   adder.AddIngredient(new Ingredient(id, ingredient.Food));
               }
             
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