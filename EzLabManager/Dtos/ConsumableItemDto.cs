namespace EzLabManager.Dtos;

/// <summary>
/// 耗材基础信息的数据传输对象。
/// </summary>
/// <remarks>
/// 该 DTO 用于在服务层和 ViewModel 之间传递耗材基础信息。
/// 它不是 EF Core 实体，不直接表示数据库表。
/// </remarks>
public class ConsumableItemDto
{
    /// <summary>
    /// 耗材主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 耗材类目。
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 耗材型号或规格。
    /// </summary>
    public string ModelName { get; set; } = string.Empty;
}