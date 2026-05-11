using EzLabManager.Dtos;

namespace EzLabManager.Services;

/// <summary>
/// 定义库存汇总业务服务。
/// </summary>
/// <remarks>
/// 该接口用于查询按耗材汇总的库存信息，以及按批次汇总的库存明细。
/// </remarks>
public interface IInventorySummaryService
{
    /// <summary>
    /// 获取按耗材汇总的库存列表。
    /// </summary>
    /// <param name="keyword">
    /// 可选搜索关键词。可匹配耗材类目或型号。
    /// </param>
    /// <param name="includeZeroStock">
    /// 是否包含零库存耗材。
    /// </param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>按耗材汇总的库存列表。</returns>
    Task<List<InventoryItemSummaryDto>> GetItemSummariesAsync(
        string? keyword = null,
        bool includeZeroStock = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取按入库批次汇总的库存明细。
    /// </summary>
    /// <param name="keyword">
    /// 可选搜索关键词。可匹配耗材类目、型号或批号。
    /// </param>
    /// <param name="includeZeroStock">
    /// 是否包含已经出完的批次。
    /// </param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>按批次汇总的库存明细列表。</returns>
    Task<List<InventoryBatchDto>> GetBatchSummariesAsync(
        string? keyword = null,
        bool includeZeroStock = true,
        CancellationToken cancellationToken = default);
}