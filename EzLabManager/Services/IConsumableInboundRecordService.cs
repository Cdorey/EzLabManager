using EzLabManager.Dtos;

namespace EzLabManager.Services;

/// <summary>
/// 定义耗材入库记录的业务服务。
/// </summary>
/// <remarks>
/// 该接口用于 ViewModel 和入库记录数据访问实现之间解耦。
/// </remarks>
public interface IConsumableInboundRecordService
{
    /// <summary>
    /// 获取最近的入库记录列表。
    /// </summary>
    /// <param name="take">最多返回的记录数。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>入库记录 DTO 列表。</returns>
    Task<List<ConsumableInboundRecordDto>> GetRecentListAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建新的耗材入库记录。
    /// </summary>
    /// <param name="dto">待创建的入库记录。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>创建后的入库记录。</returns>
    Task<ConsumableInboundRecordDto> CreateAsync(
        ConsumableInboundRecordDto dto,
        CancellationToken cancellationToken = default);
}