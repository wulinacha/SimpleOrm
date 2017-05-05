using SimpleMapper;
using SimpleMapper.infrastructure;
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
            string connectionString = "Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "\\DataSource\\" + ConfigHelper.GetConnectionString("SQLiteConnection");
            //string connectionString =  ConfigHelper.GetConnectionString("SQLiteConnection");

            ///此部分最好在全局中注册，或者在IOC注册中注册
            ProviderFactory.RegisterProviderFactory(connectionString, DateProvider.Sqlite);
            
            #region 表达式方法
            ExpressionRepository<tb_User> repository = new ExpressionRepository<tb_User>(connectionString);
            //新增
            var user = new tb_User() { mobile = "15989027156", name = "bosco", password = "123456", sex = 1 };
            repository.Insert(user);
            //查找
            string username = "bosco";
            tb_User user0 = repository.Get(e => e.name == username);
            Console.WriteLine(user0.mobile);
            //列表查找
            List<tb_User> list = repository.GetList(e => e.name == username);
            foreach (var item in list)
            {
                Console.WriteLine("列表："+item.mobile);
            }
            
            //更新
            user0.mobile = "15989027155";
            repository.Update(user0, e => e.name == "bosco");
            Console.WriteLine(user0.mobile);
            //删除
            repository.Detele(e => e.name == "bosco");
            user0 = repository.Get(e => e.mobile == username);
            Console.WriteLine(user0.name);
            #endregion
            #region 查询对象方法
            QueryObjectRepository<tb_User> query = new QueryObjectRepository<tb_User>(connectionString);
            var userq = new tb_User() { mobile = "15989027156", name = "bosco", password = "123456", sex = 1 };
            query.Insert(userq);
            userq = query.Get(new Condition() { field = "mobile", operarorsign = "=", value = "15989027156" });
            Console.WriteLine(userq.mobile);
            List<Condition> conditions = new List<Condition>();
            conditions.Add(new Condition() { field = "mobile", operarorsign = "=", value = "15989027156" });
            userq = query.Get(conditions);
            Console.WriteLine(userq.mobile);
            #endregion
            Console.Read();
        }
    }
}
