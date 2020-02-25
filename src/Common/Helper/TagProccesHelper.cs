using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using KitchenPC.Data;
using KitchenPC.Data.DTO;

namespace KitchenPC.Helper
{
    public class TagAndTagType
    {
        public string TagTypeName { get; set; }
        public string TagName { get; set; }
        
        public TagAndTagType(string tagTypeName, string tagName)
        {
            TagTypeName = tagTypeName;
            TagName = tagName;
        }
    }

    
    public class TagProcessHelper
    {
        JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        TagAndTagType ParseField(string name)
        {
            string[] splitCamelCase  = Regex.Split(name, @"(?<!^)(?=[A-Z])");
            var tagTypeName = splitCamelCase.First();
            string tagName;

            if (splitCamelCase.Length > 1)
            {
                tagName = string.Join("", splitCamelCase.Skip(1));
            }
            else
            {
                tagName = tagTypeName;
            } 
               
            return new TagAndTagType(tagTypeName, tagName);
        }
        
        public static HttpRequestMessage Request(string query)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/v1/graphql");
            request.Headers.Add("x-hasura-admin-secret", "ADMIN_SECRET_KEY");
            request.Content = new StringContent(query, Encoding.UTF8, "application/json");   
            
            return request; 
        }


        public TagTypeGraphQLResponse SendTagTypt(HttpClient client, IGrouping<string, TagAndTagType> tagType)
        {
            var queryMutationType = GraphQlRequestBuilder.CreateMutable()
                .Table("insert_tag_type")
                .AppendObject("name", tagType.Key)
                .AppendReturn("id").Result();
                    
            HttpRequestMessage request = Request(queryMutationType);

            return client.SendAsync(request)
                .ContinueWith(responseTask =>
                {
                    Console.WriteLine("Response: {0}", responseTask.Result.Content.ReadAsStringAsync().Result);
                    return JsonSerializer.Deserialize<TagTypeGraphQLResponse>(responseTask.Result.Content.ReadAsStringAsync().Result, Options);
                }).Result;  
        }
        
        
        public TagGraphQlResponse SendTag(HttpClient client, TagAndTagType tag, InsertTagType tagType)
        {
            var queryMutationTag = GraphQlRequestBuilder.CreateMutable()
                .Table("insert_tag")
                .AppendObject("name", tag.TagName)
                .AppendObject("tag_type_id", tagType.Returning.First().id)
                .AppendReturn("id").Result();
                        
            HttpRequestMessage requestTag = Request(queryMutationTag);
            
            return client.SendAsync(requestTag)
                .ContinueWith(responseTask =>
                {
                    Console.WriteLine("Response: {0}", responseTask.Result.Content.ReadAsStringAsync().Result);
                    return JsonSerializer.Deserialize<TagGraphQlResponse>(responseTask.Result.Content.ReadAsStringAsync().Result, Options);
                }).Result;  
        }
        
        public void SendRecipeTag(HttpClient client, Guid recipeId, InsertTagType tag)
        {
            var queryMutationRecipeTag = GraphQlRequestBuilder.CreateMutable()
                .Table("insert_recipe_tag")
                .AppendObject("recipe_id", recipeId.ToString())
                .AppendObject("tag_id", tag.Returning.First().id)
                .AppendReturn("id").Result();
                        
            HttpRequestMessage requestRecipeTag = Request(queryMutationRecipeTag);
                    
          
            client.SendAsync(requestRecipeTag).ContinueWith(responseTask =>
            { 
                Console.WriteLine("Response: {0}", responseTask.Result.Content.ReadAsStringAsync().Result);
            }); 
        }

        public void combineSendTagsAndRecipe(IEnumerable<IGrouping<string, TagAndTagType>> list, Guid recipeId)
        {
            using (var client = new HttpClient())
            {
                foreach (var tagType in list)
                {
                    var tagTypeResult = SendTagTypt(client, tagType);

                    foreach (var tag in tagType)
                    {
                        var tagResult = SendTag(client, tag, tagTypeResult.Data.InsertTagType);
                        SendRecipeTag(client, recipeId, tagResult.Data.InsertTagType);
                    }

                }
            }

        }


        public void ImportTagToHasura(DataStore store)
        {
            var tag = new TagProcessHelper();
            foreach (var metadata in store.RecipeMetadata)
            {
                tag.combineSendTagsAndRecipe(tag.MetadataProcess(metadata), metadata.RecipeId);
            }
            
        }

        public IEnumerable<IGrouping<string, TagAndTagType>> MetadataProcess(RecipeMetadata metadata)
        {
            return new List<TagAndTagType>
            {
                ParseField(nameof(metadata.Commonality)),
                metadata.MealBreakfast ? ParseField(nameof(metadata.MealBreakfast)) : null,
                metadata.MealDessert ? ParseField(nameof(metadata.MealDessert)) : null,
                metadata.MealDinner ? ParseField(nameof(metadata.MealDinner)) : null,
                metadata.MealLunch ? ParseField(nameof(metadata.MealLunch)) : null,
                ParseField(nameof(metadata.PhotoRes)),
                metadata.SkillCommon ? ParseField(nameof(metadata.SkillCommon)) : null,
                metadata.SkillEasy ? ParseField(nameof(metadata.SkillEasy)) : null,
                metadata.SkillQuick ? ParseField(nameof(metadata.SkillQuick)) : null,
                metadata.UsdaMatch ? ParseField(nameof(metadata.UsdaMatch)) : null,
                metadata.DietGlutenFree ? ParseField(nameof(metadata.DietGlutenFree)) : null,
                metadata.DietNoAnimals ? ParseField(nameof(metadata.DietNoAnimals)) : null,
                metadata.DietNoPork ? ParseField(nameof(metadata.DietNoPork)) : null,
                metadata.NutritionLowCalorie ? ParseField(nameof(metadata.NutritionLowCalorie)) : null,
                metadata.NutritionLowCarb ? ParseField(nameof(metadata.NutritionLowCarb)) : null,
                metadata.NutritionLowFat ? ParseField(nameof(metadata.NutritionLowFat)) : null,
                metadata.NutritionLowSodium ? ParseField(nameof(metadata.NutritionLowSodium)) : null,
                metadata.NutritionLowSugar ? ParseField(nameof(metadata.NutritionLowSugar)) : null,
                ParseField(nameof(metadata.NutritionTotalfat)),
                ParseField(nameof(metadata.NutritionTotalCalories)),
                ParseField(nameof(metadata.NutritionTotalCarbs)),
                ParseField(nameof(metadata.NutritionTotalSodium)),
                ParseField(nameof(metadata.NutritionTotalSugar)),
                metadata.DietNoRedMeat ? ParseField(nameof(metadata.DietNoRedMeat)) : null,
                ParseField(nameof(metadata.TasteMildToSpicy)),
                ParseField(nameof(metadata.TasteSavoryToSweet))
            }.Where(x => x != null).ToList().GroupBy(x => x.TagTypeName);
        }

    }
}