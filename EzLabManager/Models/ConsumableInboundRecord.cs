namespace EzLabManager.Models;

/// <summary>
/// 表示实验室耗材的一条入库记录。
/// </summary>
/// <remarks>
/// 每次入库均应生成一条独立记录。
/// 若同一耗材、同一批号在不同日期多次入库，也建议记录为多条入库流水。
/// </remarks>
public class ConsumableInboundRecord
{
    /// <summary>
    /// 入库记录主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 入库耗材主键。
    /// </summary>
    public int ConsumableItemId { get; set; }

    /// <summary>
    /// 入库耗材。
    /// </summary>
    public ConsumableItem ConsumableItem { get; set; } = null!;

    /// <summary>
    /// 耗材批号。
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// 耗材效期。
    /// </summary>
    /// <remarks>
    /// 对于没有明确效期的耗材，可允许该字段为空。
    /// </remarks>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 入库数量。
    /// </summary>
    /// <remarks>
    /// 当前按整数数量管理。
    /// 如果后续需要管理毫升、克等可拆分单位，可改为 decimal。
    /// </remarks>
    public int Quantity { get; set; }

    /// <summary>
    /// 入账日期。
    /// </summary>
    public DateTime InboundDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 入账人主键。
    /// </summary>
    public int InboundById { get; set; }

    /// <summary>
    /// 入账人。
    /// </summary>
    public LabTechnician InboundBy { get; set; } = null!;

    /// <summary>
    /// 该入库批次对应的所有出库记录。
    /// </summary>
    public ICollection<ConsumableOutboundRecord> OutboundRecords { get; set; }
        = new List<ConsumableOutboundRecord>();
}
