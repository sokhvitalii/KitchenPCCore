using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using DB.Helper;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.DB;
using KitchenPC.DB.Helper;
using KitchenPC.DB.Provisioning;
using KitchenPC.Ingredients;
using KitchenPC.WebApi.Model;
using KitchenPC.WebApi.Model.error;

namespace KitchenPC.WebApi.Common
{
    public class CreateRecipeHelper
    {
        public DBContext context;

        public static HttpRequestMessage Request(string query, JsonHelper conf)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, conf.HasuraHost);
            request.Headers.Add("x-hasura-admin-secret", "ADMIN_SECRET_KEY");
            request.Content = new StringContent(query, Encoding.UTF8, "application/json");
            Console.WriteLine("Request query: {0}", query);
            return request;
        }

        public T SendHttpRequest<T>(HttpClient client, HttpRequestMessage msg, JsonHelper conf)
        {
            return client.SendAsync(msg)
                .ContinueWith(responseTask =>
                {
                    var res = responseTask.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Response from tag: {0}", res);
                    return JsonSerializer.Deserialize<T>(res, conf.Options);
                }).Result;
        }

        public TagResponseFromGq GetTagIds(List<string> tags, JsonHelper conf)
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
                
                var request = Request(query.BulkResult("_or"), conf);
                list = SendHttpRequest<TagResponseFromGq>(client, request, conf);
            }

            return list;
        }
        
           
        public int SendToInsertMainIngredient(Ingredient ingredient, Guid recipeId, JsonHelper conf)
        {
           
            using (var client = new HttpClient())
            {
                
                var queryGet = GraphQlRequestBuilder
                    .CreateQuery()
                    .Table("tag")
                    .AppendReturn("id")
                    .AppendReturn("name")
                    .AppendCondition(new ConditionType("tag_type_id", 3, "_eq"))
                    .AppendCondition(new ConditionType("name", ingredient.Name, "_eq"));
                
                
                var requestGet = Request(queryGet.BulkResult("_and"), conf);
                var response = SendHttpRequest<TagResponseFromGq>(client, requestGet, conf);

                if (response.Data?.Tag == null || response.Data?.Tag.Count == 0)
                {
                    var query =  GraphQlRequestBuilder.CreateMutation()
                        .Table("insert_tag")
                        .AppendReturn("id")
                        .AppendReturn("name")
                        .AppendObject("name", ingredient.Name)
                        .AppendObject("tag_type_id", 3);
                
                    var request = Request(query.Result(), conf);
                    var obj = SendHttpRequest<TagGraphQlResponse>(client, request, conf);
                    
                    if (obj.Data!?.InsertTag!?.Returning != null || obj.Data.InsertTag.Returning.Length >= 0)
                    {
                        return obj.Data.InsertTag.Returning.First().id;  
                    }
                }
                else
                {
                    return response.Data.Tag.First().Id;
                }
            }

            return 0;
        }

        
        
        public RecipeTagResponseFromGq SendToInsertRecipeTag(TagResponseFromGq tags, Guid recipeId, JsonHelper conf)
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
                 var request = Request(query.BulkResult(), conf);
                 list = SendHttpRequest<RecipeTagResponseFromGq>(client, request, conf);
                
            }

            return list;
        }

        private Units getUnit(string units)
        {
            if (Enum.TryParse(typeof(Units), units, true, out var possible))
                return (Units) possible;

            return Units.Unit;
        }

        public IngredientAdder setAdder(IngredientAdder adder, List<IngredientUsage> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                adder.AddIngredient(ingredient.Ingredient, ingredient.Amount);
            }
            
            return adder;
        }
        
        
        public List<IngredientUsage> GetIngredients(CreateRecipeRequest req)
        {
            var ingredientDto = new List<Data.DTO.Ingredients>();
            var allIngredient = new List<IngredientUsage>();
            foreach (var ingredient in req.Ingredients)
            {
                var amount = ingredient.Quantity.Unit != null ? 
                    new Amount(ingredient.Quantity.Size,  getUnit(ingredient.Quantity.Unit)) : 
                    new Amount {SizeHigh = ingredient.Quantity.Size};
                
                // var pResult = context.Parser.Parse(ingredient.Name.Trim());
                /*
                if (pResult.Usage?.Ingredient == null)
                    throw new ResponseError("NLP can not parse ingredient name: " + ingredient.Name);
                    */

                Guid id;

                try
                {
                    id = context.ReadIngredient(ingredient.Name.Trim()).Id;
                }
                catch (Exception e)
                {
                    id = Guid.NewGuid();
                    var ing = new Data.DTO.Ingredients();
                    ing.DisplayName = ingredient.Name.Trim();
                    ing.IngredientId = id;
                    ing.ManufacturerName = ingredient.Name;
                    ing.UnitName = ingredient.Aisle;
                    ingredientDto.Add(ing);
                }

                var ingr = new Ingredient(id, ingredient.Name);
                allIngredient.Add(new IngredientUsage(ingr, null, amount, null));
            }

            if (context.Adapter is DatabaseAdapter databaseAdapter)
            {
                using (var importer = new DatabaseImporter(databaseAdapter.GetSession()))
                {
                    importer.Import(ingredientDto);
                }
            }
            
            return allIngredient;
        }


        public CreateRecipeHelper(DBContext ctx)
        {
            context = ctx;
        }
    }
}