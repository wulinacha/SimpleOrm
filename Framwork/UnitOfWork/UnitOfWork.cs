using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framwork.UnitOfWork
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public class UnitOfWork
    {
        private static Hashtable ht = new Hashtable();

        public object GetMap(string name) { 
            return ht[name];
        }

        public object SetMap(string name) {
            return ht[name];
        }
    }
}
