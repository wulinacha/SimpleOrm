using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
    public class SqliteProvider:BaseProvider
    {
        private DbProviderFactory dbProvider;
        public SqliteProvider()
        {
            dbProvider = GetProvideFactory("System.Data.SQLite.SQLiteFactory,System.Data.SQLite,Version=1.0.105.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139");
        }

        public override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }
    }
}
