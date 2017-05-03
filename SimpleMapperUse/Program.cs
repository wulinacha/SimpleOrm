using SimpleMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapperUse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExpressionRepository<tb_User> repository = new ExpressionRepository<tb_User>();
            //新增
            var user = new tb_User() { mobile = "15989027156", name = "bosco", password = "123456", sex = 1 };
            repository.Insert(user);
            //查找
            string username = "bosco";
            tb_User user0 = repository.Get(e => e.name == username);
            Console.WriteLine(user0.mobile);
            //更新
            user0.mobile = "15989027155";
            repository.Update(user0, e => e.name == "bosco");
            Console.WriteLine(user0.mobile);
            //删除
            repository.Detele(e => e.name == "bosco");
            user0 = repository.Get(e => e.mobile == username);
            Console.WriteLine(user0.name);
            Console.Read();
        }
    }
}
