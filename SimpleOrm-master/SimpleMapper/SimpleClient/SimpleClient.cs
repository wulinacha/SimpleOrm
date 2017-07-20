using SimpleMapper;
using SimpleMapper.TransForTool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class StoreSimpleClient<T> where T:new()
    {
        private AbstractMapper mapper;
        private string connectionString;
        private QueryTranslator translator;
        public StoreSimpleClient(string connectionString) {
            this.connectionString = connectionString;
            mapper = new AbstractMapper(connectionString);
            mapper.isStronglyTyped = true;
            translator = new QueryTranslator();
        }
        public StoreSimpleClient(string connectionString,DbContext context)
        {
            this.connectionString = connectionString;
            mapper = new AbstractMapper(connectionString);
            mapper.isStronglyTyped = true;
            mapper.RegisterDbContext(context);
            translator = new QueryTranslator();
        }
        public T Get(Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            return mapper.Find<T>(where);
        }
        public IEnumerable<T> GetList(Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            return mapper.FinAll<T>(where);
        }

        public PageList<T> GetPageList(Expression<Func<T, bool>> expression, int pageIndex, int pageSize)
        {
            string where = translator.TranslateWhere(expression);
            int total = 0;
            List<T> list = mapper.FindPageList<T>(where, pageIndex, pageSize, out total).ToList();
            return new PageList<T>() { pageIndex=pageIndex,pageSize=pageSize, rowCount = total, Items = list };
        }

        public int Update(T model, Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            return mapper.Update(model, where);
        }

        public int UpdatePart(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            string update = translator.TranslateUpdate(updateExpression);
            return mapper.Excute(update + where);
        }

        public int Insert(T model) {
            return mapper.Insert(model);
        }

        public int Detele(Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            return mapper.Delete<T>(where);
        }
        public TResult QuerySingle<TResult>(string sql, object paramters)
        {
            return mapper.QuerySingle<TResult>(sql, paramters);
        }
        public List<TResult> Query<TResult>(string sql, object paramters)
        {
            return mapper.Query<TResult>(sql, paramters).ToList();
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql,Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> act,object paramters=null) where TReturn:new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public QueryReader QueryMultiple(string sql,object paramters=null){
            return mapper.QueryMultiple(sql, paramters);
        }
        public List<TResult> QueryReflect<TResult>(string sql, object paramters) where TResult : new()
        {
            return mapper.QueryReflect<TResult>(sql, paramters);
        }
    }

    public class NonSimpleClient
    {
        private AbstractMapper mapper;
        private QueryTranslator translator;
        public NonSimpleClient(string connectionString)
        {
            mapper = new AbstractMapper(connectionString);
            mapper.isStronglyTyped = false;
            translator = new QueryTranslator();
        }
        public NonSimpleClient(string connectionString, DbContext context)
        {
            mapper = new AbstractMapper(connectionString);
            mapper.isStronglyTyped = false;
            mapper.RegisterDbContext(context);
        }
        public T Get<T>(Expression<Func<T, bool>> expression) where T:new()
        {
            string where = translator.TranslateWhere(expression);
            return mapper.Find<T>(where);
        }
        public IEnumerable<T> GetList<T>(Expression<Func<T, bool>> expression) where T : new()
        {
            string where = translator.TranslateWhere(expression);
            return mapper.FinAll<T>(where);
        }

        public PageList<T> GetPageList<T>(Expression<Func<T, bool>> expression, int pageIndex, int pageSize) where T : new()
        {
            string where = translator.TranslateWhere(expression);
            int total = 0;
            List<T> list = mapper.FindPageList<T>(where, pageIndex, pageSize, out total).ToList();
            return new PageList<T>() { pageIndex = pageIndex, pageSize = pageSize, rowCount = total, Items = list };
        }

        public int Update<T>(T model, Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            return mapper.Update<T>(model, where);
        }
        public int UpdatePart<T>(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            string update = translator.TranslateUpdate(updateExpression);
            return mapper.Excute(update + where);
        }
        public int Insert<T>(T model)
        {
            return mapper.Insert<T>(model);
        }

        public int Detele<T>(Expression<Func<T, bool>> expression)
        {
            string where = translator.TranslateWhere(expression);
            return mapper.Delete<T>(where);
        }
        public TResult QuerySingle<TResult>(string sql, object paramters)
        {
            return mapper.QuerySingle<TResult>(sql, paramters);
        }
        public IEnumerable<TResult> Query<TResult>(string sql, object paramters) where TResult : new()
        {
            return mapper.Query<TResult>(sql, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public TReturn Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> act, object paramters = null) where TReturn : new()
        {
            return mapper.Query<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(sql, act, paramters);
        }
        public QueryReader QueryMultiple(string sql, object paramters)
        {
            return mapper.QueryMultiple(sql, paramters);
        }
        public List<TResult> QueryReflect<TResult>(string sql, object paramters) where TResult : new()
        {
            return mapper.QueryReflect<TResult>(sql, paramters);
        }
    }
}
