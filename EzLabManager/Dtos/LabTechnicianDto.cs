namespace EzLabManager.Dtos;

/// <summary>
/// 检验师信息的数据传输对象。
/// </summary>
/// <remarks>
/// 该 DTO 用于在服务层和 ViewModel 之间传递检验师基础信息。
/// 它不是 EF Core 实体，不直接表示数据库表。
/// </remarks>
public class LabTechnicianDto
{
    /// <summary>
    /// 检验师主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 检验师姓名。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 检验师工号。
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// 表示该检验师是否有效。
    /// </summary>
    public bool IsActive { get; set; } = true;
}