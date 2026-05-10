namespace EzLabManager.Models;

/// <summary>
/// 表示实验室检验师或耗材操作人员。
/// </summary>
/// <remarks>
/// 该实体用于记录入库人和出库人。
/// 当前不包含登录名、密码或权限信息，仅作为人员基础信息表使用。
/// </remarks>
public class LabTechnician
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
    /// 表示该检验师是否仍然有效。
    /// </summary>
    /// <remarks>
    /// 若人员离职或不再参与耗材管理，可将该字段设置为 false。
    /// 历史入库和出库记录仍然保留对该人员的引用。
    /// </remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 该检验师经手的所有入库记录。
    /// </summary>
    public ICollection<ConsumableInboundRecord> InboundRecords { get; set; }
        = new List<ConsumableInboundRecord>();

    /// <summary>
    /// 该检验师经手的所有出库记录。
    /// </summary>
    public ICollection<ConsumableOutboundRecord> OutboundRecords { get; set; }
        = new List<ConsumableOutboundRecord>();
}
