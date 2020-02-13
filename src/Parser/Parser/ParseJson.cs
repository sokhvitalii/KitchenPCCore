using System;
using System.Collections.Generic;
using System.IO;
using  System.Text.Json;
using System.Text.Json.Serialization;
using NHibernate.Mapping;
using Parser.Model;

namespace Parser.Parser
{
    /*
    public class TestJson
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public TestJson()
        {
        
        }
        public TestJson(string name, int age)
        {
            Name = name;
            Age = age;
        }  
        var test = new TestJson("vetal", 31);
                    JsonDocument result = null;
                    using (var fileStream = new FileStream(new Uri("/home/vitaliisokh/project/work/kitchenpc/test2/KitchenPCCore/Parser/Resources/Edamam_recipes.json", UriKind.Absolute).AbsolutePath, FileMode.Open))
                    {    
                       result = JsonDocument.Parse(fileStream);
                    }
        //var jsonDocument =  JsonDocument.Parse("\"name\": \"sdsdsd\", \"age\": 55");
        // var objDes = JsonSerializer.Serialize(test, options);
        // JsonValue = objDes;
    }*/
    
    public class ParseJson
    {
        public JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public RecipesFromJson[] RecipesFromJson { get; set; }
        public RecipesFromJson[] GetRecipesFromJson()
        { 
            var tmp = File.ReadAllText("../Parser/Resources/Edamam_recipes.json"); 
            return JsonSerializer.Deserialize<RecipesFromJson[]>(tmp, Options);
        }
        public ParseJson()
        {
            var recipes = GetRecipesFromJson();
            RecipesFromJson = recipes;
        }
    }
}