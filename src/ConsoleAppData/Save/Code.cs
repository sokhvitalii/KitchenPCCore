using System;
using FluentNHibernate.Cfg.Db;
using KitchenPC;
using KitchenPC.Context;
using KitchenPC.DB;
using KitchenPC.DB.Search;

namespace ConsoleAppData.Save
{
    public class Code
    {


        public void Connect()
        {
            var path = "/home/vitaliisokh/project/work/kitchenpc/test2/KitchenPCCore/WebApplication2/Resources/";
            var staticConfig = Configuration<StaticContext>.Build
                .Context(StaticContext.Configure
                    .DataDirectory(path)
                    .Identity(() => new AuthIdentity("c52a2874-bf95-4b50-9d45-a85a84309e75", "Mike"))
                )
                .Create();

            var postgreSettings = PostgreSQLConfiguration.PostgreSQL82
                .ConnectionString(@"Server=localhost;Port=5432;User Id=pguser;Password=postgres;Database=KPCTest")
                .ShowSql();

            var dbConfig = Configuration<DBContext>.Build
                .Context(DBContext.Configure
                    .Adapter(
                        DatabaseAdapter.Configure
                            .DatabaseConfiguration(postgreSettings)
                            .SearchProvider(NHSearch.Instance)
                    )
                    .Identity(() => new AuthIdentity("c52a2874-bf95-4b50-9d45-a85a84309e75", "Mike"))
                ).Create();


        }
        
        
        
        
        
        /**
* uri
* url
* source
* yield
* calories
* totalWeight
* dietLabels
* healthLabels
* cautions
* ingredientLines
* ingredients {
*  text
*  quantity
*  }
* totalNutrients {
*  label
*  quantity
*  unit
*  }
* digest {
*  tag
*  schemaOrgTag
*  label
*  unit
*  total
*  hasRDI
*  daily
*  }
* cuisineType
* mealType
* dishType
*/
            
        /*dbConfig.Context.InitializeStore();
        staticConfig.Context.Initialize();
        dbConfig.Context.Import(staticConfig.Context);
        */

        /*
        KPCContext.Initialize(dbConfig);
        */

        /*
        var recipes = dbConfig.Context.Recipes
            .Load(Recipe.FromId(new Guid("ee371336-c183-448d-889c-d6b1f767bf59"))) // Weeknight Cheese Quiche
            .Load(Recipe.FromId(new Guid("e6ab278f-5f4d-4249-ab7d-8f66af044265"))) // Berry-Good Mousse
            .WithMethod
            .WithUserRating
            .List();*/
    }
}