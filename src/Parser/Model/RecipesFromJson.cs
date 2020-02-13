using System.Collections.Generic;

namespace Parser.Model
{
    public class NutrientsFromJson
    {
        public string Label { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        
        public NutrientsFromJson()
        {
                
        }
    }
    
    public class IngredientsFromJson
    {
        public string Text { get; set; }
        public double Quantity { get; set; }
        public string Measure { get; set; }
        public string Food { get; set; }
        public double Weight { get; set; }
        public IngredientsFromJson()
        {
                
        }
    }

    public class SubFromJson
    {
        public string Tag { get; set; }
        public string SchemaOrgTag { get; set; }
        public string Label { get; set; }
        public string Unit { get; set; }
        public double Total { get; set; }
        public bool HasRDI { get; set; }
        public double Daily { get; set; }

        public SubFromJson()
        {
            
        }

    }
    public class DigestFromJson
    {
        public string Tag { get; set; }
        public string SchemaOrgTag { get; set; }
        public string Label { get; set; }
        public string Unit { get; set; }
        public double Total { get; set; }
        public bool HasRDI { get; set; }
        public double Daily { get; set; }
        public SubFromJson[] Sub { get; set; }

        public DigestFromJson()
        {
            
        }
    }
    public class RecipesFromJson
    {
        public string Uri { get; set; }
        public string Label { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public string Source { get; set; }
        public double Yield { get; set; }
        public double Calories { get; set; }
        public double TotalWeight { get; set; }
        public string[] DietLabels { get; set; }
        public string[] HealthLabels { get; set; }
        public string[] Cautions { get; set; }
        public string[] IngredientLines { get; set; }
        public IngredientsFromJson[] Ingredients { get; set; }
        public Dictionary<string, NutrientsFromJson> TotalNutrients { get; set; }
        public DigestFromJson[] Digest { get; set; }
        public string Instructions { get; set; }
        public double TotalTime { get; set; }
        public string[] CuisineType { get; set; }
        public string[] MealType { get; set; }
        public string[] DishType { get; set; }

        public RecipesFromJson()
        {
            
        }
    }
}