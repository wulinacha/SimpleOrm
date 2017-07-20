=====================================================<br/>
# SimpleOrm使用方法
第一步获取连接;第二步注册全局数据提供者，可以注册多个数据库提供者，可以在需要使用不同的数据库来完成任务，但必须利用已注册的连接字符串实例化不同的DB上下文，此部分最好放在全局中;第三步创建DB上下文，不同数据库对应不同DB上下文；第四步DB上下文工厂方法实例化SimpleClient客户端对象，然后就可以使用了，代码如下<br/>
```C# string connectionString = ConfigHelper.GetConnectionString("SqlConnection");
ProviderFactory.RegisterProviderFactory(connectionString, DateProvider.SqlServer);//注册工厂建议放在全局处
DbContext context = new DbContext(connectionString);
StoreSimpleClient<tb_User> client = context.CreateStoreSimpleClient<tb_User>();
var result=client.Query<tb_User>(sql, new { }).FirstOrDefault();//查询数据
```
# SimpleOrm支持强类型和无类型
创建SimpleClient对象内置封装多组操作方法，SimpleClient支持强类型和无类型，由DB上下文创建，使用强类型和无类型属于同一个DB上下文，那么可以在同一个事务中使用两个不同类型的Client，创建方法如下：
``` C#
var nonClient = context.CreateNonSimpleClient();//无类型
StoreSimpleClient<tb_User> client = context.CreateStoreSimpleClient<tb_User>();//强类型
```
# SimpleOrm系统事务支持
系统事务的支持相当简单，只需要显式标志开始和结束即可，也可以根据需要设置事务隔离级别，默认为ReadCommitted读已提交（不可重复度），可以换成其他级别防止不可重复读、幻读；DB上下文标识事务开始和结束，中间增删改语句；
``` C#     
context.CreateTransaction();
client.Insert(new tb_User() { mobile = "15989027256", name = "bosco", password = "123456", sex = 1, roleid = 1 });
client.Update(new tb_User { mobile = "15989027255" }, e => e.name == "cxb");
context.CommitTransaction();
```
# SimpleOrm支持分页
这里的分页返回类型为PageList，里面包含PageIndex页面、PageSize页大小、rowCount数据总数、Itemes数据列表（Item是List<TResult>类型）,满足一般的需要；
``` C#
 int pageindex = 1;
 int pagesize = 100;
 var nonPagelist = nonClient.GetPageList<UserInfo>(e =>true, pageindex, pagesize);
 var pagelist = client.GetPageList(e => true, pageindex, pagesize);
```
# SimpleOrm支持动态SQL查询操作
支持动态SQL，支持匿名参数，返回的是List<TResult>类型，如果想返回单条数据，可以使用QueryFirst方法，将返回类型T，使用QuerySingle方法，就算结果集有多条也只返回一条。</p>
``` C#
var result = client.Query<tb_User>("select * from tb_User", new { });
var resultSingle = client.QuerySingle<tb_User>("select * from tb_User", new { });
```
# SimpleOrm支持DDD聚合
DDD中划分聚合和聚合根之后，这里支持动态SQL执行返回结果集后，自定义将聚合关联到聚合根，假设角色是聚合根，用户是它的聚合，当然现实中是不可能的，这里只是假设，方法如下：
``` C#
tb_Role roles=new tb_Role();
client.Query<tb_Role, tb_UserX, tb_Role>("select * from tb_User u left join tb_Role r on u.roleid=r.rid", (tb_Role, tb_UserX) =>
            {
                if (tb_Role.rid != roles.rid) roles = tb_Role;
                roles.UserList.Add(tb_UserX);
                return roles;
            });
```
# SimpleOrm支持多结果集
使用QueryMultiple方法，返回多结果集对象QueryReader，通过QueryReader对象可以获取多个结果集，免去多次执行查询方法的麻烦，代码如下：
``` C#
QueryReader reader=client.QueryMultiple("select * from tb_User;select * from tb_Role");
List<tb_User> ReadUserList = reader.ReadList<tb_User>().ToList();
List<tb_Role> ReadRoleList = reader.ReadList<tb_Role>().ToList();
```
# 发展方向，下一版本将支持动态类型和lingq方法扩展、支持.net core

