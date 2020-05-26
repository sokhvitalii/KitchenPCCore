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
        
           
        public List<int> SendToInsertMainIngredient(List<string> ingredient, Guid recipeId, JsonHelper conf)
        {
            var result = new List<int>();
           
            using (var client = new HttpClient())
            {
                var queryGet = GraphQlRequestBuilder
                    .CreateQuery()
                    .Table("tag")
                    .AppendReturn("id")
                    .AppendReturn("name");
                
                foreach (var name in ingredient)
                {
                    queryGet.AppendCondition(new ConditionType("name", name, "_eq"));
                }
                
                
                var requestGet = Request(queryGet.BulkResult("_or"), conf);
                var response = SendHttpRequest<TagResponseFromGq>(client, requestGet, conf);

                var tags = response.Data;
                response?.Data?.Tag?.ForEach(x => result.Add(x.Id));
                if (tags?.Tag == null || tags?.Tag.Count == 0 || tags?.Tag.Count != ingredient.Count)
                {
                    var ing = ingredient.Where(x => tags != null && !tags.Tag.Exists(t => t.Name == x)).ToList();
                    if (ing.Count > 0)
                    {
                        var query = GraphQlRequestBuilder
                            .CreateMutation()
                            .Table("insert_tag")
                            .AppendReturn("id")
                            .AppendReturn("name");

                        foreach (var name in ing)
                        {
                            var record = new MutationSingleObject();
                            record.AppendObject("name", name);
                            record.AppendObject("tag_type_id", 3);
                            query.AppendObject(record);
                        }

                        var request = Request(query.BulkResult(), conf);
                        var obj = SendHttpRequest<TagGraphQlResponse>(client, request, conf);

                        if (obj.Data?.InsertTag?.Returning != null || obj.Data?.InsertTag?.Returning?.Length >= 0)
                        { 
                            obj.Data.InsertTag.Returning.ForEach(x => result.Add(x.id));
                        }
                    }
                }
            }

            return result.Distinct().ToList();
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
        
        public RecipeTagResponseFromGq SendRecipeStep(RecipeStepRequest[] steps, Guid recipeId, JsonHelper conf)
        {
            RecipeTagResponseFromGq list = new RecipeTagResponseFromGq(); 
            using (var client = new HttpClient())
            {
                var query =  GraphQlRequestBuilder.CreateMutation()
                    .Table("insert_recipe_step")
                    .AppendReturn("id");

                foreach (var t in steps)
                {
                    var record = new MutationSingleObject();
                    record.AppendObject("imageurl", t.ImageUrl);
                    record.AppendObject("order", t.Order);
                    record.AppendObject("text", t.Text.Replace("\"", ""));
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
                adder.AddIngredient(ingredient);
            }
            
            return adder;
        }
        
        
        public List<IngredientUsage> GetIngredients(CreateRecipeRequest req)
        {
            
            var duplicates = req.Ingredients.GroupBy(x => x.Name)
                .Where(g => g.Count() > 1)
                .Select(y => y)
                .ToList();

            var distinct = req.Ingredients
                .GroupBy(customer => customer.Name)
                .Select(group => group.First());;
            
            var ingredientDto = new List<Data.DTO.Ingredients>();
            var allIngredient = new List<IngredientUsage>();
            foreach (var ingredient in distinct)
            {
                var amount = ingredient.Quantity.Unit != null ? 
                    new Amount(ingredient.Quantity.Size,  getUnit(ingredient.Quantity.Unit)) : 
                    new Amount(ingredient.Quantity.Size);
                
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
                allIngredient.Add(new IngredientUsage(ingr,  amount, ingredient.Comment));
            }

            if (context.Adapter is DatabaseAdapter databaseAdapter)
            {
                using (var importer = new DatabaseImporter(databaseAdapter.GetSession()))
                {
                    importer.Import(ingredientDto);
                }
            }
            
            foreach (var group in duplicates)
            {
                var ingredient = allIngredient.Find(x => x.Ingredient.Name == group.Key);
                allIngredient.Remove(ingredient);
                foreach (var value in group)
                {
                    var amount = value.Quantity.Unit != null
                        ? new Amount(value.Quantity.Size, getUnit(value.Quantity.Unit))
                        : new Amount(value.Quantity.Size);
                    var ingr = new Ingredient(ingredient.Ingredient.Id, ingredient.Ingredient.Name);
                    allIngredient.Add(new IngredientUsage(ingr, amount, value.Comment));
                }
            }
            
            return allIngredient;
        }
        
        public DishResponseFromGq GetDishe(Guid mealId, JsonHelper conf)
        {
            DishResponseFromGq list; 
            using (var client = new HttpClient())
            { 
                var query = GraphQlRequestBuilder
                    .CreateQuery()
                    .Table("dish")
                    .AppendReturn("id")
                    .AppendReturn("recipe_id")
                    .AppendCondition(new ConditionType("meal_id", mealId, "_eq"));

                    var request = Request(query.SingleResult(), conf);
                list = SendHttpRequest<DishResponseFromGq>(client, request, conf);
            }

            return list;
        }
 
        public PlanItemsResponseFromGq GetPlanItems(Guid mealId, JsonHelper conf)
        {
            var date = DateTime.Now;
            PlanItemsResponseFromGq list; 
            using (var client = new HttpClient())
            {
                var query = GraphQlRequestBuilder
                    .CreateQuery()
                    .Table("plan_item")
                    .AppendReturn("id")
                    .AppendReturn("plan_id")
                    .AppendReturn("servings")
                    .AppendReturn("date")
                    .AppendCondition(new ConditionType("meal_id", mealId, "_eq"))
                    //.AppendCondition(new ConditionType("date", date, "_gte"))
                    .BulkResult("_and");
                
                var request = Request(query, conf);
                list = SendHttpRequest<PlanItemsResponseFromGq>(client, request, conf);
            }

            return list;
        }

        public CreateRecipeHelper(DBContext ctx)
        {
            context = ctx;
        }
    }
}