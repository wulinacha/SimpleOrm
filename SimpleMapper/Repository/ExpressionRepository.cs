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
    public class ExpressionRepository<T> where T:new()
    {
        private AbstractMapper<T> mapper;
        private string connectionString;
        private QueryTranslator translator;
        public ExpressionRepository(string connectionString) {
            this.connectionString = connectionString;
            mapper = new AbstractMapper<T>(connectionString);
        }
        public T Get(Expression<Func<T, bool>> expression)
        {
            string where = GetWhere(expression);
            return mapper.Find(where);
        }
        public List<T> GetList(Expression<Func<T, bool>> expression)
        {
            string where = GetWhere(expression);
            return mapper.FinAll(where);
        }

        public PageList<T> GetPageList(Expression<Func<T, bool>> expression, int pageIndex, int pageSize)
        {
            string where = GetWhere(expression);
            int total = 0;
            List<T> list = mapper.FindPageList(where, pageIndex, pageSize, out total);
            return new PageList<T>() { pageIndex=pageIndex,pageSize=pageSize, rowCount = total, Items = list };
        }

        public int Update(T model, Expression<Func<T, bool>> expression)
        {
            string where = GetWhere(expression);
            return mapper.Update(model, where);
        }

        public int Insert(T model) {
            return mapper.Insert(model);
        }

        public int Detele(Expression<Func<T, bool>> expression)
        {
            string where = GetWhere(expression);
            return mapper.Delete(where);
        }

        public List<TResult> Query<TResult>(string sql, object paramters)where TResult:new()
        {
            return mapper.Query<TResult>(sql, paramters);
        }

        public string GetWhere(Expression<Func<T, bool>> expression)
        {
            if (translator.IsNullOrSpace())
                translator = new QueryTranslator();
            return translator.TranslateWhere(expression);
        }
        public void BeginTransaction(IsolationLevel level = IsolationLevel.Unspecified)
        {
            DbContext context = new DbContext(this.connectionString);
            mapper.RegisterDbContext(context);
            mapper.context.CreateTransaction(level);
        }
        public void CommitTransaction() {
            mapper.context.CommitTransaction();
        }
    }
}
