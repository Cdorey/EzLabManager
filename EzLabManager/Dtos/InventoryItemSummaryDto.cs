namespace EzLabManager.Dtos;

/// <summary>
/// 表示按耗材汇总后的库存信息。
/// </summary>
/// <remarks>
/// 该 DTO 由耗材基础信息、入库记录和出库记录汇总计算得到。
/// 它不是 EF Core 实体，不直接表示数据库表。
/// </remarks>
public class InventoryItemSummaryDto
{
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
    /// 累计入库数量。
    /// </summary>
    public int InboundQuantity { get; set; }

    /// <summary>
    /// 累计出库数量。
    /// </summary>
    public int OutboundQuantity { get; set; }

    /// <summary>
    /// 当前剩余库存数量。
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// 当前有效批次数。
    /// </summary>
    public int AvailableBatchCount { get; set; }
}