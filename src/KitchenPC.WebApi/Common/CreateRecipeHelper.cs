using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.DB;
using KitchenPC.DB.Helper;
using KitchenPC.DB.Provisioning;
using KitchenPC.Ingredients;
using KitchenPC.WebApi.Model;

namespace KitchenPC.WebApi.Common
{
    public class CreateRecipeHelper
    {
        public DBContext context;

        public static HttpRequestMessage Request(string query)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/v1/graphql");
            request.Headers.Add("x-hasura-admin-secret", "ADMIN_SECRET_KEY");
            request.Content = new StringContent(query, Encoding.UTF8, "application/json");

            return request;
        }

        public TagResponseFromGq SendHttpRequest(HttpClient client, HttpRequestMessage msg)
        {
            return client.SendAsync(msg)
                .ContinueWith(responseTask =>
                {
                    Console.WriteLine("Response from tag: {0}", responseTask.Result.Content.ReadAsStringAsync().Result);
                    return JsonSerializer.Deserialize<TagResponseFromGq>(
                        responseTask.Result.Content.ReadAsStringAsync().Result, JsonHelper.Options);
                }).Result;
        }

        public List<TagResponseFromGq> GetTagIds(string[] tags)
        {
            var list = new List<TagResponseFromGq>(); 
            using (var client = new HttpClient())
            {
                foreach (var t in tags)
                {
                    var query = GraphQlRequestBuilder
                        .CreateQuery()
                        .Table("tag")
                        .AppendCondition(new ConditionType("name", t, "eq"))
                        .AppendReturn("id")
                        .AppendReturn("name")
                        .Result();
                    
                    var request = Request(query);

                    list.Add(SendHttpRequest(client, request));
                }
            }

            return list;
        }
        
        
        public List<TagResponseFromGq> SendToInsertRecipeTag(List<TagResponseFromGq> tags, Guid recipeId)
        {
            var list = new List<TagResponseFromGq>(); 
            using (var client = new HttpClient())
            {
                foreach (var t in tags)
                {
                    var query =  GraphQlRequestBuilder.CreateMutation()
                        .Table("insert_recipe_tag")
                        .AppendObject("recipe_id", recipeId.ToString())
                        .AppendObject("tag_id", t.Data.Tag.First().Id)
                        .AppendReturn("id").Result();
                    
                    var request = Request(query);

                    list.Add(SendHttpRequest(client, request));
                }
            }

            return list;
        }

        private Units getUnit(string units)
        {
            if (Enum.TryParse(typeof(Units), units, true, out var possible))
                return (Units) possible;

            return Units.Unit;
        }

        public IngredientAdder setAdder(IngredientAdder adder, IngredientsRequest[] ingredients)
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
                    ing.ConversionType = (UnitType) unitType;
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

        public CreateRecipeHelper(DBContext ctx)
        {
            context = ctx;
        }
    }
}