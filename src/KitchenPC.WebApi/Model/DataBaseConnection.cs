using FluentNHibernate.Cfg.Db;
using KitchenPC.Context;
using KitchenPC.DB;
using KitchenPC.DB.Search;
using KitchenPC.WebApi.Common;

namespace KitchenPC.WebApi.Model
{
    public class DataBaseConnection
    {
        public IConfiguration<DBContext> Context { get; }

        public DataBaseConnection(AuthIdentity authIdentity, JsonHelper conf)
        {
            var postgreConf = PostgreSQLConfiguration.PostgreSQL82
                .ConnectionString(@conf.DBHost)
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