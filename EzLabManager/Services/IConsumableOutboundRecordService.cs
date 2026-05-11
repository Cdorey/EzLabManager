using EzLabManager.Dtos;

namespace EzLabManager.Services;

/// <summary>
/// 定义耗材出库记录的业务服务。
/// </summary>
/// <remarks>
/// 该接口用于 ViewModel 和出库记录数据访问实现之间解耦。
/// </remarks>
public interface IConsumableOutboundRecordService
{
    /// <summary>
    /// 获取当前仍有剩余库存的入库批次。
    /// </summary>
    /// <param name="keyword">
    /// 可选搜索关键词。可匹配耗材类目、型号或批号。
    /// </param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>可出库库存批次列表。</returns>
    Task<List<InventoryBatchDto>> GetAvailableBatchesAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近的出库记录列表。
    /// </summary>
    /// <param name="take">最多返回的记录数。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>出库记录 DTO 列表。</returns>
    Task<List<ConsumableOutboundRecordDto>> GetRecentListAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建新的耗材出库记录。
    /// </summary>
    /// <param name="dto">待创建的出库记录。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>创建后的出库记录。</returns>
    Task<ConsumableOutboundRecordDto> CreateAsync(
        ConsumableOutboundRecordDto dto,
        CancellationToken cancellationToken = default);
}