using SimpleMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapperUse.Entity
{
    public class User
    {
        [PrimaryKey("Id")]
        public int ID { get; set; }
        public int UserID { get; set; }
        public DateTime CreateTime { get; set; }
        public string Name { get; set; }
        public int IsSendMessage { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
    }
}
