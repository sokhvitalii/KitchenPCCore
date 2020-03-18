using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DB.Helper;
using KitchenPC.Data;
using KitchenPC.Data.DTO;
using KitchenPC.DB.Helper;

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
            string[] splitCamelCase = Regex.Split(name, @"(?<!^)(?=[A-Z])");
            var tagTypeName = splitCamelCase.First();
            string tagName = splitCamelCase.Length > 1 ? string.Join("", splitCamelCase.Skip(1)) : tagTypeName;
            
            return new TagAndTagType(tagTypeName, tagName);
        }

        public static HttpRequestMessage Request(string query)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/v1/graphql");
            request.Headers.Add("x-hasura-admin-secret", "ADMIN_SECRET_KEY");
            request.Content = new StringContent(query, Encoding.UTF8, "application/json");

            return request;
        }


        public TagTypeGraphQLResponse SendTagType(HttpClient client, IGrouping<string, TagAndTagType> tagType)
        {
            var queryMutationType = GraphQlRequestBuilder.CreateMutation()
                .Table("insert_tag_type")
                .AppendObject("name", tagType.Key)
                .AppendReturn("id").Result();

            HttpRequestMessage request = Request(queryMutationType);

            return client.SendAsync(request)
                .ContinueWith(responseTask =>
                {
                    Console.WriteLine("Response from tag_type: {0}",
                        responseTask.Result.Content.ReadAsStringAsync().Result);
                    return JsonSerializer.Deserialize<TagTypeGraphQLResponse>(
                        responseTask.Result.Content.ReadAsStringAsync().Result, Options);
                }).Result;
        }


        public TagGraphQlResponse SendTag(HttpClient client, TagAndTagType tag, InsertTagType tagType)
        {
            var queryMutationTag = GraphQlRequestBuilder.CreateMutation()
                .Table("insert_tag")
                .AppendObject("name", tag.TagName)
                .AppendObject("tag_type_id", tagType.Returning.First().id)
                .AppendReturn("id").Result();

            HttpRequestMessage requestTag = Request(queryMutationTag);

            return client.SendAsync(requestTag)
                .ContinueWith(responseTask =>
                {
                    Console.WriteLine("Response from tag: {0}", responseTask.Result.Content.ReadAsStringAsync().Result);
                    return JsonSerializer.Deserialize<TagGraphQlResponse>(
                        responseTask.Result.Content.ReadAsStringAsync().Result, Options);
                }).Result;
        }

        public void SendRecipeTag(HttpClient client, Guid recipeId, int tagId)
        {
            var queryMutationRecipeTag = GraphQlRequestBuilder.CreateMutation()
                .Table("insert_recipe_tag")
                .AppendObject("recipe_id", recipeId.ToString())
                .AppendObject("tag_id", tagId)
                .AppendReturn("id").Result();

            HttpRequestMessage requestRecipeTag = Request(queryMutationRecipeTag);


            client.SendAsync(requestRecipeTag).ContinueWith(responseTask =>
            {
                Console.WriteLine("Response from recipe_tag: {0}",
                    responseTask.Result.Content.ReadAsStringAsync().Result);
            });
        }

        public void combineSendTagsAndRecipe(List< TagAndTagType> list, Guid recipeId, Dictionary<string, int> map)
        {
            
            using (var client = new HttpClient())
            {
                foreach (var tagType in list)
                {
                    var id = map.SingleOrDefault(x => x.Key == tagType.TagName).Value;
                    SendRecipeTag(client, recipeId, id);
                }
            }
        }

        List<TagAndTagType> PrepareData()
        {
            return new List<TagAndTagType>
            {
                ParseField(nameof(RecipeMetadata.Commonality)),
                ParseField(nameof(RecipeMetadata.MealBreakfast)),
                ParseField(nameof(RecipeMetadata.MealDessert)),
                ParseField(nameof(RecipeMetadata.MealDinner)),
                ParseField(nameof(RecipeMetadata.MealLunch)),
                ParseField(nameof(RecipeMetadata.PhotoRes)),
                ParseField(nameof(RecipeMetadata.SkillCommon)),
                ParseField(nameof(RecipeMetadata.SkillEasy)),
                ParseField(nameof(RecipeMetadata.SkillQuick)),
                ParseField(nameof(RecipeMetadata.UsdaMatch)),
                ParseField(nameof(RecipeMetadata.DietGlutenFree)),
                ParseField(nameof(RecipeMetadata.DietNoAnimals)),
                ParseField(nameof(RecipeMetadata.DietNoPork)),
                ParseField(nameof(RecipeMetadata.NutritionLowCalorie)),
                ParseField(nameof(RecipeMetadata.NutritionLowCarb)),
                ParseField(nameof(RecipeMetadata.NutritionLowFat)),
                ParseField(nameof(RecipeMetadata.NutritionLowSodium)),
                ParseField(nameof(RecipeMetadata.NutritionLowSugar)),
                ParseField(nameof(RecipeMetadata.NutritionTotalfat)),
                ParseField(nameof(RecipeMetadata.NutritionTotalCalories)),
                ParseField(nameof(RecipeMetadata.NutritionTotalCarbs)),
                ParseField(nameof(RecipeMetadata.NutritionTotalSodium)),
                ParseField(nameof(RecipeMetadata.NutritionTotalSugar)),
                ParseField(nameof(RecipeMetadata.DietNoRedMeat)),
                ParseField(nameof(RecipeMetadata.TasteMildToSpicy)),
                ParseField(nameof(RecipeMetadata.TasteSavoryToSweet))
            };        }

        public void ImportTagToHasura(DataStore store)
        {
            var prepareData = PrepareData().GroupBy(x => x.TagTypeName);
            Dictionary<string, int> map = new Dictionary<string, int>();
            using (var client = new HttpClient())
                
            {
                foreach (var tagType in prepareData)
                {
                    var tagTypeResult = SendTagType(client, tagType);

                    foreach (var tag in tagType)
                    {
                        var tagResult = SendTag(client, tag, tagTypeResult.Data.InsertTagType);
                        map.Add(tag.TagName, tagResult.Data.InsertTag.Returning.First().id);
                    }
                }
            }
            
            var tags = new TagProcessHelper();
            foreach (var metadata in store.RecipeMetadata)
            {
                tags.combineSendTagsAndRecipe(tags.MetadataProcess(metadata), metadata.RecipeId, map);
            }
        }

        public List<TagAndTagType> MetadataProcess(RecipeMetadata metadata)
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
            }.Where(x => x != null).ToList();
        }
    }
}