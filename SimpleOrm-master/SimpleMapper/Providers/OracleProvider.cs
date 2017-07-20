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
        protected override DbProviderFactory GetDbProvider()
        {
            return this.dbProvider;
        }

        public override string GetPageListSql(string sql, int start = 1)
        {
            string[] sqls = DBProviderHelper.SplitSql(sql);
            var pageSql = new StringBuilder();
            pageSql.Append("select ROWNUM RN,  PageTab.* from ");
            pageSql.AppendFormat(" ( {0} ) PageTab ", sql);
            pageSql.Append(" where ROWNUM <= :PageEnd ");

            //如果查询不是第一页，则需要判断起始行号
            if (start > 1)
            {
                pageSql.Insert(0, "select * from ( ");
                pageSql.Append(" ) Where RN>= :PageStart ");
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
