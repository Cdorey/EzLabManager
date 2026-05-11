using EzLabManager.Data;
using EzLabManager.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EzLabManager.Services;

/// <summary>
/// 库存汇总业务服务。
/// </summary>
/// <remarks>
/// 该服务通过入库记录和出库记录动态计算当前库存。
/// 当前不额外维护库存快照表，以避免流水和库存数量不一致。
/// </remarks>
public class InventorySummaryService : IInventorySummaryService
{
    private readonly IDbContextFactory<EzLabDbContext> _dbContextFactory;

    /// <summary>
    /// 初始化 <see cref="InventorySummaryService"/> 类的新实例。
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂。</param>
    public InventorySummaryService(
        IDbContextFactory<EzLabDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<List<InventoryItemSummaryDto>> GetItemSummariesAsync(
        string? keyword = null,
        bool includeZeroStock = true,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var query = dbContext.ConsumableItems
            .AsNoTracking()
            .Select(x => new
            {
                ConsumableItemId = x.Id,
                x.CategoryName,
                x.ModelName,
                InboundQuantity = x.InboundRecords
                    .Sum(y => (int?)y.Quantity) ?? 0,
                OutboundQuantity = x.InboundRecords
                    .SelectMany(y => y.OutboundRecords)
                    .Sum(y => (int?)y.Quantity) ?? 0,
                AvailableBatchCount = x.InboundRecords
                    .Count(y =>
                        y.Quantity -
                        (y.OutboundRecords.Sum(z => (int?)z.Quantity) ?? 0) > 0)
            })
            .Select(x => new InventoryItemSummaryDto
            {
                ConsumableItemId = x.ConsumableItemId,
                CategoryName = x.CategoryName,
                ModelName = x.ModelName,
                InboundQuantity = x.InboundQuantity,
                OutboundQuantity = x.OutboundQuantity,
                RemainingQuantity = x.InboundQuantity - x.OutboundQuantity,
                AvailableBatchCount = x.AvailableBatchCount
            })
            .AsQueryable();

        keyword = keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.CategoryName.Contains(keyword) ||
                x.ModelName.Contains(keyword));
        }

        if (!includeZeroStock)
        {
            query = query.Where(x => x.RemainingQuantity > 0);
        }

        return await query
            .OrderBy(x => x.CategoryName)
            .ThenBy(x => x.ModelName)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<InventoryBatchDto>> GetBatchSummariesAsync(
        string? keyword = null,
        bool includeZeroStock = true,
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
            .AsQueryable();

        keyword = keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.CategoryName.Contains(keyword) ||
                x.ModelName.Contains(keyword) ||
                x.BatchNumber.Contains(keyword));
        }

        if (!includeZeroStock)
        {
            query = query.Where(x => x.RemainingQuantity > 0);
        }

        return await query
            .OrderBy(x => x.ExpirationDate == null)
            .ThenBy(x => x.ExpirationDate)
            .ThenBy(x => x.CategoryName)
            .ThenBy(x => x.ModelName)
            .ThenBy(x => x.BatchNumber)
            .ToListAsync(cancellationToken);
    }
}