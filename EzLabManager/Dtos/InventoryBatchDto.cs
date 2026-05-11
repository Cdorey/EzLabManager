namespace EzLabManager.Dtos;

/// <summary>
/// 表示一个可用于出库的库存批次。
/// </summary>
/// <remarks>
/// 该 DTO 由入库记录和对应出库记录汇总计算得到。
/// 它不是 EF Core 实体，不直接表示数据库表。
/// </remarks>
public class InventoryBatchDto
{
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
    /// 批号。
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// 效期。
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 入账日期。
    /// </summary>
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 原始入库数量。
    /// </summary>
    public int InboundQuantity { get; set; }

    /// <summary>
    /// 已出库数量。
    /// </summary>
    public int OutboundQuantity { get; set; }

    /// <summary>
    /// 当前剩余数量。
    /// </summary>
    public int RemainingQuantity { get; set; }
}