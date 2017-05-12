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

namespace SimpleMapper
{
    public class AbstractMapper<T> where T : new()
    {
        #region 初始化
        public DataMap map;
        private BaseProvider dbprovider;//数据提供者
        public DbContext context;//连接、事务上下文
        private string connectionString;
        public void RegisterDbContext(DbContext context) {
            this.context = context;
        }
        public AbstractMapper(string connectionString)
        {
            this.connectionString = connectionString;
            this.dbprovider = ProviderFactory.GetProviderFatory(connectionString);
        }
        #endregion
        #region 查询方法
        public T Find(string where)
        {
            T model = new T();
            map = Metadata.GetDataMap(typeof(T));
            string sql = string.Format("select {0} from {1} {2}", map.GetColumnMapListStr(), map.GetTableName(), where);
            var conn = CreateNativeContection();
            using (DbCommand cmd = this.dbprovider.CreateDbCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = sql;
                DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (reader.HasRows)
                {
                    reader.Read();
                    Load(model, reader);
                }
                reader.Close();
            }
            return model;
        }

        public List<T> FinAll(string where) {
            List<T> list = new List<T>();
            map = Metadata.GetDataMap(typeof(T));
            string sql = string.Format("select {0} from {1} {2}", map.GetColumnMapListStr(), map.GetTableName(), where);
            var conn = CreateNativeContection();
            using (DbCommand cmd = this.dbprovider.CreateDbCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = sql;
                DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(SqlMapper.Load(new T(), reader));
                    }
                }
                reader.Close();
                
            }
            return list;
        }
        //分页select * from (select ROW_NUMBER() over(order by id asc) as 'rowNumber', * from tb_User) as temp where rowNumber between (((@pageindex-1)*@pagesize)+1) and (@pageindex*@pagesize)
        public List<T> FindPageList(string where, int pageIndex, int pageSize, out int total,string sort="")
        {
            List<T> list = new List<T>();
            map = Metadata.GetDataMap(typeof(T));
            DBProviderHelper db=new DBProviderHelper();
            var jointSql = dbprovider.JointSql(map.tableName, map.GetColumnMapListStr(), where, sort);
            var start = ((pageIndex - 1) * pageSize) + 1;
            var end=(pageIndex * pageSize);
            string sql = dbprovider.GetPageListSql(jointSql);
            DbParameter[] param = dbprovider.GetParameter(start,end,pageSize);
            var conn = CreateNativeContection();
            using (DbCommand cmd = this.dbprovider.CreateDbCommand())
            {
                //先获取记录总数
                total = GetCount(jointSql);
                cmd.Connection = conn;
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(param);
                DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(Load(new T(), reader));
                    }
                }
                reader.Close();

            }
            return list;
        }
        public int GetCount(string sql)
        {
            var conn = CreateNativeContection();
            try
            {
                using (DbCommand cmd = this.dbprovider.CreateDbCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Connection = conn;
                    var ret = cmd.ExecuteScalar().ToString();
                    int i;
                    if (!int.TryParse(ret, out i))
                    {
                        throw new Exception("can't parse it to int,the value is " + ret);
                    }
                    return i;
                }
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
        }
        public List<TResult> Query<TResult>(string sql, object paramters, params object[] customObject) where TResult:new()
        {
            List<TResult> list = new List<TResult>();
            var conn = CreateNativeContection();
            var paramter=dbprovider.CeateDbParameter();
            //反射获取参数值
            List<DbParameter> param = new List<DbParameter>();
            PropertyInfo[] properties = paramters.GetType().GetProperties();
            foreach (PropertyInfo info in properties)
            {
                param.Add(dbprovider.CreateParameter(info.Name, info.GetValue(paramters)));
            }
            
            try
            {
                using (DbCommand cmd = this.dbprovider.CreateDbCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Connection = conn;
                    cmd.Parameters.AddRange(param.ToArray());
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(Load(new TResult(), reader));
                        }
                    }
                    reader.Close();
                }
                return list;
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
        }
        #endregion
        #region 执行方法
        public int Update(T model,string where) 
        {
            StringBuilder sbSet = new StringBuilder();
            string tableName = "";
            try
            {
                map = Metadata.GetDataMap(typeof(T));
                tableName = map.tableName;
                System.Reflection.PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (System.Reflection.PropertyInfo info in properties)
                {
                    if (info.Name.ToLower() != "id")
                    {
                        if (info.PropertyType == typeof(Int32))
                            sbSet.Append(info.Name + "=" + info.GetValue(model) + ",");
                        else
                            sbSet.Append(info.Name + "=" + "'" + info.GetValue(model) + "'" + ",");
                    }
                }
                string setStr = sbSet.ToString();
                string sql = string.Format(updateSql, tableName, StringHelper.RemoveLastElment(setStr), where);
                var conn=CreateConnection();
                DbCommand command = conn.CreateCommand();
                command.CommandText = sql;
                if (!IsNullContextAndTran()) command.Transaction = context.tran;
                var result= command.ExecuteNonQuery();
                this.Close(conn);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int Insert(T model)
        {
            StringBuilder sbFields = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            string tableName = "";
            try
            {
                map = Metadata.GetDataMap(typeof(T));
                tableName = map.tableName;
                System.Reflection.PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (System.Reflection.PropertyInfo info in properties)
                {
                    if (info.Name.ToLower() != "id")
                    {
                        sbFields.Append(info.Name + ",");
                        if (info.PropertyType == typeof(Int32))
                            sbValues.Append(info.GetValue(model) + ",");
                        else
                            sbValues.Append("'" + info.GetValue(model) + "'" + ",");
                    }
                }
                string fieldsStr = sbFields.ToString();
                string valuesStr = sbValues.ToString();
                string sql = string.Format(insertSql, tableName, StringHelper.RemoveLastElment(fieldsStr), StringHelper.RemoveLastElment(valuesStr));
                var conn = CreateConnection();
                
                DbCommand command = conn.CreateCommand();
                command.CommandText = sql;
                if (!IsNullContextAndTran()) command.Transaction = context.tran;
                var result= command.ExecuteNonQuery();
                command.Dispose();
                this.Close(conn);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int Delete(string where) 
        {
            try
            {
                map = Metadata.GetDataMap(typeof(T));
                string sql = string.Format(deleteSql, map.GetTableName(), where);
                var conn = CreateConnection();
                DbCommand command = conn.CreateCommand();
                command.CommandText = sql;
                if (!IsNullContextAndTran()) command.Transaction = context.tran;
                var result= command.ExecuteNonQuery();
                this.Close(conn);
                command.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region 连接管理
        public void Close(DbConnection conn) {
            if (IsNullContextAndTran())
                conn.Close();
        }
        public DbConnection CreateConnection() {

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
        public bool IsNullContextAndTran() {
            return context.IsNullOrSpace() || context.tran.IsNullOrSpace();
        }
        #endregion
        #region Sql语句
        private string insertSql = @"INSERT INTO {0}({1}) VALUES ({2})";
        private string updateSql = @"UPDATE {0} SET {1} {2}";
        private string deleteSql = "Delete from {0} {1}";
        #endregion
        #region 通用方法
        public string GetWhere<T>(string sql, string tableName, Expression<Func<T, bool>> exrpression)
        {
            return string.Format(sql, tableName, new QueryTranslator().TranslateWhere(exrpression));
        }
        #endregion
    }

    public class SqlMapper
    {
        #region 数据加载
        public static TResult Load<TResult>(TResult model, IDataReader reader) where TResult : new()
        {
            Type[] parameterTypes = new Type[] { typeof(IDataReader) };
            DynamicMethod method = new DynamicMethod(string.Format("Deserialize{0}", Guid.NewGuid()), typeof(object), parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.DeclareLocal(typeof(TResult));
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Stloc_0);
            string[] names = (from i in Enumerable.Range(0,reader.FieldCount) select reader.GetName(i)).ToArray<string>();
            Type t = typeof(TResult);
            PropertyInfo[] properties = t.GetProperties();
            DataMap map = new DataMap(t.Name, t.Name);
            foreach (PropertyInfo info in properties)
            {
                if (reader[info.Name].GetType() == typeof(Int64))
                {
                    info.SetValue(model, Convert.ToInt32(reader[info.Name]));
                    continue;
                }
                info.SetValue(model, reader[info.Name]);
            }
            return model;
        }

        #endregion
    }
}
