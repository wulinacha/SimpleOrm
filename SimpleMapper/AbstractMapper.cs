using Framwork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class AbstractMapper<T> where T : new()
    {
        public static readonly string connstr = ConfigHelper.GetConfigurationManagerStr("SqlConnection");
        public static DataMap map;

        public static T Find(string where) 
        {
            T model = new T();
            map=Metadata.GetDataMap(typeof(T));
            string sql=string.Format("select {0} from {1} {2}",map.GetColumnMapListStr(),map.GetTableName(),where);
            using (SqlConnection con = new SqlConnection(connstr))
            {
                con.Open();
                SqlDataAdapter Adapter = new SqlDataAdapter();
                DataSet ds = new DataSet();
                Adapter.SelectCommand = new SqlCommand(sql, con);
                Adapter.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                 Load(model, ds.Tables[0].Rows[0]);
            }
            return model;
        }

        public static void Load(T model, DataRow row)
        {
            Type t=typeof(T);
            System.Reflection.PropertyInfo[] properties = t.GetProperties();
            DataMap map = new DataMap(t.Name, t.Name);
            foreach (System.Reflection.PropertyInfo info in properties)
            {
                info.SetValue(model, row[info.Name]);
            }
        }

        public static int Update(T model,string where) 
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
                using (SqlConnection con = new SqlConnection(connstr))
                {
                    con.Open();
                    SqlCommand command = new SqlCommand(sql, con);
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static int Insert(T model)
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
                using (SqlConnection con = new SqlConnection(connstr))
                {
                    con.Open();
                    SqlCommand command = new SqlCommand(sql, con);
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static int Delete(string where) 
        {
            map = Metadata.GetDataMap(typeof(T));
            string sql = string.Format(deleteSql,map.GetTableName(), where);
            using (SqlConnection con = new SqlConnection(connstr))
            {
                con.Open();
                SqlCommand command = new SqlCommand(sql,con);
                return command.ExecuteNonQuery();
            }
        }

        private static string insertSql = @"INSERT INTO {0}({1}) VALUES ({2})";
        private static string updateSql = @"UPDATE {0} SET {1} {2}";
        private static string deleteSql = "Delete from {0} {1}";

        public static string GetWhere<T>(string sql, string tableName, Expression<Func<T, bool>> exrpression)
        {
            return string.Format(sql, tableName, new QueryTranslator().TranslateWhere(exrpression));
        }
    }
}
