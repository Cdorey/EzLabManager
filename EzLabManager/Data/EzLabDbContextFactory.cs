using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EzLabManager.Data;

/// <summary>
/// EzLabManager 数据库上下文的设计期工厂。
/// </summary>
/// <remarks>
/// 该工厂主要供 EF Core 设计期工具使用，例如 Add-Migration 和 Update-Database。
/// 它不是业务代码中日常增删改查的主要入口。
/// 
/// 在 WPF 项目中，EF Core 工具不一定能像 ASP.NET Core 项目那样
/// 自动从标准 Host 或 Program.cs 中解析 DbContext。
/// 因此显式提供设计期工厂，可以让迁移命令稳定地创建
/// <see cref="EzLabDbContext"/>。
/// </remarks>
public class EzLabDbContextFactory : IDesignTimeDbContextFactory<EzLabDbContext>
{
    /// <summary>
    /// 创建设计期使用的 <see cref="EzLabDbContext"/> 实例。
    /// </summary>
    /// <param name="args">
    /// 由 EF Core 设计期工具传入的命令行参数。
    /// 当前项目暂未使用该参数。
    /// </param>
    /// <returns>
    /// 配置好 SQLite Provider 和连接字符串的
    /// <see cref="EzLabDbContext"/> 实例。
    /// </returns>
    public EzLabDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EzLabDbContext>()
            .UseSqlite(DatabasePath.GetConnectionString())
            .Options;

        return new EzLabDbContext(options);
    }
}