using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ConsoleAppData.Helper;
using KitchenPC.Helper;
using KitchenPC.Data;
using KitchenPC.Data.DTO;
using KitchenPC.WebApi.Model;

namespace ConsoleAppData
{

    
    class Program
    {

        public static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        
        static void Main(string[] args)
        {
             //DataBaseHelper.SaveInitData();
            

            var data =
                "{\"event\": {\"session_variables\": {\"x-hasura-role\": \"admin\",\"x-hasura-user-id\": \"e\"},\"op\": \"INSERT\",\"data\": {\"new\": {\"recipe_id\": \"4140a462-216e-4573-b721-c5b74818a568\",\"meal\": \"dffffffffffffffff\",\"id\": 4,\"plan_id\": 2,\"servings\": \"2edasdrere\"}} }}";

            StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");

            var bar = nameof(SessionVariables.HasuraUserId);


            
            
            var list = new List<TagAndTagType>
            {
                new TagAndTagType("333333e", "vvvvvv"),
                new TagAndTagType("vetal33", "rrrrttttt"),
                new TagAndTagType("vetal33", "vvvvvv"),
                new TagAndTagType("333333e", "vvvvvv")
            }.GroupBy(x => x.TagName);
            
            var recipeId = new Guid("f419a451-c11a-4e00-82ff-17d73cfbfe75");


            TagProcessHelper tagHelper = new TagProcessHelper();
            
            using (var client = new HttpClient())
            {
                /*client.BaseAddress = uri;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("x-hasura-admin-secret", "ADMIN_SECRET_KEY");*/
                
                foreach (var tagType in list)
                {
                  
                    var tagTypeResult = tagHelper.SendTagType(client, tagType);
                    foreach (var tag in tagType)
                    {
                        var tagResult = tagHelper.SendTag(client, tag, tagTypeResult.Data.InsertTagType);
                        tagHelper.SendRecipeTag(client, recipeId, tagResult.Data.InsertTag.Returning.First().id);
                        
                    }

                }
            }
            
            
            
            
            /*
            StringBuilder builder = new StringBuilder();
            foreach (char c in bar) {
                if (Char.IsUpper(c) && builder.Length > 0) builder.Append(' ');
                builder.Append(c);
            }*/
            
            string[] SplitCamelCase(string source) {
                return Regex.Split(source, @"(?<!^)(?=[A-Z])");
            }

            var tt = SplitCamelCase(bar);

            var yyy = tt.Skip(1);

            var iii = string.Join("", yyy);
            

            /*
            HttpResponseMessage response =
                client2.PostAsync(new Uri("http://localhost:5001/ShoppingList"), queryString).Result;

            string responseBody = response.Content.ReadAsStringAsync().Result;
            */
            
            

            Console.WriteLine("111111111111111111111111 = ");
        }

    }
}