namespace EzLabManager.Models;

/// <summary>
/// 表示实验室耗材的基础信息。
/// </summary>
/// <remarks>
/// 该实体只保存耗材的静态主数据，例如耗材类目和型号。
/// 实际库存数量不建议直接存储在该表中，而应通过入库记录和出库记录计算得到。
/// </remarks>
public class ConsumableItem
{
    /// <summary>
    /// 耗材主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 耗材类目。
    /// </summary>
    /// <example>
    /// 采血管、枪头、离心管、试剂盒。
    /// </example>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 耗材型号或规格。
    /// </summary>
    /// <example>
    /// 10μL、200μL、1.5mL、96T。
    /// </example>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// 该耗材对应的所有入库记录。
    /// </summary>
    public ICollection<ConsumableInboundRecord> InboundRecords { get; set; }
        = new List<ConsumableInboundRecord>();
}