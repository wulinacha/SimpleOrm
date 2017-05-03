using Framwork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class AbstractMapper<T> where T : new()
    {
        #region 初始化
        public readonly string connstr = ConfigHelper.GetConfigurationManagerStr("SqlConnection");
        public DataMap map;
        private SqlConnection conn;//连接对象
        private SqlTransaction tran;//事务对象
        #endregion
        #region 查询方法
        public T Find(string where) 
        {
            T model = new T();
            map=Metadata.GetDataMap(typeof(T));
            string sql=string.Format("select {0} from {1} {2}",map.GetColumnMapListStr(),map.GetTableName(),where);
            using (SqlConnection con = new SqlConnection(connstr))
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    if (reader.HasRows)//HasRows判断reader中是否有数据
                    {
                        reader.Read();
                        Load(model,reader);
                    }
                    reader.Close();
                }
            }
            return model;
        }

        public void Load(T model, SqlDataReader reader)
        {
            Type t = typeof(T);
            System.Reflection.PropertyInfo[] properties = t.GetProperties();
            DataMap map = new DataMap(t.Name, t.Name);
            foreach (System.Reflection.PropertyInfo info in properties)
            {
                info.SetValue(model, reader[info.Name]);
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
                CreateConnection();
                SqlCommand command = conn.CreateCommand();
                command.CommandText=sql;
                if (tran != null) command.Transaction = tran;
                return command.ExecuteNonQuery();
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
                CreateConnection();
                SqlCommand command = conn.CreateCommand();
                command.CommandText=sql;
                if (tran != null) command.Transaction = tran;
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int Delete(string where) 
        {
            map = Metadata.GetDataMap(typeof(T));
            string sql = string.Format(deleteSql,map.GetTableName(), where);
            CreateConnection();
            SqlCommand command = conn.CreateCommand();
            command.CommandText = sql;
            if (tran != null) command.Transaction = tran;
            return command.ExecuteNonQuery();
        }
        #endregion
        #region 连接池管理
        //创建连接
        public void CreateConnection() {
            if (conn == null || conn.State != ConnectionState.Open)
            {
                conn = new SqlConnection(connstr);
                conn.Open();
            }
        }
        //创建事务
        public void CreateTransaction() {
            CreateConnection();
            tran = conn.BeginTransaction();
        }
        //提交事务
        public void CommitTransaction() {
            CreateConnection();
            tran.Commit();
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
