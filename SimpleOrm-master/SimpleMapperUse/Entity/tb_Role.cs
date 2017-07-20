using SimpleMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapperUse
{
    public class tb_Role
    {
        public tb_Role() {
            UserList = new List<tb_UserX>();
        }
        [PrimaryKey("rid")]
        public int rid { get; set; }
        public string rname { get; set; }
        public int risDelete { get; set; }

        public List<tb_UserX> UserList { get; set; }

        public string[] test { get; set; }
    }
}
