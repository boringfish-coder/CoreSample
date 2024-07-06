using Microsoft.Extensions.DependencyInjection;
using SqlSugar;


var service = new ServiceCollection();

service.AddScoped<ISqlSugarClient>(s =>
{
    //创建数据库对象 (用法和EF Dappper一样通过new保证线程安全)
    SqlSugarClient client = new SqlSugarClient(new ConnectionConfig()
    {
        ConnectionString = "datasource=demo.db",
        DbType = DbType.Sqlite,
        IsAutoCloseConnection = true
    },
    db =>
    {

        db.Aop.OnLogExecuting = (sql, pars) =>
        {

            //获取原生SQL推荐 5.1.4.63  性能OK
            Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));

            //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
            //Console.WriteLine(UtilMethods.GetSqlString(DbType.SqlServer, sql, pars));


        };

        //注意多租户 有几个设置几个
        //db.GetConnection(i).Aop

    });
    return client;
});
service.AddScoped(typeof(SimpleClient<>));

var provider = service.BuildServiceProvider();

var client = provider.GetService<ISqlSugarClient>();
//建库
client.DbMaintenance.CreateDatabase();//达梦和Oracle不支持建库

//建表（看文档迁移）
client.CodeFirst.InitTables<Student>(); //所有库都支持     

if (false)
{
    //查询表的所有
    var list = client.Queryable<Student>().ToList();

    //插入
    client.Insertable(new Student() { SchoolId = 1, Name = "jack" }).ExecuteCommand();

    //更新
    client.Updateable(new Student() { Id = 1, SchoolId = 2, Name = "jack2" }).ExecuteCommand();

    //删除
    client.Deleteable<Student>().Where(it => it.Id == 1).ExecuteCommand();
}
if (false)
{
    try
    {
        using (var uow = client.CreateContext())
        {
            //使用仓储查询
            var repo = client.GetSimpleClient<Student>();
            var rets = repo.AsQueryable().Where(it => it.Name == "jack2").ToList();
            Console.WriteLine("执行前：" + rets.Count);

            repo.Delete(rets);
            rets = repo.AsQueryable().Where(it => it.Name == "jack2").ToList();
            Console.WriteLine("执行后：" + rets.Count);

            throw new Exception("抛出异常");
        }
    }
    catch
    {
        //使用仓储查询
        var repo = client.GetSimpleClient<Student>();
        var rets = repo.AsQueryable().Where(it => it.Name == "jack2").ToList();
        Console.WriteLine("回滚后：" + rets.Count);
    }
}

{
    client.Ado.BeginTran();
    //使用仓储查询
    var repo = client.GetSimpleClient<Student>();
    var rets = repo.AsQueryable().Where(it => it.Name == "jack2").ToList();
    Console.WriteLine("执行前：" + rets.Count);
    repo.Delete(rets);
    rets = repo.AsQueryable().Where(it => it.Name == "jack2").ToList();
    Console.WriteLine("执行后：" + rets.Count);

    client = provider.GetService<ISqlSugarClient>();
    client.Ado.RollbackTran();
    //使用仓储查询
    repo = client.GetSimpleClient<Student>();
    rets = repo.AsQueryable().Where(it => it.Name == "jack2").ToList();
    Console.WriteLine("回滚后：" + rets.Count);
}


Console.ReadKey();


//实体与数据库结构一样
public class Student
{
    //数据是自增需要加上IsIdentity 
    //数据库是主键需要加上IsPrimaryKey 
    //注意：要完全和数据库一致2个属性
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public int? SchoolId { get; set; }
    public string? Name { get; set; }
}