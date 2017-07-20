using SimpleMapper;
using SimpleMapper.Rpository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            where.Append("where ");
            foreach (var item in conditions) {
                where.Append(item.GetWhere(IsString(item.field)));
            }
            return where.ToString();
        }

        public bool IsString(string fileName) {
            DataMap dm=Metadata.GetDataMap(className);
            FieldInfo cm = dm.GetColumnMapByFileName(fileName);
            return cm.FieldType == typeof(string) || cm.FieldType == typeof(DateTime);
        }
    }
}
