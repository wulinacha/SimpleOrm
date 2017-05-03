using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class DataMap
    {
        public DataMap(string _tableName, string _className)
        {
            this.tableName = _tableName;
            this.className = _className;
        }
        public string tableName { get; set; }
        public string className { get; set; }

        private List<ColumnMap> list = new List<ColumnMap>();

        public void SetColumnMap(string columnName, string fileName, Type type)
        {
            list.Add(new ColumnMap(columnName, fileName, type));
        }
        public string GetTableName() {
            return tableName;
        }
        public string GetClassName() {
            return className;
        }
        public string GetColumnMapListStr() {
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append(item.fileName + ",");
            }
            return sb.ToString().Substring(0, sb.Length-1);
        }
        public List<ColumnMap> GetColumnList() {
            return list;
        }

        public ColumnMap GetColumnMapByFileName(string fileName)
        {
            return list.Where(e => e.fileName == fileName).FirstOrDefault();
        }
    }
}
