using SimpleMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapperUse
{
    public class tb_User
    {
        public string name { get; set; }
        public string mobile { get; set; }
        public string password { get; set; }
        public int sex { get; set; }
        [PrimaryKey("Id")]
        public int id { get; set; }
        public bool isDelete { get; set; }
         [Column("RoleID")]
        public int roleid { get; set; }
    }
    public class tb_UserX
    {
        [PrimaryKey("Id")]
        public int id { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string password { get; set; }
        public int sex { get; set; }
        public bool isDelete { get; set; }
        public string RoleName { get; set; }
    }
    [Table("tb_User")]
    public class UserInfo
    {
        public string name { get; set; }
        public int sex { get; set; }
        [Column("mobile")]
        public string phone { get; set; }
    }
}
