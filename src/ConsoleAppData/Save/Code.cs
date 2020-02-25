/*using System;
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
using Parser.Model;
using Parser.Parser;

namespace ConsoleAppData.Save
{
    public class Code
    {
        
        
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
                        ConversionType = (UnitType) Enum.Parse(typeof(Units), ingredient.Measure, true),
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


        public void Connect()
        {
            
            
            var dbConfig = DataBaseHelper.DBConnector();
            var objRecipes = new ParseJson();
            var recipesFromJson = objRecipes.RecipesFromJson[1];

            var recipes = dbConfig.Context.Recipes
                .Load(Recipe.FromId(new Guid("96f00d3f-c6c9-49e4-8f5b-357b6b635e78")))
                .Load(Recipe.FromId(new Guid("0ce9cc8f-ed2b-4ad2-936d-8fbe697ceaff")))
                .WithMethod
                .WithUserRating
                .List();

         
            var foo = new ShoppingListAdder
            {
                Recipes = new List<Recipe>(recipes),
                Ingredients = recipes.SelectMany(x => x.Ingredients.Select(j => j.Ingredient)).ToList(),
                Usages = recipes.SelectMany(x => x.Ingredients).ToList()
            };

            var created2 = dbConfig.Context.ShoppingLists.Create.AddItems(foo).Commit();
            Console.WriteLine("111111111111111111111111 = " +
                              JsonSerializer.Serialize(objRecipes.RecipesFromJson, objRecipes.Options));


        }
        
        

         var created = dbConfig.Context.Recipes.Create
             .WithImage(new Uri(recipesFromJson.Image))
             .WithTitle(recipesFromJson.Label)
             .WithCookTime(Convert.ToInt16(recipesFromJson.TotalTime))
             .WithIngredients(x => setAdder(x, recipesFromJson, dbConfig.Context))
             .WithTags(RecipeTag.Breakfast | RecipeTag.Dinner)
             .WithMethod(recipesFromJson.Instructions);
            
         
         var res = created.Commit().NewRecipe;
    #1#
        //var created2 = dbConfig.Context.ShoppingLists.Create.AddItems(imem => imem.AddRecipe(recipes[0])).Commit();
        
        
        /*
        private string GetToken(IHeaderDictionary headers)
        {
            var token = headers.SingleOrDefault(x => x.Key == "Authorization").Value.ToString();
            return token.Substring("Bearer ".Length);
        }

        private string GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token).Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
            
            /*
            var defaultToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImU1YTkxZDlmMzlmYTRkZTI1NGExZTg5ZGYwMGYwNWI3ZTI0OGI5ODUiLCJ0eXAiOiJKV1QifQ.eyJuYW1lIjoi0JLQsNC70LXRgNC40Y8g0JzQsNC60LDRgNC10L3QutC-IiwicGljdHVyZSI6Imh0dHBzOi8vZ3JhcGguZmFjZWJvb2suY29tLzI5MzUyMTI4MDk4NDY4ODgvcGljdHVyZSIsImh0dHBzOi8vaGFzdXJhLmlvL2p3dC9jbGFpbXMiOnsieC1oYXN1cmEtZGVmYXVsdC1yb2xlIjoiYW5vbnltb3VzIiwieC1oYXN1cmEtYWxsb3dlZC1yb2xlcyI6WyJ1c2VyIiwiYW5vbnltb3VzIl0sIngtaGFzdXJhLW9yZy1pZCI6IjEyMyIsIngtaGFzdXJhLWN1c3RvbSI6ImN1c3RvbS12YWx1ZSIsIngtaGFzdXJhLXVzZXItaWQiOiJTME1iRDhoa2o5ZFFhcTVQYlFneW5IUjBHdzAyIn0sImlzcyI6Imh0dHBzOi8vc2VjdXJldG9rZW4uZ29vZ2xlLmNvbS9mZ2wyLTYzZGQzIiwiYXVkIjoiZmdsMi02M2RkMyIsImF1dGhfdGltZSI6MTU3OTY4MTM5NSwidXNlcl9pZCI6IlMwTWJEOGhrajlkUWFxNVBiUWd5bkhSMEd3MDIiLCJzdWIiOiJTME1iRDhoa2o5ZFFhcTVQYlFneW5IUjBHdzAyIiwiaWF0IjoxNTc5NjgxMzk1LCJleHAiOjE1Nzk2ODQ5OTUsImVtYWlsIjoicGFuYWNlYUB0ZS5uZXQudWEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZmFjZWJvb2suY29tIjpbIjI5MzUyMTI4MDk4NDY4ODgiXSwiZW1haWwiOlsicGFuYWNlYUB0ZS5uZXQudWEiXX0sInNpZ25faW5fcHJvdmlkZXIiOiJmYWNlYm9vay5jb20ifX0.AVXxUJT6muVrmQNX7maG69pWOb2je-UMFqfYqauKCPkex3qFhdJ0YHDtGsQ_qA5VnOMwBmK2j9yeF5DFpQqZVsHtu12cwygJEy--Df7RuUYRz2TL50EBS2pij8qUUL0TTUJnLrDUQfP466MG0hdvj7wLvy6y8FbTFzBIinXdoVI5DUuF3imueGm_SVCCs3-a58PtUH-zKEjlbLVjOyLU3r_Q9DNitDKrUucXJGbhC-NMlPW75ANH9brS0oP147el_Lb-9hBHzU3flanr6iWMIC_Fi5WzlOiP7-SuUc-u3Mp5iSPukS-EBvJLXN_HUi2UJx7ri1bB3HKlp4waImx5tg";
            var token = GetToken(Request.Headers);
            var userId = GetUserIdFromToken(token);
            #2#
        }#1#
        
        
        
        /**
* uri
* url
* source
* yield
* calories
* totalWeight
* dietLabels
* healthLabels
* cautions
* ingredientLines
* ingredients {
*  text
*  quantity
*  }
* totalNutrients {
*  label
*  quantity
*  unit
*  }
* digest {
*  tag
*  schemaOrgTag
*  label
*  unit
*  total
*  hasRDI
*  daily
*  }
* cuisineType
* mealType
* dishType
#1#
            
        /*dbConfig.Context.InitializeStore();
        staticConfig.Context.Initialize();
        dbConfig.Context.Import(staticConfig.Context);
        #1#

        /*
        KPCContext.Initialize(dbConfig);
        #1#

        /*
        var recipes = dbConfig.Context.Recipes
            .Load(Recipe.FromId(new Guid("ee371336-c183-448d-889c-d6b1f767bf59"))) // Weeknight Cheese Quiche
            .Load(Recipe.FromId(new Guid("e6ab278f-5f4d-4249-ab7d-8f66af044265"))) // Berry-Good Mousse
            .WithMethod
            .WithUserRating
            .List();#1#
    }
}*/