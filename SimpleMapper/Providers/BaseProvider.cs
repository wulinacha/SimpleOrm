using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
    public abstract class BaseProvider
    {
        public string connectionString;//连接字符串
        protected DbProviderFactory GetProvideFactory(string assseblyname)
        {
            Type provideclass = Type.GetType(assseblyname);
            return (DbProviderFactory)provideclass.GetField("Instance").GetValue(null);
        }

        public DbConnection CreateDbConnection() {
            return GetDbProvider().CreateConnection();
        }

        public DbCommand CreateDbCommand() {
            return GetDbProvider().CreateCommand();
        }
        public DbParameter CeateDbParameter() {
            return GetDbProvider().CreateParameter();
        }
        public abstract DbProviderFactory GetDbProvider();
    }
}
