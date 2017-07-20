using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using SimpleMapper.infrastructure;

namespace SimpleMapper
{
    public static class Metadata
    {
        private static Hashtable maps;
        static Metadata() {
            maps = new Hashtable();
        }
        public static void Add(Type t){
            DataMap map=null;
            TableAttribute tableAttribute = null;
            var tableAttributeObject = t.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            if (tableAttributeObject.IsNullOrSpace())
            {
                map = new DataMap(t.Name, t.Name, t);
            }
            else
            {
                tableAttribute = (TableAttribute)tableAttributeObject;
                map = new DataMap(tableAttribute.tableName,t.Name,t);
            }
            maps.Add(t,map);
        }
        public static DataMap GetDataMap(Type t)
        {
            if (maps[t].IsNullOrSpace())
                Add(t);
            return (DataMap)maps[t];
        }
    }
}
