using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
    public class MysqlProvider:BaseProvider
    {
        private DbProviderFactory dbProvider;
        public MysqlProvider() {
            dbProvider = GetProvideFactory("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.9.9.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d");
        }

        public override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }
    }
}
