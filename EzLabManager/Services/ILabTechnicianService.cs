using EzLabManager.Dtos;

namespace EzLabManager.Services;

/// <summary>
/// 定义检验师信息的业务服务。
/// </summary>
/// <remarks>
/// 该接口用于 ViewModel 和检验师数据访问实现之间解耦。
/// </remarks>
public interface ILabTechnicianService
{
    /// <summary>
    /// 获取检验师列表。
    /// </summary>
    /// <param name="keyword">
    /// 可选搜索关键词。可匹配姓名或工号。
    /// </param>
    /// <param name="includeInactive">
    /// 是否包含已停用的检验师。
    /// </param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>检验师 DTO 列表。</returns>
    Task<List<LabTechnicianDto>> GetListAsync(
        string? keyword = null,
        bool includeInactive = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据主键获取单个检验师。
    /// </summary>
    /// <param name="id">检验师主键。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>
    /// 若找到对应检验师，则返回检验师 DTO；否则返回 null。
    /// </returns>
    Task<LabTechnicianDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建新的检验师。
    /// </summary>
    /// <param name="dto">待创建的检验师信息。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    /// <returns>创建后的检验师信息。</returns>
    Task<LabTechnicianDto> CreateAsync(
        LabTechnicianDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新已有检验师信息。
    /// </summary>
    /// <param name="dto">待更新的检验师信息。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    Task UpdateAsync(
        LabTechnicianDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除或停用指定检验师。
    /// </summary>
    /// <remarks>
    /// 如果检验师没有被任何入库或出库记录引用，则物理删除。
    /// 如果已经存在历史业务记录，则不删除历史引用，而是将其设置为无效。
    /// </remarks>
    /// <param name="id">检验师主键。</param>
    /// <param name="cancellationToken">异步操作取消令牌。</param>
    Task DeleteOrDeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);
}