using System.Text.Json.Serialization;

namespace DB.Helper
{
    public class Ids
    {
        public int id { get; set; }
       
        public Ids()
        {
        }
    } 
    
    public class InsertTagType
    {
        public Ids[] Returning { get; set; }
       
        public InsertTagType()
        {
        }
    }
        
        
    public class DataTagType
    {
        [JsonPropertyName("insert_tag_type")]
        public InsertTagType InsertTagType { get; set; }
       
        public DataTagType()
        {
        }
    }       
    
    public class DataTag
    {
        [JsonPropertyName("insert_tag")]
        public InsertTagType InsertTag { get; set; }
       
        public DataTag()
        {
        }
    }   
    
    public class TagTypeGraphQLResponse
    {
        public DataTagType Data { get; set; }

        public TagTypeGraphQLResponse()
        {

        }
    }
    
    public class TagGraphQlResponse
    {
        public DataTag Data { get; set; }

        public TagGraphQlResponse()
        {

        }
    }
}