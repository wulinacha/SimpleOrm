using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
    public class SqlerverProvider : BaseProvider
    {
        private DbProviderFactory dbProvider;
        public SqlerverProvider(){
             dbProvider = GetProvideFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }

        public override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }
    }
}
