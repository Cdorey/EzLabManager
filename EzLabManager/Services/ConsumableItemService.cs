using EzLabManager.Data;
using EzLabManager.Dtos;
using EzLabManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EzLabManager.Services;

/// <summary>
/// 耗材基础信息业务服务。
/// </summary>
/// <remarks>
/// 该服务通过 <see cref="IDbContextFactory{TContext}"/> 创建短生命周期的
/// <see cref="EzLabDbContext"/> 实例，完成耗材基础信息的增删改查。
/// </remarks>
public class ConsumableItemService : IConsumableItemService
{
    private readonly IDbContextFactory<EzLabDbContext> _dbContextFactory;

    /// <summary>
    /// 初始化 <see cref="ConsumableItemService"/> 类的新实例。
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂。</param>
    public ConsumableItemService(
        IDbContextFactory<EzLabDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<List<ConsumableItemDto>> GetListAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var query = dbContext.ConsumableItems
            .AsNoTracking()
            .AsQueryable();

        keyword = keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.CategoryName.Contains(keyword) ||
                x.ModelName.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.CategoryName)
            .ThenBy(x => x.ModelName)
            .Select(x => new ConsumableItemDto
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                ModelName = x.ModelName
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConsumableItemDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        return await dbContext.ConsumableItems
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ConsumableItemDto
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                ModelName = x.ModelName
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConsumableItemDto> CreateAsync(
        ConsumableItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var categoryName = NormalizeRequiredText(dto.CategoryName, "耗材类目");
        var modelName = NormalizeRequiredText(dto.ModelName, "耗材型号");

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var isDuplicate = await dbContext.ConsumableItems
            .AnyAsync(
                x => x.CategoryName == categoryName &&
                     x.ModelName == modelName,
                cancellationToken);

        if (isDuplicate)
        {
            throw new InvalidOperationException(
                $"耗材“{categoryName} - {modelName}”已经存在。");
        }

        var entity = new ConsumableItem
        {
            CategoryName = categoryName,
            ModelName = modelName
        };

        dbContext.ConsumableItems.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConsumableItemDto
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            ModelName = entity.ModelName
        };
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        ConsumableItemDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Id <= 0)
        {
            throw new ArgumentException("耗材主键无效。", nameof(dto));
        }

        var categoryName = NormalizeRequiredText(dto.CategoryName, "耗材类目");
        var modelName = NormalizeRequiredText(dto.ModelName, "耗材型号");

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.ConsumableItems
            .FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("未找到需要更新的耗材。");
        }

        var isDuplicate = await dbContext.ConsumableItems
            .AnyAsync(
                x => x.Id != dto.Id &&
                     x.CategoryName == categoryName &&
                     x.ModelName == modelName,
                cancellationToken);

        if (isDuplicate)
        {
            throw new InvalidOperationException(
                $"耗材“{categoryName} - {modelName}”已经存在。");
        }

        entity.CategoryName = categoryName;
        entity.ModelName = modelName;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException("耗材主键无效。", nameof(id));
        }

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.ConsumableItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        var hasInboundRecords = await dbContext.ConsumableInboundRecords
            .AnyAsync(x => x.ConsumableItemId == id, cancellationToken);

        if (hasInboundRecords)
        {
            throw new InvalidOperationException(
                "该耗材已经存在入库记录，不能直接删除。");
        }

        dbContext.ConsumableItems.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 规范化必填文本。
    /// </summary>
    /// <param name="value">原始文本。</param>
    /// <param name="displayName">字段显示名称。</param>
    /// <returns>去除首尾空格后的文本。</returns>
    /// <exception cref="ArgumentException">
    /// 当文本为空或仅包含空白字符时抛出。
    /// </exception>
    private static string NormalizeRequiredText(
        string? value,
        string displayName)
    {
        value = value?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{displayName}不能为空。");
        }

        return value;
    }
}