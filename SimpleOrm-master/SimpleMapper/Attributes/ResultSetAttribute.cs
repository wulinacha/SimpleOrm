using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Attributes
{
    public class ResultSetAttribute
    {
        public string tableName { get; set; }
        public ResultSetAttribute(string tableName) {
            this.tableName = tableName;
        }
    }
}
