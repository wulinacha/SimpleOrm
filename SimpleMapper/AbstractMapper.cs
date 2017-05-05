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

        public T Load(T model, DbDataReader reader)
        {
            Type t = typeof(T);
            System.Reflection.PropertyInfo[] properties = t.GetProperties();
            DataMap map = new DataMap(t.Name, t.Name);
            foreach (System.Reflection.PropertyInfo info in properties)
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
                        list.Add(Load(new T(), reader));
                    }
                }
                reader.Close();
                
            }
            return list;
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
}
