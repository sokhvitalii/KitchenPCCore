using FluentNHibernate.Cfg.Db;
using KitchenPC.Context;
using KitchenPC.DB;
using KitchenPC.DB.Search;

namespace KitchenPC.WebApi.Model
{
    public class DataBaseConnection
    {
        public IConfiguration<DBContext> Context { get; }

        public DataBaseConnection(AuthIdentity authIdentity)
        {
            var postgreConf = PostgreSQLConfiguration.PostgreSQL82
                .ConnectionString(@"Server=localhost;Port=5432;User Id=pguser;Password=postgres;Database=KPCTest")
                .ShowSql();
            
            Context = Configuration<DBContext>.Build
                .Context(DBContext.Configure
                    .Adapter(
                        DatabaseAdapter.Configure
                            .DatabaseConfiguration(postgreConf)
                            .SearchProvider(NHSearch.Instance)
                    )
                    .Identity(() => authIdentity)
                ).Create();

            Context.Context.Initialize();
        }
    }
}