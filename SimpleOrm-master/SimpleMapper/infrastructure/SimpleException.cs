using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class SimpleException:Exception
    {
        public SimpleException(string Message):base(Message) { }
    }
}
