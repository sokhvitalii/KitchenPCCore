namespace KitchenPC.WebApi.Model
{
    
    public class TagsGQ
    {
        public int Id { get; set; }
        public string Name { get; set; }
       
        public TagsGQ()
        {
        }
    } 
    
    public class DataTag
    {
        public TagsGQ[] Tag { get; set; }
       
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