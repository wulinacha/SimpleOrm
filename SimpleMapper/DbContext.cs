using SimpleMapper.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    /// <summary>
    /// db上下文就是为了共享事务、连接,必须这样做，例如在不同数据映射器的时候，可以共享上下文，如果连接和事务都在一个映射器，那就麻烦了！
    /// </summary>
    public class DbContext
    {
        public DbConnection conn;//连接对象
        public DbTransaction tran;//事务对象
        private BaseProvider dbprovider;//数据提供者
        private string connectionString;

        public DbContext(string connectionString) {
            this.connectionString = connectionString;
            dbprovider = ProviderFactory.GetProviderFatory(connectionString);
        }

        #region 连接池管理
        //创建连接
        public void CreateConnection()
        {
            if(conn==null)
                conn = dbprovider.CreateDbConnection();

            if (conn.State != ConnectionState.Open)
            {
                conn.ConnectionString = this.connectionString;
                conn.Open();
            }
        }
        public void Close()
        {
           this.conn.Close();
           this.tran.Dispose();
        }
        //创建事务
        public void CreateTransaction(IsolationLevel level = IsolationLevel.Unspecified)
        {
            CreateConnection();
            tran = conn.BeginTransaction(level);
        }
        //提交事务
        public void CommitTransaction()
        {
            CreateConnection();
            tran.Commit();
            Close();
        }
        //回滚事务
        public void Rollback()
        {
            if (tran != null)
            {
                tran.Rollback();
                tran.Dispose();
                tran = null;
                Close();
            }
        }
        #endregion
    }
}
