using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
    public class OracleProvider:BaseProvider
    {
        private DbProviderFactory dbProvider;
        public OracleProvider() {
            this.dbProvider = GetProvideFactory("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Version=4.121.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342");
        }

        public override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }
    }
}
