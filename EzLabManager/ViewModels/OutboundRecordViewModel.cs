using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EzLabManager.Dtos;
using EzLabManager.Services;

namespace EzLabManager.ViewModels;

/// <summary>
/// 耗材出库页面的 ViewModel。
/// </summary>
/// <remarks>
/// 该 ViewModel 负责展示可出库库存批次、录入出库信息、保存出库记录和展示最近出库记录。
/// </remarks>
public partial class OutboundRecordViewModel : ObservableObject
{
    private readonly ILabTechnicianService _labTechnicianService;
    private readonly IConsumableOutboundRecordService _outboundRecordService;

    /// <summary>
    /// 初始化 <see cref="OutboundRecordViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="labTechnicianService">检验师信息业务服务。</param>
    /// <param name="outboundRecordService">出库记录业务服务。</param>
    public OutboundRecordViewModel(
        ILabTechnicianService labTechnicianService,
        IConsumableOutboundRecordService outboundRecordService)
    {
        _labTechnicianService = labTechnicianService;
        _outboundRecordService = outboundRecordService;
    }

    /// <summary>
    /// 当前可出库的库存批次列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<InventoryBatchDto> availableBatches = new();

    /// <summary>
    /// 最近出库记录列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ConsumableOutboundRecordDto> recentOutboundRecords = new();

    /// <summary>
    /// 有效检验师列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LabTechnicianDto> activeLabTechnicians = new();

    /// <summary>
    /// 当前选中的库存批次。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveOutboundRecordCommand))]
    private InventoryBatchDto? selectedInventoryBatch;

    /// <summary>
    /// 当前选中的出账人。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveOutboundRecordCommand))]
    private LabTechnicianDto? selectedOutboundBy;

    /// <summary>
    /// 库存批次搜索关键词。
    /// </summary>
    [ObservableProperty]
    private string inventoryBatchSearchKeyword = string.Empty;

    /// <summary>
    /// 出库数量。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveOutboundRecordCommand))]
    private int outboundQuantity = 1;

    /// <summary>
    /// 出账日期。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveOutboundRecordCommand))]
    private DateTime? outboundDate = DateTime.Today;

    /// <summary>
    /// 当前是否正在执行异步操作。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(SearchInventoryBatchesCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveOutboundRecordCommand))]
    private bool isBusy;

    /// <summary>
    /// 当前状态提示。
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// 初始化出库页面数据。
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await RefreshAsync();

        StatusMessage = "出库页面已加载。";
    }

    /// <summary>
    /// 刷新出库页面数据。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;

            await RefreshAvailableBatchesAsync();
            await RefreshActiveLabTechniciansAsync();
            await RefreshRecentOutboundRecordsAsync();

            if (SelectedOutboundBy is null && ActiveLabTechnicians.Count > 0)
            {
                SelectedOutboundBy = ActiveLabTechnicians[0];
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 根据关键词搜索可出库库存批次。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearchInventoryBatches))]
    private async Task SearchInventoryBatchesAsync()
    {
        await RefreshAvailableBatchesAsync();

        StatusMessage = string.IsNullOrWhiteSpace(InventoryBatchSearchKeyword)
            ? "已显示全部可出库批次。"
            : $"已按关键词“{InventoryBatchSearchKeyword.Trim()}”搜索可出库批次。";
    }

    /// <summary>
    /// 保存当前出库记录。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveOutboundRecord))]
    private async Task SaveOutboundRecordAsync()
    {
        if (SelectedInventoryBatch is null)
        {
            StatusMessage = "请选择需要出库的库存批次。";
            return;
        }

        if (SelectedOutboundBy is null)
        {
            StatusMessage = "请选择出账人。";
            return;
        }

        if (OutboundDate is null)
        {
            StatusMessage = "请选择出账日期。";
            return;
        }

        try
        {
            IsBusy = true;

            await _outboundRecordService.CreateAsync(
                new ConsumableOutboundRecordDto
                {
                    InboundRecordId = SelectedInventoryBatch.InboundRecordId,
                    Quantity = OutboundQuantity,
                    OutboundDate = OutboundDate.Value,
                    OutboundById = SelectedOutboundBy.Id
                });

            await RefreshAvailableBatchesAsync();
            await RefreshRecentOutboundRecordsAsync();

            OutboundQuantity = 1;
            OutboundDate = DateTime.Today;

            StatusMessage = "出库记录已保存。";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 当前选中库存批次变化时，调整默认出库数量。
    /// </summary>
    /// <param name="value">新的库存批次。</param>
    partial void OnSelectedInventoryBatchChanged(
        InventoryBatchDto? value)
    {
        if (value is null)
        {
            return;
        }

        if (OutboundQuantity <= 0 || OutboundQuantity > value.RemainingQuantity)
        {
            OutboundQuantity = Math.Min(1, value.RemainingQuantity);
        }

        StatusMessage = $"已选择批次“{value.BatchNumber}”，剩余库存 {value.RemainingQuantity}。";
    }

    /// <summary>
    /// 刷新可出库库存批次。
    /// </summary>
    private async Task RefreshAvailableBatchesAsync()
    {
        var selectedInboundRecordId = SelectedInventoryBatch?.InboundRecordId;

        var items = await _outboundRecordService.GetAvailableBatchesAsync(
            InventoryBatchSearchKeyword);

        AvailableBatches = new ObservableCollection<InventoryBatchDto>(items);

        if (selectedInboundRecordId is not null)
        {
            SelectedInventoryBatch = AvailableBatches
                .FirstOrDefault(x => x.InboundRecordId == selectedInboundRecordId.Value);
        }
    }

    /// <summary>
    /// 刷新有效检验师列表。
    /// </summary>
    private async Task RefreshActiveLabTechniciansAsync()
    {
        var selectedTechnicianId = SelectedOutboundBy?.Id;

        var items = await _labTechnicianService.GetListAsync(
            keyword: null,
            includeInactive: false);

        ActiveLabTechnicians = new ObservableCollection<LabTechnicianDto>(items);

        if (selectedTechnicianId is not null)
        {
            SelectedOutboundBy = ActiveLabTechnicians
                .FirstOrDefault(x => x.Id == selectedTechnicianId.Value);
        }
    }

    /// <summary>
    /// 刷新最近出库记录。
    /// </summary>
    private async Task RefreshRecentOutboundRecordsAsync()
    {
        var items = await _outboundRecordService.GetRecentListAsync();

        RecentOutboundRecords = new ObservableCollection<ConsumableOutboundRecordDto>(items);
    }

    /// <summary>
    /// 判断是否可以刷新页面。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanRefresh()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以搜索库存批次。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSearchInventoryBatches()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以保存出库记录。
    /// </summary>
    /// <returns>
    /// 如果已选择库存批次、出账人、出账日期，且出库数量合法，则返回 true。
    /// </returns>
    private bool CanSaveOutboundRecord()
    {
        return !IsBusy &&
               SelectedInventoryBatch is not null &&
               SelectedOutboundBy is not null &&
               OutboundDate is not null &&
               OutboundQuantity > 0 &&
               OutboundQuantity <= SelectedInventoryBatch.RemainingQuantity;
    }
}