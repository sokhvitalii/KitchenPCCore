using System;
using KitchenPC.Context;
using KitchenPC.Parser;
using KitchenPC.Parser.Parsers;

namespace ParserConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize KitchenPC
            KPCContext.Initialize(Configuration<StaticContext>.Build
                .Context(StaticContext.Configure
                    .DataDirectory(@"/home/vitaliisokh/project/work/kitchenpc/core-master-git/src/KitchenPC.WebApi/Resources/")
                ).Create());

            // Initialize Parser Factorydone
            
            ParserFactory.Initialize(
                KPCContext.Current,
                typeof(hRecipeParser).Assembly,
                typeof(hRecipeParser));

            // Test URIs
            Uri[] urls = new[]
            {
                // new Uri("http://allrecipes.com/recipe/classic-peanut-butter-cookies/"),

                new Uri("http://www.food.com/recipe/aunt-eileens-sauce-pan-brownies-256306"),
                new Uri("http://www.food.com/recipe/lower-fat-banana-nut-chip-muffins-199237"),
                new Uri("http://www.food.com/recipe/grated-apple-cinnamon-cake-183836"),
                new Uri("http://www.food.com/recipe/buttery-ricotta-cookies-339259")
            };


            foreach (var u in urls)
            {
                var parser = ParserFactory.GetParser(u);
                var result = parser.Parse(u);

                if (result.Result == ParserResult.Status.Success)
                    Console.WriteLine("Successfully parsed recipe: {0}", result.Recipe.Title);
                else
                    Console.WriteLine("Could not parse {0}", u);
            }
            
            Console.WriteLine("\nDone! [Press Any Key]");
            Console.ReadLine();
        }
    }
}