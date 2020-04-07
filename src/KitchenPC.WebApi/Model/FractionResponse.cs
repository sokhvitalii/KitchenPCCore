namespace KitchenPC.WebApi.Model
{
    public class FractionResponse
    {
        public double Value { get; }
        public string Fraction { get; }
        
        public FractionResponse(string fraction, double value)
        {
            Fraction = fraction;
            Value = value;
        }
        public FractionResponse()
        {
        }
    }
}