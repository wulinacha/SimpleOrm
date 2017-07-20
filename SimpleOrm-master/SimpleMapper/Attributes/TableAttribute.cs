using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
     [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute:Attribute
    {
         public TableAttribute(string tableName) {
             this.tableName = tableName;
         }

         public string tableName { get; set; }
    }
}
