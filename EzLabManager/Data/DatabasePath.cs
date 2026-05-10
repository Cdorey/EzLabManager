using System.IO;

namespace EzLabManager.Data;

/// <summary>
/// 提供 EzLabManager 数据库文件路径和连接字符串。
/// </summary>
/// <remarks>
/// 当前数据库文件存放在当前 Windows 用户的“我的文档”目录下。
/// 实际路径结构为：Documents\EzSuite\EzLabManager.db。
/// </remarks>
public static class DatabasePath
{
    /// <summary>
    /// EzSuite 套件目录名称。
    /// </summary>
    private const string SuiteDirectoryName = "EzSuite";

    /// <summary>
    /// SQLite 数据库文件名称。
    /// </summary>
    private const string DatabaseFileName = "EzLabManager.db";

    /// <summary>
    /// 获取 EzSuite 套件目录的完整路径。
    /// </summary>
    /// <returns>
    /// 当前用户“我的文档”目录下的 EzSuite 文件夹路径。
    /// </returns>
    public static string GetSuiteDirectoryPath()
    {
        var documentsPath = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);

        return Path.Combine(documentsPath, SuiteDirectoryName);
    }

    /// <summary>
    /// 获取 EzLabManager SQLite 数据库文件的完整路径。
    /// </summary>
    /// <remarks>
    /// 如果 EzSuite 目录不存在，该方法会自动创建目录。
    /// </remarks>
    /// <returns>
    /// SQLite 数据库文件的完整路径。
    /// </returns>
    public static string GetDatabaseFilePath()
    {
        var suiteDirectoryPath = GetSuiteDirectoryPath();

        Directory.CreateDirectory(suiteDirectoryPath);

        return Path.Combine(suiteDirectoryPath, DatabaseFileName);
    }

    /// <summary>
    /// 获取 EF Core SQLite 数据库连接字符串。
    /// </summary>
    /// <returns>
    /// 可传入 UseSqlite 方法的 SQLite 连接字符串。
    /// </returns>
    public static string GetConnectionString()
    {
        var databaseFilePath = GetDatabaseFilePath();

        return $"Data Source={databaseFilePath}";
    }
}