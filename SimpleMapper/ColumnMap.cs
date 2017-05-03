using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SimpleMapper
{
    public class ColumnMap
    {
        public ColumnMap(string _columnName, string _fileName,Type _type)
        {
            this.columnName = _columnName;
            this.fileName = _fileName;
            this.type = _type;
        }
        public string columnName { get; set; }
        public string fileName { get; set; }
        public Type type { get; set; }

    }
}
