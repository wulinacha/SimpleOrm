using SimpleMapper.QueryStrategy;
using SimpleMapper.Rpository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class QueryObjectSimpleClient<T> where T : new()
    {
        private AbstractMapper mapper;
        public QueryObjectSimpleClient(string connectionString)
        {
            mapper = new AbstractMapper(connectionString);
            strategy = new QueryObjectStrategy(typeof(T));
        }
        private readonly IQueryStrategy strategy;
        public T Get(Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.Find<T>(where);
        }
        public IEnumerable<T> GetList(Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.FinAll<T>(where);
        }
        public T Get(List<Condition> conditions) {
            string where = GetWhere(conditions);
            return mapper.Find<T>(where);
        }
        public int Update(T model, Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.Update<T>(model, where);
        }

        public int Insert(T model)
        {
            return mapper.Insert<T>(model);
        }

        public int Detele(Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.Delete<T>(where);
        }

        public string GetWhere(Condition condition)
        {
            return strategy.Equal(condition).Excute();
        }
        public string GetWhere(List<Condition> conditions)
        {
            return strategy.Equal(conditions).Excute();
        }
    }
}
