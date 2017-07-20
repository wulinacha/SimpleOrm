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
        protected override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }

        public override string GetPageListSql(string sql, int start = 1)
        {
            string[] sqls = DBProviderHelper.SplitSql(sql);
            var pageSql = new StringBuilder();
            if (start > 1)
                pageSql.AppendFormat("{0} limit @PageStart,@PageLimit", sql);
            else
                pageSql.AppendFormat("{0} limit @PageLimit", sql);
            return pageSql.ToString();
        }

        public override DbParameter[] GetParameter(int start, int end, int PageSize)
        {
            if (start > 1)
            {
                var paras = new DbParameter[2];
                paras[0] = CreateParameter("@PageLimit", PageSize);
                paras[1] = CreateParameter("@PageStart", start);
                return paras;
            }
            else
            {
                var paras = new DbParameter[1];
                paras[0] = CreateParameter("@PageLimit", PageSize);
                return paras;
            }
        }
    }
}
