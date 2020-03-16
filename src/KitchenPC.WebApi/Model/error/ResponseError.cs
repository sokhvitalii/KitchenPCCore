namespace KitchenPC.WebApi.Model.error
{
    public class ResponseError
    {
        public string Message { get; set; }
        
        public ResponseError(string m)
        {
            Message = m;
        }
        
        public ResponseError()
        {
        }
    }
}