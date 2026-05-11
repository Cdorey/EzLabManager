using EzLabManager.Data;
using EzLabManager.Dtos;
using EzLabManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EzLabManager.Services;

/// <summary>
/// 耗材出库记录业务服务。
/// </summary>
/// <remarks>
/// 该服务负责查询可用库存批次、查询最近出库记录，以及创建新的出库记录。
/// 创建出库记录时会校验出库数量不能超过当前批次剩余库存。
/// </remarks>
public class ConsumableOutboundRecordService : IConsumableOutboundRecordService
{
    private readonly IDbContextFactory<EzLabDbContext> _dbContextFactory;

    /// <summary>
    /// 初始化 <see cref="ConsumableOutboundRecordService"/> 类的新实例。
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂。</param>
    public ConsumableOutboundRecordService(
        IDbContextFactory<EzLabDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<List<InventoryBatchDto>> GetAvailableBatchesAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var query = dbContext.ConsumableInboundRecords
            .AsNoTracking()
            .Select(x => new
            {
                InboundRecordId = x.Id,
                x.ConsumableItemId,
                x.ConsumableItem.CategoryName,
                x.ConsumableItem.ModelName,
                x.BatchNumber,
                x.ExpirationDate,
                x.InboundDate,
                InboundQuantity = x.Quantity,
                OutboundQuantity = x.OutboundRecords
                    .Sum(y => (int?)y.Quantity) ?? 0
            })
            .Select(x => new InventoryBatchDto
            {
                InboundRecordId = x.InboundRecordId,
                ConsumableItemId = x.ConsumableItemId,
                CategoryName = x.CategoryName,
                ModelName = x.ModelName,
                BatchNumber = x.BatchNumber,
                ExpirationDate = x.ExpirationDate,
                InboundDate = x.InboundDate,
                InboundQuantity = x.InboundQuantity,
                OutboundQuantity = x.OutboundQuantity,
                RemainingQuantity = x.InboundQuantity - x.OutboundQuantity
            })
            .Where(x => x.RemainingQuantity > 0)
            .AsQueryable();

        keyword = keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.CategoryName.Contains(keyword) ||
                x.ModelName.Contains(keyword) ||
                x.BatchNumber.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.ExpirationDate ?? DateTime.MaxValue)
            .ThenBy(x => x.CategoryName)
            .ThenBy(x => x.ModelName)
            .ThenBy(x => x.BatchNumber)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ConsumableOutboundRecordDto>> GetRecentListAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            take = 100;
        }

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        return await dbContext.ConsumableOutboundRecords
            .AsNoTracking()
            .OrderByDescending(x => x.OutboundDate)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .Select(x => new ConsumableOutboundRecordDto
            {
                Id = x.Id,
                InboundRecordId = x.InboundRecordId,
                ConsumableItemId = x.InboundRecord.ConsumableItemId,
                CategoryName = x.InboundRecord.ConsumableItem.CategoryName,
                ModelName = x.InboundRecord.ConsumableItem.ModelName,
                BatchNumber = x.BatchNumber,
                Quantity = x.Quantity,
                OutboundDate = x.OutboundDate,
                OutboundById = x.OutboundById,
                OutboundByName = x.OutboundBy.Name,
                OutboundByEmployeeNumber = x.OutboundBy.EmployeeNumber
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConsumableOutboundRecordDto> CreateAsync(
        ConsumableOutboundRecordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.InboundRecordId <= 0)
        {
            throw new ArgumentException("请选择需要出库的库存批次。", nameof(dto));
        }

        if (dto.OutboundById <= 0)
        {
            throw new ArgumentException("请选择出账人。", nameof(dto));
        }

        if (dto.Quantity <= 0)
        {
            throw new ArgumentException("出库数量必须大于 0。", nameof(dto));
        }

        if (dto.OutboundDate == default)
        {
            throw new ArgumentException("出账日期无效。", nameof(dto));
        }

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var batch = await dbContext.ConsumableInboundRecords
            .Where(x => x.Id == dto.InboundRecordId)
            .Select(x => new
            {
                InboundRecordId = x.Id,
                x.ConsumableItemId,
                x.ConsumableItem.CategoryName,
                x.ConsumableItem.ModelName,
                x.BatchNumber,
                InboundQuantity = x.Quantity,
                OutboundQuantity = x.OutboundRecords
                    .Sum(y => (int?)y.Quantity) ?? 0
            })
            .Select(x => new
            {
                x.InboundRecordId,
                x.ConsumableItemId,
                x.CategoryName,
                x.ModelName,
                x.BatchNumber,
                x.InboundQuantity,
                x.OutboundQuantity,
                RemainingQuantity = x.InboundQuantity - x.OutboundQuantity
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (batch is null)
        {
            throw new InvalidOperationException("未找到需要出库的库存批次。");
        }

        if (batch.RemainingQuantity <= 0)
        {
            throw new InvalidOperationException("该批次已经没有剩余库存。");
        }

        if (dto.Quantity > batch.RemainingQuantity)
        {
            throw new InvalidOperationException(
                $"出库数量不能超过当前剩余库存。当前剩余：{batch.RemainingQuantity}。");
        }

        var outboundBy = await dbContext.LabTechnicians
            .AsNoTracking()
            .Where(x => x.Id == dto.OutboundById)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.EmployeeNumber,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (outboundBy is null)
        {
            throw new InvalidOperationException("未找到出账人。");
        }

        if (!outboundBy.IsActive)
        {
            throw new InvalidOperationException("当前出账人已被设置为无效，不能继续出库。");
        }

        var entity = new ConsumableOutboundRecord
        {
            InboundRecordId = batch.InboundRecordId,
            BatchNumber = batch.BatchNumber,
            Quantity = dto.Quantity,
            OutboundDate = dto.OutboundDate,
            OutboundById = outboundBy.Id
        };

        dbContext.ConsumableOutboundRecords.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new ConsumableOutboundRecordDto
        {
            Id = entity.Id,
            InboundRecordId = batch.InboundRecordId,
            ConsumableItemId = batch.ConsumableItemId,
            CategoryName = batch.CategoryName,
            ModelName = batch.ModelName,
            BatchNumber = batch.BatchNumber,
            Quantity = entity.Quantity,
            OutboundDate = entity.OutboundDate,
            OutboundById = outboundBy.Id,
            OutboundByName = outboundBy.Name,
            OutboundByEmployeeNumber = outboundBy.EmployeeNumber
        };
    }
}