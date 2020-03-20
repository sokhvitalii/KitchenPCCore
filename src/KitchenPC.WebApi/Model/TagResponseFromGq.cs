using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KitchenPC.WebApi.Model
{
    
    public class TagsGQ
    {
        public int Id { get; set; }
        public string Name { get; set; }
       
        public TagsGQ(int id)
        {
            Id = id;
        }  
        
        public TagsGQ()
        {
        }
    } 
    
    public class DataReturning
    {
    
        public TagsGQ[] Returning { get; set; }
       
        public DataReturning()
        {
        }
    }

    public class DataRecipeTag
    {
        [JsonPropertyName("insert_recipe_tag")]
        public DataReturning Tag { get; set; }
       
        public DataRecipeTag()
        {
        }
    }

    public class RecipeTagResponseFromGq
    {
        public DataRecipeTag Data { get; set; }

        public RecipeTagResponseFromGq()
        {

        }
    }
    
    public class DataTag
    {
        public List<TagsGQ> Tag { get; set; }
       
        public DataTag()
        {
        }
    }

    public class TagResponseFromGq
    {
        public DataTag Data { get; set; }

        public TagResponseFromGq()
        {

        }
    }
}