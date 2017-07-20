using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class ColumnAttribute:Attribute
    {
        public string columnName { get; set; }
        public string fieldName { get; set; }
        public ColumnAttribute(string columnName)
        {
            this.columnName = columnName;
        }
    }
}
