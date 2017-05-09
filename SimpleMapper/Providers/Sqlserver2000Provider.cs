using SimpleMapper.Providers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class Sqlserver2000Provider : BaseProvider
    {
        private DbProviderFactory dbProvider;
        public Sqlserver2000Provider()
        {
             dbProvider = GetProvideFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }
        protected override System.Data.Common.DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }
        public override string GetPageListSql(string sql, int start=1)
        {
            string[] sqls = DBProviderHelper.SplitSql(sql);
            var pageSql = new StringBuilder();
            pageSql.AppendFormat("SELECT TOP @PageEnd {0} {1}", sqls[2], sqls[3]);
            if (start > 1)
            {
                var orderChange = string.IsNullOrEmpty(sqls[5]) ? null :
                    string.Compare(sqls[5], "desc", true) == 0 ?
                    string.Format("{0} ASC ", sqls[4]) :
                    string.Format("{0} DESC ", sqls[4]);
                pageSql.Insert(0, "SELECT TOP 100 PERCENT  * FROM (SELECT TOP @PageLimit * FROM ( ");
                pageSql.AppendFormat(" ) PageTab {0} ) PageTab2 {1}", orderChange, sqls[3]);
            }
            return pageSql.ToString();
        }

        public override DbParameter[] GetParameter(int start, int end, int PageSize)
        {
            if (start > 1)
            {
                var paras = new DbParameter[2];
                paras[0] = CreateParameter("@PageLimit", PageSize);
                paras[1] = CreateParameter("@PageEnd", end);
                return paras;
            }
            else
            {
                var paras = new DbParameter[1];
                paras[0] = CreateParameter("@PageEnd", end);
                return paras;
            }
        }
    }
}
