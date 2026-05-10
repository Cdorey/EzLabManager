using EzLabManager.Dtos;

namespace EzLabManager.Services;

/// <summary>
/// 定义耗材基础信息的业务服务。
/// </summary>
/// <remarks>
/// 该接口用于 ViewModel 和具体数据访问实现之间解耦。
/// ViewModel 只依赖该接口，不需要关心底层是 EF Core、SQLite，还是将来其他数据来源。
/// </remarks>
public interface IConsumableItemService
{
    /// <summary>
    /// 获取耗材基础信息列表。
    /// </summary>
    /// <param name="keyword">
    /// 可选搜索关键词。可匹配耗材类目或耗材型号。
    /// </param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>耗材基础信息 DTO 列表。</returns>
    Task<List<ConsumableItemDto>> GetListAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据主键获取单个耗材基础信息。
    /// </summary>
    /// <param name="id">耗材主键。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>
    /// 若找到对应耗材，则返回耗材 DTO；否则返回 null。
    /// </returns>
    Task<ConsumableItemDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建新的耗材基础信息。
    /// </summary>
    /// <param name="dto">待创建的耗材基础信息。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>创建后的耗材基础信息。</returns>
    Task<ConsumableItemDto> CreateAsync(
        ConsumableItemDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新已有耗材基础信息。
    /// </summary>
    /// <param name="dto">待更新的耗材基础信息。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    Task UpdateAsync(
        ConsumableItemDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定耗材基础信息。
    /// </summary>
    /// <param name="id">耗材主键。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}