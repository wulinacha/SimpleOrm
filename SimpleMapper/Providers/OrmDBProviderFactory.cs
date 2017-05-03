using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleMapper.Providers
{
  public partial class DbHelper
    {
        enum DBType
        {
            SqlServer2000,
            SqlServer,
            Oracle,
            SQLite
        }

        #region 成员定义
        private DbProviderFactory dbProvider;//数据库Provider
        private DBType dbType;//数据库类型
        private char pSymbol = '@';//参数符号

        private DbConnection conn;//连接对象
        private string connectionString;//连接字符串
        private DbTransaction tran;//事务对象

        private IList parameterList = new List<DbParameter>();//过程参数列表
        private bool hasOutput = false;//是否包含输出参数
        private Dictionary<string, object> dicPara = new Dictionary<string, object>();//输出参数列表
        #endregion

        #region 构造方法，实例化连接字符串

        /// <summary>
        /// 读取WebConfig链接字符串
        /// </summary>
        /// <param name="connectionName">ConnectionString配置名</param>
        public DbHelper(string connectionName = "")
        {
            //默认使用ConnectionString第一项
            var config = string.IsNullOrEmpty(connectionName) ?
                ConfigurationManager.ConnectionStrings[0] :
                ConfigurationManager.ConnectionStrings[connectionName];
            dbProvider = DbProviderFactories.GetFactory(config.ProviderName);
            connectionString = config.ConnectionString;
            CommonConstruct(config.ProviderName);
        }

        /// <summary>
        /// 有参构造，实例化连接字符串
        /// </summary>
        /// <param name="provider">DbProvider</param>
        /// <param name="connectionString">连接字符串</param>
        public DbHelper(DbProviderFactory provider, string connectionString)
        {
            this.dbProvider = provider;
            this.connectionString = connectionString;
            CommonConstruct(provider.GetType().Name);
        }

        private void CommonConstruct(string _dbtype = "")
        {
            // Try using type name first (more reliable)
            if (_dbtype.StartsWith("Oracle")) dbType = DBType.Oracle;
            else if (_dbtype.StartsWith("SQLite")) dbType = DBType.SQLite;
            else if (_dbtype.StartsWith("System.Data.SqlClient")) dbType = DBType.SqlServer;
            // else try with provider name
            else if (_dbtype.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0) dbType = DBType.Oracle;
            else if (_dbtype.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0) dbType = DBType.SQLite;

            if (dbType == DBType.Oracle)
                pSymbol = ':';
            else
                pSymbol = '@';
        }
        #endregion

        #region 实现接口IDisposable
        /// <释放资源接口>
        /// 实现接口IDisposable
        /// </释放资源接口>
        public void Dispose()
        {
            if (conn != null)
            {
                if (conn.State == ConnectionState.Open)//判断数据库连接池是否打开
                {
                    conn.Close();
                }

                if (parameterList.Count > 0)//判断参数列表是否清空
                {
                    parameterList.Clear();
                }
                conn.Dispose();//释放连接池资源
                GC.SuppressFinalize(this);//垃圾回收
            }
        }
        #endregion

        #region 执行SQL或存储过程 并返回影响的行数
        /// <summary>
        /// 执行SQL，并返回影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            using (var cmd = CreateCommand(sql))
            {
                return ExecuteNonQuery(cmd);
            }
        }

        /// <summary>
        /// 执行存储过程，并返回影响的行数
        /// </summary>
        /// <param name="storeProcedureName">存储过程名</param>
        /// <returns></returns>
        public int ExecuteProceudre(string storeProcedureName)
        {
            using (var cmd = CreateCommand(storeProcedureName, CommandType.StoredProcedure))
            {
                return ExecuteNonQuery(cmd);
            }
        }
        #endregion

        #region 执行SQL或者存储过程，并返回DataTable
        /// <summary>
        /// 执行SQL语句并返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public DataTable ExecuteSql(string sql)
        {
            using (var cmd = CreateCommand(sql))
            {
                return Execute(cmd);
            }
        }

        /// <summary>
        /// 执行存储过程并返回DataTable
        /// </summary>
        /// <param name="storeProcedureName">存储过程名</param>
        /// <returns></returns>
        public DataTable ExecuteProc(string storeProcedureName)
        {
            using (var cmd = CreateCommand(storeProcedureName, CommandType.StoredProcedure))
            {
                return Execute(cmd);
            }
        }
        #endregion

        #region 执行SQL或存储过程并返回DbDataReader
        /// <summary>
        /// 执行SQL语句并返回DbDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>返回DbDataReader</returns>
        public DbDataReader ExecuteReader(string sql)
        {
            using (var cmd = CreateCommand(sql))
            {
                return ExecuteReader(cmd);
            }
        }

        /// <summary>
        /// 执行存储过程并返回DbDataReader
        /// </summary>
        /// <param name="storeProcedureName">存储过程名</param>
        /// <returns>返回DbDataReader</returns>
        public DbDataReader ExecuteProcReader(string storeProcedureName)
        {
            using (var cmd = CreateCommand(storeProcedureName, CommandType.StoredProcedure))
            {
                return ExecuteReader(cmd);
            }
        }
        #endregion

        #region 执行统计
        /// <summary>
        /// 执行SQL语句 返回首行首列的值,一般用于统计
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>查询结果首行首列的值转换为整形，转换失败则返回-1</returns>
        public int Count(string sql)
        {
            using (var cmd = CreateCommand(sql))
            {
                return ExecuteScalar(cmd);
            }
        }
        #endregion

        #region 测试连接是否成功
        /// <summary>
        /// 测试连接是否成功
        /// </summary>
        /// <returns></returns>
        public bool HasConnection
        {
            get
            {
                try
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        #endregion

        #region 索引器访问
        public object this[string name]
        {
            set
            {
                this[name, DbType.Object, ParameterDirection.Input] = value;
            }
            get
            {
                object obj;
                if (dicPara.TryGetValue(name, out obj))
                {
                    return obj;
                }
                return null;
            }
        }

        public object this[string name, DbType dbtype]
        {
            set
            {
                this[name, dbtype, ParameterDirection.Input] = value;
            }
        }

        public object this[string name, DbType dbType, ParameterDirection direction]
        {
            set
            {
                if (name[0] != pSymbol) name = pSymbol + name;

                var para = dbProvider.CreateParameter();
                if (dbType != DbType.Object)
                    para.DbType = dbType;
                para.ParameterName = name;
                para.Value = value == null ? DBNull.Value : value;
                parameterList.Add(para);
            }
        }
        #endregion

        #region 命令相关处理
        /// <summary>
        /// 创建DbCommand
        /// </summary>
        /// <param name="cmdText">命名文本</param>
        /// <param name="cmdType">命名类型</param>
        /// <returns></returns>
        private DbCommand CreateCommand(string cmdText, CommandType cmdType = CommandType.Text)
        {
            //创建数据库连接对象
            if (conn == null || conn.State != ConnectionState.Open)
            {
                conn = dbProvider.CreateConnection();
                conn.ConnectionString = connectionString;
                conn.Open();//打开数据库连接池
            }

            //创建Command命令
            var cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandType = cmdType;
            if (!string.IsNullOrEmpty(cmdText))
                cmd.CommandText = cmdText;
            if (tran != null) cmd.Transaction = tran;
            cmd.CommandTimeout = 0;

            //加载过程参数
            LoadParamter(cmd);
            return cmd;
        }

        /// <summary>
        /// 创建过程参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="t">参数值类型</param>
        /// <param name="pDirection">参数类型</param>
        /// <returns></returns>
        private DbParameter CreateParameter(string name, object value, DbType t = DbType.Object, ParameterDirection pDirection = ParameterDirection.Input)
        {
            var para = dbProvider.CreateParameter();
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

        /// <summary>
        /// 执行Command 并返回影响的行数
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <returns></returns>
        private int ExecuteNonQuery(DbCommand cmd)
        {
            try
            {
                return cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
            finally
            {
                if (tran == null) Dispose();
            }
        }

        /// <summary>
        /// 执行Command 并返回影响的行数
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <returns></returns>
        private int ExecuteScalar(DbCommand cmd)
        {
            try
            {
                var ret = cmd.ExecuteScalar().ToString();
                int i;
                if (!int.TryParse(ret, out i))
                {
                    throw new Exception("can't parse it to int,the value is " + ret);
                }
                return i;
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
            finally
            {
                if (tran == null) Dispose();
            }
        }

        /// <summary>
        /// 执行Command 并返回DataTable
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <returns></returns>
        private DataTable Execute(DbCommand cmd)
        {
            try
            {
                using (var adapter = dbProvider.CreateDataAdapter())//创建适配器
                {
                    adapter.SelectCommand = cmd;
                    adapter.SelectCommand.CommandTimeout = 0;
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;//返回结果集
                }
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
            finally
            {
                if (tran == null) Dispose();
            }
        }

        /// <summary>
        /// 执行Command 并返回DbDataReader
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <returns></returns>
        private DbDataReader ExecuteReader(DbCommand cmd)
        {
            try
            {
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
            finally
            {
                if (tran == null) Dispose();
            }
        }

        /// <summary>
        /// 加载输出参数至字典中 仅当执行非查询时才调用
        /// </summary>
        /// <param name="Parameters"></param>
        private void InitDic(DbParameterCollection Parameters)
        {
            if (hasOutput)
            {
                dicPara.Clear();
                foreach (DbParameter Para in Parameters)
                {
                    if (Para.Direction != ParameterDirection.Input)
                    {
                        dicPara.Add(Para.ParameterName, Para.Value);
                    }
                }
                hasOutput = false;
            }
        }

        /// <summary>
        /// 加载过程参数输入至Commond中
        /// </summary>
        /// <param name="cmd"></param>
        private void LoadParamter(DbCommand cmd)
        {
            if (parameterList.Count != 0)
            {
                foreach (DbParameter Para in parameterList)
                {
                    if (!hasOutput && Para.Direction != ParameterDirection.Input)
                    {
                        hasOutput = true;
                    }
                    cmd.Parameters.Add(Para);
                }
                parameterList.Clear();
            }
        }

        /// <summary>
        /// 将参数化Sql转换成纯Sql，便于调试
        /// </summary>
        /// <param name="strSql">SQL语句</param>
        /// <returns></returns>
        private string getSqlOnly(DbCommand cmd)
        {
            var sql = cmd.CommandText;
            foreach (DbParameter para in cmd.Parameters)
            {
                if (para.DbType == DbType.Int16 || para.DbType == DbType.Int32 || para.DbType == DbType.Int64 || para.DbType == DbType.UInt16 || para.DbType == DbType.UInt32 || para.DbType == DbType.UInt64 || para.DbType == DbType.Decimal || para.DbType == DbType.Double || para.DbType == DbType.Single)
                {
                    sql = sql.Replace(para.ParameterName, para.Value.ToString());
                }
                else if (pSymbol == '@' || para.DbType == DbType.AnsiString || para.DbType == DbType.String || para.DbType == DbType.StringFixedLength)
                {
                    sql = sql.Replace(para.ParameterName, "'" + para.Value.ToString() + "'");
                }
                else if (pSymbol == ':' || para.DbType == DbType.DateTime || para.DbType == DbType.DateTime2 || para.DbType == DbType.DateTimeOffset)
                {
                    //排除未知的时间类型
                    DateTime time;
                    if (DateTime.TryParse(para.Value.ToString(), out time))
                    {
                        sql = sql.Replace(para.ParameterName, "to_date('" + time.ToString() + "','yyyy-MM-dd hh24:mi:ss')");
                    }
                }
            }
            return sql;
        }
        #endregion

        #region 分页查询
        /// <summary>
        /// 对指定Sql语句查询的结果集进行分页
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="start">结果集起始行号，不包括此行</param>
        /// <param name="limit">取出的行数</param>
        /// <returns></returns>
        public DataTable ExecuteSql(string sql, int start, int limit)
        {
            string[] sqls;
            var pageParms = CreatePageSql(sql, out sqls, start, limit);
            using (var cmd = CreateCommand(sqls[1]))
            {
                cmd.Parameters.AddRange(pageParms);
                return Execute(cmd);
            }
        }

        /// <summary>
        /// 对指定Sql语句查询的结果集进行分页
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="start">结果集起始行号，不包括此行</param>
        /// <param name="limit">取出的行数</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(string sql, int start, int limit)
        {
            string[] sqls;
            var pageParms = CreatePageSql(sql, out sqls, start, limit);
            using (var cmd = CreateCommand(sqls[1]))
            {
                cmd.Parameters.AddRange(pageParms);
                return ExecuteReader(cmd);
            }
        }

        /// <summary>
        /// 对指定Sql语句查询的结果集进行分页
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="start">结果集起始行号，不包括此行</param>
        /// <param name="limit">取出的行数</param>
        /// <param name="count">输出总行数</param>
        /// <returns></returns>
        public DataTable ExecuteSql(string sql, int start, int limit, out int count)
        {
            //查看是否已经使用事务,若没有使用事务，这里必须使用事务执行
            var alreadyUserTran = tran != null;
            if (!alreadyUserTran)
                BeginTransation();

            string[] sqls;
            var pageParms = CreatePageSql(sql, out sqls, start, limit, true);
            using (var cmd = CreateCommand(sqls[0]))
            {
                count = ExecuteScalar(cmd);

                //加载逆序分页 并返回过程参数
                var pageReverse = CreatePageSqlReverse(sql, ref sqls, start, limit, count);
                if (pageReverse != null)
                    cmd.Parameters.AddRange(pageParms);
                else
                    cmd.Parameters.AddRange(pageParms);

                cmd.CommandText = sqls[1];
                var dt = Execute(cmd);

                //如果事先已开启事务，则不在此处提交事务，应由用户调用时手动提交，否则自动提交方法
                if (!alreadyUserTran)
                    tran.Commit();

                return dt;
            }
        }

        /// <summary>
        /// 对指定Sql语句查询的结果集进行分页
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="start">结果集起始行号，不包括此行</param>
        /// <param name="limit">取出的行数</param>
        /// <param name="count">输出总行数</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(string sql, int start, int limit, out int count)
        {
            //查看是否已经使用事务,若没有使用事务，这里必须使用事务执行
            var alreadyUserTran = tran != null;
            if (!alreadyUserTran)
                BeginTransation();

            string[] sqls;
            var pageParms = CreatePageSql(sql, out sqls, start, limit, true);
            using (var cmd = CreateCommand(sqls[0]))
            {
                count = ExecuteScalar(cmd);

                //加载逆序分页 并返回过程参数
                var pageReverse = CreatePageSqlReverse(sql, ref sqls, start, limit, count);
                if (pageReverse != null)
                    cmd.Parameters.AddRange(pageParms);
                else
                    cmd.Parameters.AddRange(pageParms);

                cmd.CommandText = sqls[1];
                return ExecuteReader(cmd);
            }
        }
        #endregion

        #region 开启事务
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public void BeginTransation()
        {
            //创建数据库连接对象
            if (conn == null || conn.State != ConnectionState.Open)
            {
                conn = dbProvider.CreateConnection();
                conn.ConnectionString = connectionString;
                conn.Open();//打开数据库连接池
            }
            tran = conn.BeginTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            if (tran != null)
            {
                tran.Commit();
                tran.Dispose();
                tran = null;
                Dispose();
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            if (tran != null)
            {
                tran.Rollback();
                tran.Dispose();
                tran = null;
                Dispose();
            }
        }
        #endregion

        #region 生成 分页SQL语句
        /// <summary>
        /// 匹配移除Select后的sql
        /// </summary>
        private Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// 匹配SQL语句中Order By字段
        /// </summary>
        private Regex rxOrderBy = new Regex(@"\b(?<ordersql>ORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+)(?:\s+(?<order>ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// 匹配SQL语句中Distinct
        /// </summary>
        private Regex rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// 分析Sql语句 输出分析数组 信息依次为:
        /// 0.countsql
        /// 1.pageSql(保留位置此处不做分析)
        /// 2.移除了select的sql
        /// 3.order by 字段 desc
        /// 4.order by 字段
        /// 5.desc
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string[] SplitSqlForPaging(string sql)
        {
            var sqlInfo = new string[6];
            // Extract the columns from "SELECT <whatever> FROM"
            var m = rxColumns.Match(sql);
            if (!m.Success)
                return null;

            // Save column list and replace with COUNT(*)
            Group g = m.Groups[1];
            sqlInfo[2] = sql.Substring(g.Index);

            if (rxDistinct.IsMatch(sqlInfo[2]))
                sqlInfo[0] = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                sqlInfo[0] = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);


            // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlInfo[0]);
            if (!m.Success)
            {
                sqlInfo[3] = null;
            }
            else
            {
                g = m.Groups[0];
                sqlInfo[3] = g.ToString();
                //统计的SQL 移除order
                sqlInfo[0] = sqlInfo[0].Substring(0, g.Index) + sqlInfo[0].Substring(g.Index + g.Length);
                //存储排序信息
                sqlInfo[4] = m.Groups["ordersql"].Value;//order by xxx
                sqlInfo[5] = m.Groups["order"].Value;//desc 

                //select部分 移除order
                sqlInfo[2] = sqlInfo[2].Replace(sqlInfo[3], string.Empty);
            }

            return sqlInfo;
        }

        /// <summary>
        /// 生成逆序分页Sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqls"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <param name="total"></param>
        private DbParameter[] CreatePageSqlReverse(string sql, ref string[] sqls, int start, int limit, int total = 0)
        {
            //如果总行数不多或分页的条数位于前半部分，没必要逆序分页
            if (total < 100 || start <= total / 2)
            {
                return null;
            }

            //sql正则分析过后的数组有5个值，若未分析，此处分析
            if (sqls == null || sqls.Length == 6)
            {
                sqls = SplitSqlForPaging(sql);
                if (sqls == null)
                {
                    //无法解析的SQL语句
                    throw new Exception("can't parse sql to pagesql ,the sql is " + sql);
                }
            }

            //如果未定义排序规则，则无需做逆序分页计算
            if (string.IsNullOrEmpty(sqls[5]))
            {
                return null;
            }

            //逆序分页检查
            string sqlOrder = sqls[3];
            int end = start + limit;

            //获取逆序排序的sql
            string sqlOrderChange = string.Compare(sqls[5], "desc", true) == 0 ?
                string.Format("{0} ASC ", sqls[4]) :
                string.Format("{0} DESC ", sqls[4]);

            /*理论
             * total:10000 start:9980 limit:10 
             * 则 end:9990 分页条件为 RN >= 9980+1 and RN <= 9990
             * 逆序调整后 
             * start = total - start = 20
             * end = total - end + 1 = 11
             * 交换start和end，分页条件为 RN >= 11 and RN<= 20
             */
            //重新计算start和end
            start = total - start;
            end = total - end + 1;
            //交换start end
            start = start + end;
            end = start - end;
            start = start - end;


            //定义分页SQL
            var pageSql = new StringBuilder();

            if (dbType == DBType.SqlServer2000)
            {
                pageSql.AppendFormat("SELECT TOP @PageLimit * FROM ( SELECT TOP @PageEnd {0} {1} ) ", sqls[2], sqlOrderChange);
            }
            else if (dbType == DBType.SqlServer)
            {
                //组织分页SQL语句
                pageSql.AppendFormat("SELECT PageTab.* FROM ( SELECT TOP @PageEnd ROW_NUMBER() over ({0}) RN , {1}  ) PageTab ",
                    sqlOrderChange,
                    sqls[2]);

                //如果查询不是第一页，则需要判断起始行号
                if (start > 1)
                {
                    pageSql.Append("Where RN >= :PageStart ");
                }
            }
            else if (dbType == DBType.Oracle)
            {
                pageSql.AppendFormat("SELECT ROWNUM RN,  PageTab.* FROM  ( Select {0} {1} ) PageTab  where ROWNUM <= :PageEnd ", sqls[2], sqlOrderChange);

                //如果查询不是第一页，则需要判断起始行号
                if (start > 1)
                {
                    pageSql.Insert(0, "SELECT * FROM ( ");
                    pageSql.Append(" ) ");
                    pageSql.Append(" WHERE RN>= :PageStart ");
                }
            }
            else if (dbType == DBType.SQLite)
            {
                pageSql.AppendFormat("SELECT * FROM ( SELECT {0} {1} limit  @PageStart,@PageLimit ) PageTab ", sqls[2], sqlOrderChange);
            }

            //恢复排序
            pageSql.Append(sqlOrder);

            //存储生成的分页SQL语句  
            sqls[1] = pageSql.ToString();

            //临时测试
            //sqls[1] = sqls[1].Replace("@", "").Replace(":", "").Replace("PageStart", start + "").Replace("PageEnd", end + "").Replace("PageLimit", limit + "");

            //组织过程参数
            DbParameter[] paras = null;
            if (dbType == DBType.SqlServer2000 || dbType == DBType.SQLite)
            {
                paras = new DbParameter[2];
                paras[0] = CreateParameter("@PageLimit", limit);
                paras[1] = CreateParameter("@PageEnd", end);
            }
            else if (start > 1)
            {
                paras = new DbParameter[2];
                paras[0] = CreateParameter("@PageStart", start);
                paras[1] = CreateParameter("@PageEnd", end);
            }
            else
            {
                paras = new DbParameter[] { CreateParameter("@PageEnd", end) };
            }

            return paras;
        }

        /// <summary>
        /// 生成常规Sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqls"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <param name="createCount"></param>
        private DbParameter[] CreatePageSql(string sql, out string[] sqls, int start, int limit, bool createCount = false)
        {
            //需要输出的sql数组
            sqls = null;

            //生成count的SQL语句 SqlServer生成分页，必须通过正则拆分
            if (createCount || dbType == DBType.SqlServer || dbType == DBType.SqlServer2000)
            {
                sqls = SplitSqlForPaging(sql);
                if (sqls == null)
                {
                    //无法解析的SQL语句
                    throw new Exception("can't parse sql to pagesql ,the sql is " + sql);
                }
            }
            else
            {
                sqls = new string[2];
            }

            //组织分页SQL语句
            var pageSql = new StringBuilder();

            //构建分页参数
            var end = start + limit;
            start++;

            if (dbType == DBType.SqlServer2000)
            {
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
            }
            else if (dbType == DBType.SqlServer)
            {
                pageSql.AppendFormat(" Select top (@PageEnd) ROW_NUMBER() over ({0}) RN , {1}",
                    string.IsNullOrEmpty(sqls[3]) ? "ORDER BY (SELECT NULL)" : sqls[3],
                    sqls[2]);

                //如果查询不是第一页，则需要判断起始行号
                if (start > 1)
                {
                    pageSql.Insert(0, "Select PageTab.* from ( ");
                    pageSql.Append(" ) PageTab Where RN >= @PageStart");
                }
            }
            else if (dbType == DBType.Oracle)
            {
                pageSql.Append("select ROWNUM RN,  PageTab.* from ");
                pageSql.AppendFormat(" ( {0} ) PageTab ", sql);
                pageSql.Append(" where ROWNUM <= :PageEnd ");

                //如果查询不是第一页，则需要判断起始行号
                if (start > 1)
                {
                    pageSql.Insert(0, "select * from ( ");
                    pageSql.Append(" ) Where RN>= :PageStart ");
                }
            }
            else if (dbType == DBType.SQLite)
            {
                pageSql.AppendFormat("{0} limit @PageStart,@PageLimit", sql, start, limit);
            }

            //存储生成的分页SQL语句  
            sqls[1] = pageSql.ToString();

            //临时测试
            //sqls[1] = sqls[1].Replace("@", "").Replace(":", "").Replace("PageStart", start + "").Replace("PageEnd", end + "").Replace("PageLimit", limit + "");

            //组织过程参数
            DbParameter[] paras;
            if (dbType == DBType.SqlServer2000 || dbType == DBType.SQLite)
            {
                paras = new DbParameter[2];
                paras[0] = CreateParameter("@PageLimit", limit);
                paras[1] = CreateParameter("@PageEnd", end);
            }
            else if (start > 1)
            {
                paras = new DbParameter[2];
                paras[0] = CreateParameter("@PageStart", start);
                paras[1] = CreateParameter("@PageEnd", end);
            }
            else
            {
                paras = new DbParameter[] { CreateParameter("@PageEnd", end) };
            }

            return paras;
        }
        #endregion

    }

}
