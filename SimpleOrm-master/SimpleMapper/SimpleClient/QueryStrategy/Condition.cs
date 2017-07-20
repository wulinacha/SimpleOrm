using SimpleMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class Condition
    {
        public string field;
        public string value;
        public string operarorsign;

        public void SetCondition(string field, string value, string operarorsign) {
            this.field = field;
            this.value = value;
            this.operarorsign = operarorsign;
        }

        public string GetWhere(bool isString){
            return this.field + this.operarorsign + (isString ? StringFormat(this.value) : this.value);
        }
        public string StringFormat(string value) {
            return "'" + value + "'";
        }

    }
}
