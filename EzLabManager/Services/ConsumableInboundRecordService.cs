using EzLabManager.Data;
using EzLabManager.Dtos;
using EzLabManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EzLabManager.Services;

/// <summary>
/// 耗材入库记录业务服务。
/// </summary>
/// <remarks>
/// 该服务通过 <see cref="IDbContextFactory{TContext}"/> 创建短生命周期的
/// <see cref="EzLabDbContext"/> 实例，完成入库记录的查询和新增。
/// </remarks>
public class ConsumableInboundRecordService : IConsumableInboundRecordService
{
    private readonly IDbContextFactory<EzLabDbContext> _dbContextFactory;

    /// <summary>
    /// 初始化 <see cref="ConsumableInboundRecordService"/> 类的新实例。
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂。</param>
    public ConsumableInboundRecordService(
        IDbContextFactory<EzLabDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<List<ConsumableInboundRecordDto>> GetRecentListAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            take = 100;
        }

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        return await dbContext.ConsumableInboundRecords
            .AsNoTracking()
            .OrderByDescending(x => x.InboundDate)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .Select(x => new ConsumableInboundRecordDto
            {
                Id = x.Id,
                ConsumableItemId = x.ConsumableItemId,
                CategoryName = x.ConsumableItem.CategoryName,
                ModelName = x.ConsumableItem.ModelName,
                BatchNumber = x.BatchNumber,
                ExpirationDate = x.ExpirationDate,
                Quantity = x.Quantity,
                InboundDate = x.InboundDate,
                InboundById = x.InboundById,
                InboundByName = x.InboundBy.Name,
                InboundByEmployeeNumber = x.InboundBy.EmployeeNumber
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConsumableInboundRecordDto> CreateAsync(
        ConsumableInboundRecordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.ConsumableItemId <= 0)
        {
            throw new ArgumentException("请选择需要入库的耗材。", nameof(dto));
        }

        if (dto.InboundById <= 0)
        {
            throw new ArgumentException("请选择入账人。", nameof(dto));
        }

        var batchNumber = NormalizeRequiredText(dto.BatchNumber, "批号");

        if (dto.Quantity <= 0)
        {
            throw new ArgumentException("入库数量必须大于 0。", nameof(dto));
        }

        if (dto.InboundDate == default)
        {
            throw new ArgumentException("入账日期无效。", nameof(dto));
        }

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var consumableItem = await dbContext.ConsumableItems
            .AsNoTracking()
            .Where(x => x.Id == dto.ConsumableItemId)
            .Select(x => new
            {
                x.Id,
                x.CategoryName,
                x.ModelName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (consumableItem is null)
        {
            throw new InvalidOperationException("未找到需要入库的耗材。");
        }

        var inboundBy = await dbContext.LabTechnicians
            .AsNoTracking()
            .Where(x => x.Id == dto.InboundById)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.EmployeeNumber,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (inboundBy is null)
        {
            throw new InvalidOperationException("未找到入账人。");
        }

        if (!inboundBy.IsActive)
        {
            throw new InvalidOperationException("当前入账人已被设置为无效，不能继续入库。");
        }

        var entity = new ConsumableInboundRecord
        {
            ConsumableItemId = consumableItem.Id,
            BatchNumber = batchNumber,
            ExpirationDate = dto.ExpirationDate,
            Quantity = dto.Quantity,
            InboundDate = dto.InboundDate,
            InboundById = inboundBy.Id
        };

        dbContext.ConsumableInboundRecords.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConsumableInboundRecordDto
        {
            Id = entity.Id,
            ConsumableItemId = consumableItem.Id,
            CategoryName = consumableItem.CategoryName,
            ModelName = consumableItem.ModelName,
            BatchNumber = entity.BatchNumber,
            ExpirationDate = entity.ExpirationDate,
            Quantity = entity.Quantity,
            InboundDate = entity.InboundDate,
            InboundById = inboundBy.Id,
            InboundByName = inboundBy.Name,
            InboundByEmployeeNumber = inboundBy.EmployeeNumber
        };
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