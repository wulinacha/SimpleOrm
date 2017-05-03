using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Framwork;

namespace SimpleMapper
{
    public static class Metadata
    {
        private static Hashtable maps;
        static Metadata() {
            maps = new Hashtable();
        }
        public static void Add(Type t){
            System.Reflection.PropertyInfo[] properties = t.GetProperties();
            DataMap map = new DataMap(t.Name, t.Name);
            foreach (System.Reflection.PropertyInfo info in properties)
            {
                map.SetColumnMap(info.Name, info.Name, info.PropertyType);
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
