using EzLabManager.Data;
using EzLabManager.Services;
using EzLabManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace EzLabManager;

/// <summary>
/// EzLabManager WPF 应用程序入口。
/// </summary>
/// <remarks>
/// 当前应用使用 .NET Generic Host 管理依赖注入、数据库上下文工厂和窗口创建过程。
/// 不再使用 App.xaml 中的 StartupUri 自动创建主窗口。
/// </remarks>
public partial class App : Application
{
    /// <summary>
    /// 当前应用程序的 Generic Host 实例。
    /// </summary>
    private IHost? _host;

    /// <summary>
    /// 应用程序启动时执行初始化逻辑。
    /// </summary>
    /// <param name="e">启动事件参数。</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Services.AddDbContextFactory<EzLabDbContext>(options =>
            {
                options.UseSqlite(DatabasePath.GetConnectionString());
            });

            builder.Services.AddTransient<IConsumableItemService, ConsumableItemService>();
            builder.Services.AddTransient<MainWindowViewModel>();
            builder.Services.AddTransient<MainWindow>();
            builder.Services.AddTransient<ILabTechnicianService, LabTechnicianService>();

            _host = builder.Build();

            await _host.StartAsync();

            await ApplyDatabaseMigrationsAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "应用程序启动失败",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
        }
    }

    /// <summary>
    /// 应用程序退出时释放 Host 资源。
    /// </summary>
    /// <param name="e">退出事件参数。</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        base.OnExit(e);
    }

    /// <summary>
    /// 应用数据库迁移。
    /// </summary>
    /// <remarks>
    /// 该方法会根据当前 EF Core 迁移记录创建或更新 SQLite 数据库结构。
    /// 如果数据库文件不存在，SQLite 会在连接时创建数据库文件。
    /// </remarks>
    private async Task ApplyDatabaseMigrationsAsync()
    {
        if (_host is null)
        {
            throw new InvalidOperationException("Host 尚未初始化。");
        }

        using var scope = _host.Services.CreateScope();

        var dbContextFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<EzLabDbContext>>();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.Database.MigrateAsync();
    }
}