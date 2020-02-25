using System;
using FluentNHibernate.Cfg.Db;
using KitchenPC;
using KitchenPC.Context;
using KitchenPC.DB;
using KitchenPC.DB.Search;

namespace ConsoleAppData.Helper
{
    public class DataBaseHelper
    {
        private static IConfiguration<DBContext> DBConfiguration()
        {
            var PostgreConf = PostgreSQLConfiguration.PostgreSQL82
                .ConnectionString(@"Server=localhost;Port=5432;User Id=pguser;Password=postgres;Database=KPCTest")
                .ShowSql();
            
            return Configuration<DBContext>.Build
                .Context(DBContext.Configure
                    .Adapter(
                        DatabaseAdapter.Configure
                            .DatabaseConfiguration(PostgreConf)
                            .SearchProvider(NHSearch.Instance)
                    )
                    .Identity(() => new AuthIdentity("c52a2874-bf95-4b50-9d45-a85a84309e75", "Mike"))
                ).Create();
        }
        
        public static void SaveInitData()
        {
            var path = "/home/vitaliisokh/project/work/kitchenpc/core-master-git/src/KitchenPC.WebApi/Resources/";
            var staticConfig = Configuration<StaticContext>.Build
                .Context(StaticContext.Configure
                    .DataDirectory(path)
                    .Identity(() => new AuthIdentity("c52a2874-bf95-4b50-9d45-a85a84309e75", "Mike"))
                )
                .Create();
            
            var dbConfig = DBConfiguration();
            
            dbConfig.Context.Initialize();
            
            staticConfig.Context.Initialize();
            dbConfig.Context.Import(staticConfig.Context);
        }

        public static IConfiguration<DBContext> DBConnector()
        {
            var dbConfig = DBConfiguration();
            
            dbConfig.Context.Initialize();

            return dbConfig;
        }
    }
}