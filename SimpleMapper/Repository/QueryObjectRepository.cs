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
        private readonly IQueryStrategy strategy;
        public QueryObjectRepository() {
            strategy = new QueryObjectStrategy(typeof(T));
        }
        public T Equal(Condition condition)
        {
            string where = GetWhere(condition);
            return AbstractMapper<T>.Find(where);
        }
        public T Equal(List<Condition> conditions) {
            string where = GetWhere(conditions);
            return AbstractMapper<T>.Find(where);
        }
        public int Update(T model, Condition condition)
        {
            string where = GetWhere(condition);
            return AbstractMapper<T>.Update(model, where);
        }

        public int Insert(T model)
        {
            return AbstractMapper<T>.Insert(model);
        }

        public int Detele(Condition condition)
        {
            string where = GetWhere(condition);
            return AbstractMapper<T>.Delete(where);
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
