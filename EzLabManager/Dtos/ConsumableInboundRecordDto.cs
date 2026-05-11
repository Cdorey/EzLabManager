namespace EzLabManager.Dtos;

/// <summary>
/// 耗材入库记录的数据传输对象。
/// </summary>
/// <remarks>
/// 该 DTO 用于在服务层和 ViewModel 之间传递入库记录信息。
/// 它不是 EF Core 实体，不直接表示数据库表。
/// </remarks>
public class ConsumableInboundRecordDto
{
    /// <summary>
    /// 入库记录主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 耗材主键。
    /// </summary>
    public int ConsumableItemId { get; set; }

    /// <summary>
    /// 耗材类目。
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 耗材型号或规格。
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// 耗材显示名称。
    /// </summary>
    public string ConsumableDisplayName => $"{CategoryName} - {ModelName}";

    /// <summary>
    /// 耗材批号。
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// 耗材效期。
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 入库数量。
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 入账日期。
    /// </summary>
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 入账人主键。
    /// </summary>
    public int InboundById { get; set; }

    /// <summary>
    /// 入账人姓名。
    /// </summary>
    public string InboundByName { get; set; } = string.Empty;

    /// <summary>
    /// 入账人工号。
    /// </summary>
    public string InboundByEmployeeNumber { get; set; } = string.Empty;
}