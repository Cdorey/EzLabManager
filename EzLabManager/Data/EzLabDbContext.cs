using EzLabManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzLabManager.Data;

/// <summary>
/// EzLabManager 的 EF Core 数据库上下文。
/// </summary>
/// <remarks>
/// 该上下文负责配置实验室耗材管理相关实体与 SQLite 数据库表之间的映射关系。
/// 当前包含耗材基础信息、入库记录、出库记录和检验师四类实体。
/// </remarks>
public class EzLabDbContext : DbContext
{
    /// <summary>
    /// 初始化 <see cref="EzLabDbContext"/> 类的新实例。
    /// </summary>
    /// <param name="options">数据库上下文配置选项。</param>
    public EzLabDbContext(DbContextOptions<EzLabDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 耗材基础信息表。
    /// </summary>
    public DbSet<ConsumableItem> ConsumableItems => Set<ConsumableItem>();

    /// <summary>
    /// 耗材入库记录表。
    /// </summary>
    public DbSet<ConsumableInboundRecord> ConsumableInboundRecords =>
        Set<ConsumableInboundRecord>();

    /// <summary>
    /// 耗材出库记录表。
    /// </summary>
    public DbSet<ConsumableOutboundRecord> ConsumableOutboundRecords =>
        Set<ConsumableOutboundRecord>();

    /// <summary>
    /// 检验师信息表。
    /// </summary>
    public DbSet<LabTechnician> LabTechnicians => Set<LabTechnician>();

    /// <summary>
    /// 配置实体模型与数据库结构之间的映射关系。
    /// </summary>
    /// <param name="modelBuilder">模型构建器。</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureConsumableItem(modelBuilder.Entity<ConsumableItem>());
        ConfigureConsumableInboundRecord(modelBuilder.Entity<ConsumableInboundRecord>());
        ConfigureConsumableOutboundRecord(modelBuilder.Entity<ConsumableOutboundRecord>());
        ConfigureLabTechnician(modelBuilder.Entity<LabTechnician>());
    }

    /// <summary>
    /// 配置耗材基础信息实体。
    /// </summary>
    /// <param name="entity">耗材基础信息实体构建器。</param>
    private static void ConfigureConsumableItem(
        EntityTypeBuilder<ConsumableItem> entity)
    {
        entity.ToTable("ConsumableItems");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.CategoryName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.ModelName)
            .HasMaxLength(100)
            .IsRequired();

        entity.HasIndex(x => new { x.CategoryName, x.ModelName })
            .IsUnique();

        entity.HasMany(x => x.InboundRecords)
            .WithOne(x => x.ConsumableItem)
            .HasForeignKey(x => x.ConsumableItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>
    /// 配置耗材入库记录实体。
    /// </summary>
    /// <param name="entity">耗材入库记录实体构建器。</param>
    private static void ConfigureConsumableInboundRecord(
        EntityTypeBuilder<ConsumableInboundRecord> entity)
    {
        entity.ToTable(
            "ConsumableInboundRecords",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ConsumableInboundRecords_Quantity_Positive",
                    "Quantity > 0");
            });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.BatchNumber)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.ExpirationDate)
            .IsRequired(false);

        entity.Property(x => x.Quantity)
            .IsRequired();

        entity.Property(x => x.InboundDate)
            .IsRequired();

        entity.HasIndex(x => x.ConsumableItemId);

        entity.HasIndex(x => new
        {
            x.ConsumableItemId,
            x.BatchNumber,
            x.ExpirationDate
        });

        entity.HasOne(x => x.ConsumableItem)
            .WithMany(x => x.InboundRecords)
            .HasForeignKey(x => x.ConsumableItemId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.InboundBy)
            .WithMany(x => x.InboundRecords)
            .HasForeignKey(x => x.InboundById)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.OutboundRecords)
            .WithOne(x => x.InboundRecord)
            .HasForeignKey(x => x.InboundRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>
    /// 配置耗材出库记录实体。
    /// </summary>
    /// <param name="entity">耗材出库记录实体构建器。</param>
    private static void ConfigureConsumableOutboundRecord(
        EntityTypeBuilder<ConsumableOutboundRecord> entity)
    {
        entity.ToTable(
            "ConsumableOutboundRecords",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ConsumableOutboundRecords_Quantity_Positive",
                    "Quantity > 0");
            });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.BatchNumber)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Quantity)
            .IsRequired();

        entity.Property(x => x.OutboundDate)
            .IsRequired();

        entity.HasIndex(x => x.InboundRecordId);

        entity.HasIndex(x => new
        {
            x.BatchNumber,
            x.OutboundDate
        });

        entity.HasOne(x => x.InboundRecord)
            .WithMany(x => x.OutboundRecords)
            .HasForeignKey(x => x.InboundRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.OutboundBy)
            .WithMany(x => x.OutboundRecords)
            .HasForeignKey(x => x.OutboundById)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>
    /// 配置检验师实体。
    /// </summary>
    /// <param name="entity">检验师实体构建器。</param>
    private static void ConfigureLabTechnician(
        EntityTypeBuilder<LabTechnician> entity)
    {
        entity.ToTable("LabTechnicians");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.EmployeeNumber)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => x.EmployeeNumber)
            .IsUnique();

        entity.HasMany(x => x.InboundRecords)
            .WithOne(x => x.InboundBy)
            .HasForeignKey(x => x.InboundById)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.OutboundRecords)
            .WithOne(x => x.OutboundBy)
            .HasForeignKey(x => x.OutboundById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}