using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public static class NullExtenssion
    {
        public static bool IsNullOrSpace(this object _object){
            if (_object == null)
                return true;
            if (_object.ToString().Trim() == "")
                return true;
            return false;
        }
    }
}
