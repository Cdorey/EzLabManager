namespace EzLabManager.Models;

/// <summary>
/// 表示实验室耗材的一条出库记录。
/// </summary>
/// <remarks>
/// 出库记录建议绑定到具体的入库记录，而不是仅通过批号识别。
/// 因为批号可能不是全局唯一值，同一批号也可能出现在不同耗材中。
/// </remarks>
public class ConsumableOutboundRecord
{
    /// <summary>
    /// 出库记录主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 对应的入库记录主键。
    /// </summary>
    /// <remarks>
    /// 通过该字段可以确定本次出库来自哪一种耗材、哪一个批号和哪一次入库。
    /// </remarks>
    public int InboundRecordId { get; set; }

    /// <summary>
    /// 对应的入库记录。
    /// </summary>
    public ConsumableInboundRecord InboundRecord { get; set; } = null!;

    /// <summary>
    /// 出库批号快照。
    /// </summary>
    /// <remarks>
    /// 该字段主要用于查询、导出和审计。
    /// 正常情况下，该字段应与 <see cref="InboundRecord"/> 中的批号一致。
    /// </remarks>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// 出库数量。
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 出账日期。
    /// </summary>
    public DateTime OutboundDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 出账人主键。
    /// </summary>
    public int OutboundById { get; set; }

    /// <summary>
    /// 出账人。
    /// </summary>
    public LabTechnician OutboundBy { get; set; } = null!;
}