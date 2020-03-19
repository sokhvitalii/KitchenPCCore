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
            Console.WriteLine("Request query: {0}", query);
            return request;
        }

        public T SendHttpRequest<T>(HttpClient client, HttpRequestMessage msg)
        {
           
            return client.SendAsync(msg)
                .ContinueWith(responseTask =>
                {
                    var res = responseTask.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Response from tag: {0}", res);
                    return JsonSerializer.Deserialize<T>(res, JsonHelper.Options);
                }).Result;
        }

        public TagResponseFromGq GetTagIds(string[] tags)
        {
            TagResponseFromGq list; 
            using (var client = new HttpClient())
            { 
                var query = GraphQlRequestBuilder
                    .CreateQuery()
                    .Table("tag")
                    .AppendReturn("id")
                    .AppendReturn("name");
                
                foreach (var t in tags)
                {
                    query.AppendCondition(new ConditionType("name", t, "_eq"));
                }
                
                var request = Request(query.BulkResult("_or"));
                list = SendHttpRequest<TagResponseFromGq>(client, request);
            }

            return list;
        }
        
        
        public RecipeTagResponseFromGq SendToInsertRecipeTag(TagResponseFromGq tags, Guid recipeId)
        {
            RecipeTagResponseFromGq list = new RecipeTagResponseFromGq(); 
            using (var client = new HttpClient())
            {
                var query =  GraphQlRequestBuilder.CreateMutation()
                    .Table("insert_recipe_tag")
                    .AppendReturn("id");

                foreach (var t in tags.Data.Tag)
                {
                    var record = new MutationSingleObject();
                    record.AppendObject("tag_id", t.Id);
                    record.AppendObject("recipe_id", recipeId.ToString());
                    query.AppendObject(record);
                }
                 var request = Request(query.BulkResult());
                 list = SendHttpRequest<RecipeTagResponseFromGq>(client, request);
                
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