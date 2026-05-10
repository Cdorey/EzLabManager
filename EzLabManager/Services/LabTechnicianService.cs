using EzLabManager.Data;
using EzLabManager.Dtos;
using EzLabManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EzLabManager.Services;

/// <summary>
/// 检验师信息业务服务。
/// </summary>
/// <remarks>
/// 该服务通过 <see cref="IDbContextFactory{TContext}"/> 创建短生命周期的
/// <see cref="EzLabDbContext"/> 实例，完成检验师信息的增删改查。
/// </remarks>
public class LabTechnicianService : ILabTechnicianService
{
    private readonly IDbContextFactory<EzLabDbContext> _dbContextFactory;

    /// <summary>
    /// 初始化 <see cref="LabTechnicianService"/> 类的新实例。
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂。</param>
    public LabTechnicianService(
        IDbContextFactory<EzLabDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<List<LabTechnicianDto>> GetListAsync(
        string? keyword = null,
        bool includeInactive = true,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var query = dbContext.LabTechnicians
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        keyword = keyword?.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.EmployeeNumber.Contains(keyword));
        }

        return await query
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.EmployeeNumber)
            .Select(x => new LabTechnicianDto
            {
                Id = x.Id,
                Name = x.Name,
                EmployeeNumber = x.EmployeeNumber,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LabTechnicianDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        return await dbContext.LabTechnicians
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LabTechnicianDto
            {
                Id = x.Id,
                Name = x.Name,
                EmployeeNumber = x.EmployeeNumber,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LabTechnicianDto> CreateAsync(
        LabTechnicianDto dto,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeRequiredText(dto.Name, "检验师姓名");
        var employeeNumber = NormalizeRequiredText(dto.EmployeeNumber, "检验师工号");

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var isDuplicate = await dbContext.LabTechnicians
            .AnyAsync(
                x => x.EmployeeNumber == employeeNumber,
                cancellationToken);

        if (isDuplicate)
        {
            throw new InvalidOperationException(
                $"工号“{employeeNumber}”已经存在。");
        }

        var entity = new LabTechnician
        {
            Name = name,
            EmployeeNumber = employeeNumber,
            IsActive = dto.IsActive
        };

        dbContext.LabTechnicians.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LabTechnicianDto
        {
            Id = entity.Id,
            Name = entity.Name,
            EmployeeNumber = entity.EmployeeNumber,
            IsActive = entity.IsActive
        };
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        LabTechnicianDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Id <= 0)
        {
            throw new ArgumentException("检验师主键无效。", nameof(dto));
        }

        var name = NormalizeRequiredText(dto.Name, "检验师姓名");
        var employeeNumber = NormalizeRequiredText(dto.EmployeeNumber, "检验师工号");

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.LabTechnicians
            .FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("未找到需要更新的检验师。");
        }

        var isDuplicate = await dbContext.LabTechnicians
            .AnyAsync(
                x => x.Id != dto.Id &&
                     x.EmployeeNumber == employeeNumber,
                cancellationToken);

        if (isDuplicate)
        {
            throw new InvalidOperationException(
                $"工号“{employeeNumber}”已经存在。");
        }

        entity.Name = name;
        entity.EmployeeNumber = employeeNumber;
        entity.IsActive = dto.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteOrDeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException("检验师主键无效。", nameof(id));
        }

        await using var dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.LabTechnicians
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        var hasInboundRecords = await dbContext.ConsumableInboundRecords
            .AnyAsync(x => x.InboundById == id, cancellationToken);

        var hasOutboundRecords = await dbContext.ConsumableOutboundRecords
            .AnyAsync(x => x.OutboundById == id, cancellationToken);

        if (hasInboundRecords || hasOutboundRecords)
        {
            entity.IsActive = false;
        }
        else
        {
            dbContext.LabTechnicians.Remove(entity);
        }

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