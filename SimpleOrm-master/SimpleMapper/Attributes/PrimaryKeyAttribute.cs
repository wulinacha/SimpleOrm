using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute:Attribute
    {
        public string fieldName { get; set; }
        public bool autoIncrement { get; set; }

        public PrimaryKeyAttribute(string fieldName, bool autoIncrement = true)
        {
            this.fieldName = fieldName;
            this.autoIncrement = autoIncrement;
        }
    }
}
