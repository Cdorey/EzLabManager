namespace EzLabManager.Dtos;

/// <summary>
/// 耗材出库记录的数据传输对象。
/// </summary>
/// <remarks>
/// 该 DTO 用于在服务层和 ViewModel 之间传递出库记录信息。
/// 它不是 EF Core 实体，不直接表示数据库表。
/// </remarks>
public class ConsumableOutboundRecordDto
{
    /// <summary>
    /// 出库记录主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 对应的入库记录主键。
    /// </summary>
    public int InboundRecordId { get; set; }

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
    /// 出库批号。
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// 出库数量。
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 出账日期。
    /// </summary>
    public DateTime OutboundDate { get; set; }

    /// <summary>
    /// 出账人主键。
    /// </summary>
    public int OutboundById { get; set; }

    /// <summary>
    /// 出账人姓名。
    /// </summary>
    public string OutboundByName { get; set; } = string.Empty;

    /// <summary>
    /// 出账人工号。
    /// </summary>
    public string OutboundByEmployeeNumber { get; set; } = string.Empty;
}