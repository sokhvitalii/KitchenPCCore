using System;

namespace KitchenPC.WebApi.Model.error
{
    public class ResponseError : Exception
    {
        
        
        public ResponseError(string m) : base(m)
        {
           
        }
        
        public ResponseError()
        {
        }
    }
}