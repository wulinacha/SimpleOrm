using SimpleMapper.QueryStrategy;
using SimpleMapper.Rpository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class QueryObjectRepository<T> where T : new()
    {
        private AbstractMapper<T> mapper;
        public QueryObjectRepository(string connectionString)
        {
            mapper = new AbstractMapper<T>(connectionString);
            strategy = new QueryObjectStrategy(typeof(T));
        }
        private readonly IQueryStrategy strategy;
        public T Get(Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.Find(where);
        }
        public List<T> GetList(Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.FinAll(where);
        }
        public T Get(List<Condition> conditions) {
            string where = GetWhere(conditions);
            return mapper.Find(where);
        }
        public int Update(T model, Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.Update(model, where);
        }

        public int Insert(T model)
        {
            return mapper.Insert(model);
        }

        public int Detele(Condition condition)
        {
            string where = GetWhere(condition);
            return mapper.Delete(where);
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
