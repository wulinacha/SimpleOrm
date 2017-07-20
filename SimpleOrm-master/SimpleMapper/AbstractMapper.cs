using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SimpleMapper.infrastructure;
using SimpleMapper.TransForTool;
using SimpleMapper.Providers;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace SimpleMapper
{
    public class AbstractMapper
    {
        #region init
        public DataMap map;
        private BaseProvider dbprovider;
        public DbContext context;
        private string connectionString;
        public bool isStronglyTyped;
        public void RegisterDbContext(DbContext context)
        {
            this.context = context;
        }
        public AbstractMapper(string connectionString)
        {
            this.connectionString = connectionString;
            this.dbprovider = ProviderFactory.GetProviderFatory(connectionString);
        }
        #endregion

        [ContractInvariantMethod]
        private void InvariantCondition() {
            Contract.Invariant(!this.connectionString.IsNullOrSpace(),"数据库链接字符串不能为空");
        }

        private IEnumerable<T> QueryCommandList<T>(string querySql, DbParameter[] paramters = null)
        {
            Contract.Requires<AggregateException>(!querySql.IsNullOrSpace(), "查询SQL语句不能为空");
            DbConnection dbConn = CreateNativeContection();
            DbDataReader reader = null;
            try
            {
                reader = QueryCommandReader(querySql,paramters);
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        yield return SqlMapper.Load<T>(map._type, reader, SqlMapper.GetHashKey(this.connectionString, querySql));
                    }
                }
                reader.Close();
                yield return default(T);
            }
            finally
            {
                reader.Close();
            }
        }
        private DbDataReader QueryCommandReader(String querySql, DbParameter[] paramters = null)
        {
            DbConnection dbConn = null;
            try
            {
                dbConn= CreateNativeContection();
                using (DbCommand cmd = this.dbprovider.CreateDbCommand())
                {
                    cmd.Connection = dbConn;
                    cmd.CommandText = querySql;
                    if (!paramters.IsNullOrSpace() && paramters.Any())
                        cmd.Parameters.AddRange(paramters.ToArray());
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception ex)
            {
                if(!dbConn.IsNullOrSpace())
                dbConn.Close();
                throw;
            }
           
        }
        private int CommandImpl(string sql)
        {
            var conn = CreateConnection();
            using (DbCommand command = this.dbprovider.CreateDbCommand())
            {
                command.Connection = conn;
                command.CommandText = sql;
                if (!IsNullContextAndTran()) command.Transaction = context.tran;
                var result = command.ExecuteNonQuery();
                Close(conn);
                return result;
            }
        }
        private List<DbParameter> CreateParater(object paramters)
        {
            List<DbParameter> param = new List<DbParameter>();
            Type paramtersType = null;
            if (!paramters.IsNullOrSpace())
            {
                paramtersType = paramters.GetType();
                param = SqlMapper.GetParams(paramters, paramtersType, dbprovider, SqlMapper.GetHashKey(this.connectionString, paramtersType));
            }

            return param;
        }

        #region query
        public T Find<T>(string where) where T : new()
        {
            map = GetDataMap<T>();
            string querySql = string.Format("select {0} from {1} {2}", map.GetColumnMapListStr(), map.tableName, where);
            return this.QueryCommandList<T>(querySql).FirstOrDefault();
        }

        public IEnumerable<T> FinAll<T>(string where) where T : new()
        {
            map = GetDataMap<T>();
            string querySql = string.Format("select {0} from {1} {2}", map.GetColumnMapListStr(), map.tableName, where);
            return this.QueryCommandList<T>(querySql,null);
        }
        //分页select * from (select ROW_NUMBER() over(order by id asc) as 'rowNumber', * from tb_User) as temp where rowNumber between (((@pageindex-1)*@pagesize)+1) and (@pageindex*@pagesize)
        public IEnumerable<T> FindPageList<T>(string where, int pageIndex, int pageSize, out int total, string sort = "") where T : new()
        {
            map = GetDataMap<T>();
            DBProviderHelper db = new DBProviderHelper();
            var jointSql = dbprovider.JointSql(map.tableName, map.GetColumnMapListStr(), where, sort);
            var start = ((pageIndex - 1) * pageSize) + 1;
            var end = (pageIndex * pageSize);
            string querySql = dbprovider.GetPageListSql(jointSql);
            DbParameter[] param = dbprovider.GetParameter(start, end, pageSize);
            //先获取记录总数
            total = GetCount("select count(1) from " + map.tableName + " " + where);
            return this.QueryCommandList<T>(querySql,param);
        }
        public int GetCount(string querySql)
        {
            IDataReader rd = null;
            rd = this.QueryCommandReader(querySql);
            rd.Read();
            return rd[0].IsNullOrSpace() ? 0 : (int)rd[0];
        }
        public List<TResult> QueryReflect<TResult>(string querySql, object paramters = null) where TResult : new()
        {
            var list = new List<TResult>();
            map = GetDataMap<TResult>();
            //反射获取参数值
            List<DbParameter> param = new List<DbParameter>();
            CreateParamster(paramters,ref param);
            var reader = QueryCommandReader(querySql, param.ToArray());
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    list.Add(Load(new TResult(), reader));
                }
            }
            reader.Close();
            return list;
        }
        public TResult QuerySingle<TResult>(string qurySql, object paramters)
        {
            map = GetDataMap<TResult>();
            List<DbParameter> param = CreateParater(paramters);
            return this.QueryCommandList<TResult>(qurySql, param.ToArray()).FirstOrDefault();
        }
        public IEnumerable<TResult> Query<TResult>(string qurySql, object paramters)
        {
            map = GetDataMap<TResult>();
            List<DbParameter> param = CreateParater(paramters);
            return this.QueryCommandList<TResult>(qurySql, param.ToArray());
        }

        //根和聚合集合
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string querySql, Delegate act, object paramters = null) where TReturn : new()
        {
            TReturn tReturn = new TReturn();
            List<DbParameter> param = CreateParater(paramters);
            var reader = this.QueryCommandReader(querySql, param.ToArray());
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    tReturn = SqlMapper.MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(act, reader, SqlMapper.GetHashKey(connectionString, querySql));
                }
            }
            reader.Close();
            return tReturn;
        }
        //多结果集
        public QueryReader QueryMultiple(string sql, object paramters = null)
        {
            QueryReader queryReader = null;
            var conn = CreateNativeContection();
            List<DbParameter> param = CreateParater(paramters);
            using (DbCommand cmd = this.dbprovider.CreateDbCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                cmd.Parameters.AddRange(param.ToArray());
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                queryReader = new QueryReader(reader, cmd, this.connectionString);
                return queryReader;
            }
        }
        #endregion

        #region cmommd
        public int Update<T>(T model, string where)
        {
            string sql = SqlMapper.CreateUpdateMethod(model, SqlMapper.GetHashKey(connectionString, GetDataMap<T>().tableName + where));
            return CommandImpl(sql);
        }

        public int Insert<T>(T model)
        {
            string sql = SqlMapper.CreateInsertMethod(model, SqlMapper.GetHashKey(connectionString, GetDataMap<T>().tableName));
            return CommandImpl(sql);
        }

        public int Delete<T>(string where)
        {
            string sql = string.Format("Delete from {0} {1}", GetDataMap<T>().tableName, where);
            return CommandImpl(sql);
        }
        public int Excute(string sql)
        {
            return CommandImpl(sql);
        }
        #endregion

        public void Close(DbConnection conn)
        {
            if (IsNullContextAndTran())
                conn.Close();
        }
        public DbConnection CreateConnection()
        {

            if (IsNullContextAndTran())
                return CreateNativeContection();

            return context.conn;
        }
        public DbConnection CreateNativeContection()
        {
            DbConnection conn = dbprovider.CreateDbConnection();
            conn.ConnectionString = this.connectionString;
            conn.Open();
            return conn;
        }
        public bool IsNullContextAndTran()
        {
            return context.IsNullOrSpace() || context.tran.IsNullOrSpace();
        }

        public DataMap GetDataMap<T>() {
            return (map.IsNullOrSpace()||!isStronglyTyped)? Metadata.GetDataMap(typeof(T)):map;
        }



        public static TResult Load<TResult>(TResult model, IDataReader reader) where TResult : new()
        {
            Type t = typeof(TResult);
            PropertyInfo[] properties = t.GetProperties();
            DataMap map = new DataMap(t.Name, t.Name, t);
            foreach (PropertyInfo info in properties)
            {
                if (reader[info.Name].GetType() == typeof(Int64))
                {
                    info.SetValue(model, Convert.ToInt32(reader[info.Name]));
                    continue;
                }
            }
            return model;
        }
        private void CreateParamster(object paramters, ref List<DbParameter> param)
        {
            if (!paramters.IsNullOrSpace())
            {
                PropertyInfo[] properties = paramters.GetType().GetProperties();
                foreach (PropertyInfo info in properties)
                {
                    param.Add(dbprovider.CreateParameter(info.Name, info.GetValue(paramters)));
                }
            }
        }

    }

    public class DontMap
    { }

}
