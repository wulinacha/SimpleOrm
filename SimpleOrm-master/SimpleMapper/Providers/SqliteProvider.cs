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
        protected override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }

        public override string GetPageListSql(string sql, int start=1)
        {
            string[] sqls = DBProviderHelper.SplitSql(sql);
            var pageSql = new StringBuilder();
            if(start>1)
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
