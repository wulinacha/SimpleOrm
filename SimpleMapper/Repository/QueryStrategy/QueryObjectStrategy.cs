using SimpleMapper;
using SimpleMapper.Rpository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.QueryStrategy
{
    public class QueryObjectStrategy : IQueryStrategy
    {
        public Type className;
        public List<Condition> conditions = new List<Condition>();
        public QueryObjectStrategy(Type classname) { 
            this.className=classname;
        }
        public IQueryStrategy Equal(Condition condition)
        {
            conditions.Add(new Condition() { field = condition.field, operarorsign = "=", value = condition.value });
            return this;
        }

        public IQueryStrategy Equal(List<Condition> conditions)
        {
            conditions.AddRange(conditions);
            return this;
        }

        public string Excute() {
            StringBuilder where = new StringBuilder();
            foreach (var item in conditions) {
                where.Append(item.GetWhere(IsString(item.field)));
            }
            return where.ToString();
        }

        public bool IsString(string fileName) {
            DataMap dm=Metadata.GetDataMap(className);
            ColumnMap cm= dm.GetColumnMapByFileName(fileName);
            return cm.type == typeof(string)||cm.type==typeof(DateTime);
        }
    }
}
