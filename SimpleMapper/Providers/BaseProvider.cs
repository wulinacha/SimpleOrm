using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
    public abstract class BaseProvider
    {
        public string connectionString;//连接字符串
        private char pSymbol = '@';//参数符号
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
        protected abstract DbProviderFactory GetDbProvider();
        public abstract string GetPageListSql(string sql, int start=1);
        public abstract DbParameter[] GetParameter(int start, int end, int PageSize);


        protected DbParameter CreateParameter(string name, object value, DbType t = DbType.Object, ParameterDirection pDirection = ParameterDirection.Input)
        {
            var para = CeateDbParameter();
            if (t != DbType.Object) para.DbType = t;
            para.Direction = pDirection;
            if (name[0] == pSymbol)
            {
                para.ParameterName = name;
            }
            else
            {
                para.ParameterName = pSymbol + name;
            }
            para.Value = value;
            return para;
        }
        public string JointSql(string tableName, string column, string where, string sort)
        {
            return "select " + column + " from " + tableName + " " + where + " " + sort;
        }
    }
}
