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

        protected override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }

        public override string GetPageListSql(string sql, int start = 1)
        {
            string[] sqls = DBProviderHelper.SplitSql(sql);
            var pageSql = new StringBuilder();
            pageSql.AppendFormat(" Select top (@PageEnd) ROW_NUMBER() over ({0}) RN , {1}",
                    string.IsNullOrEmpty(sqls[3]) ? "ORDER BY (SELECT NULL)" : sqls[3],
                    sqls[2]);

            //如果查询不是第一页，则需要判断起始行号
            if (start > 1)
            {
                pageSql.Insert(0, "Select PageTab.* from ( ");
                pageSql.Append(" ) PageTab Where RN >= @PageStart");
            }
            return pageSql.ToString();
        }

        public override DbParameter[] GetParameter(int start, int end, int PageSize)
        {
            if (start > 1)
            {
                var paras = new DbParameter[2];
                paras[1] = CreateParameter("@PageEnd", end);
                paras[0] = CreateParameter("@PageStart", start);
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
