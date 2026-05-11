using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EzLabManager.Dtos;
using EzLabManager.Models;
using EzLabManager.Services;
using System.Collections.ObjectModel;

namespace EzLabManager.ViewModels;

/// <summary>
/// 耗材入库页面的 ViewModel。
/// </summary>
/// <remarks>
/// 该 ViewModel 负责耗材选择、耗材快捷新建、入库记录新增和最近入库记录展示。
/// </remarks>
public partial class InboundRecordViewModel : ObservableObject
{
    private readonly IConsumableItemService _consumableItemService;
    private readonly ILabTechnicianService _labTechnicianService;
    private readonly IConsumableInboundRecordService _inboundRecordService;

    /// <summary>
    /// 初始化 <see cref="InboundRecordViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="consumableItemService">耗材基础信息业务服务。</param>
    /// <param name="labTechnicianService">检验师信息业务服务。</param>
    /// <param name="inboundRecordService">入库记录业务服务。</param>
    public InboundRecordViewModel(
        IConsumableItemService consumableItemService,
        ILabTechnicianService labTechnicianService,
        IConsumableInboundRecordService inboundRecordService)
    {
        _consumableItemService = consumableItemService;
        _labTechnicianService = labTechnicianService;
        _inboundRecordService = inboundRecordService;
    }

    /// <summary>
    /// 可选耗材列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ConsumableItemDto> consumableItems = new();

    /// <summary>
    /// 可选入账人列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LabTechnicianDto> activeLabTechnicians = new();

    /// <summary>
    /// 最近入库记录列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ConsumableInboundRecordDto> recentInboundRecords = new();

    /// <summary>
    /// 当前选中的耗材。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveInboundRecordCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrepareCopiedConsumableItemCommand))]
    private ConsumableItemDto? selectedConsumableItem;

    /// <summary>
    /// 当前选中的入账人。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveInboundRecordCommand))]
    private LabTechnicianDto? selectedInboundBy;

    /// <summary>
    /// 耗材搜索关键词。
    /// </summary>
    [ObservableProperty]
    private string consumableSearchKeyword = string.Empty;

    /// <summary>
    /// 入库批号。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveInboundRecordCommand))]
    private string inboundBatchNumber = string.Empty;

    /// <summary>
    /// 入库效期。
    /// </summary>
    [ObservableProperty]
    private DateTime? inboundExpirationDate;

    /// <summary>
    /// 入库数量。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveInboundRecordCommand))]
    private int inboundQuantity = 1;

    /// <summary>
    /// 入账日期。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveInboundRecordCommand))]
    private DateTime inboundDate = DateTime.Today;

    /// <summary>
    /// 新建耗材的类目。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateConsumableItemCommand))]
    private string newConsumableCategoryName = string.Empty;

    /// <summary>
    /// 新建耗材的型号或规格。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateConsumableItemCommand))]
    private string newConsumableModelName = string.Empty;

    /// <summary>
    /// 当前是否正在执行异步操作。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(SearchConsumableItemsCommand))]
    [NotifyCanExecuteChangedFor(nameof(CreateConsumableItemCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveInboundRecordCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrepareBlankConsumableItemCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrepareCopiedConsumableItemCommand))]
    private bool isBusy;

    /// <summary>
    /// 当前状态提示。
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// 初始化入库页面数据。
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await RefreshAsync();

        StatusMessage = "入库页面已加载。";
    }

    /// <summary>
    /// 刷新入库页面基础数据。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;

            await RefreshConsumableItemsAsync();
            await RefreshActiveLabTechniciansAsync();
            await RefreshRecentInboundRecordsAsync();

            if (SelectedInboundBy is null && ActiveLabTechnicians.Count > 0)
            {
                SelectedInboundBy = ActiveLabTechnicians[0];
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
    /// 根据关键词搜索耗材。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearchConsumableItems))]
    private async Task SearchConsumableItemsAsync()
    {
        await RefreshConsumableItemsAsync();

        StatusMessage = string.IsNullOrWhiteSpace(ConsumableSearchKeyword)
            ? "已显示全部耗材。"
            : $"已按关键词“{ConsumableSearchKeyword.Trim()}”搜索耗材。";
    }

    /// <summary>
    /// 准备从空白信息新建耗材。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPrepareBlankConsumableItem))]
    private void PrepareBlankConsumableItem()
    {
        NewConsumableCategoryName = string.Empty;
        NewConsumableModelName = string.Empty;

        StatusMessage = "已切换为空白新建耗材。";
    }

    /// <summary>
    /// 准备复制当前耗材类目并新建一个新规格。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPrepareCopiedConsumableItem))]
    private void PrepareCopiedConsumableItem()
    {
        if (SelectedConsumableItem is null)
        {
            StatusMessage = "请先选择一个耗材，再复制其基本信息。";
            return;
        }

        NewConsumableCategoryName = SelectedConsumableItem.CategoryName;
        NewConsumableModelName = string.Empty;

        StatusMessage = $"已复制类目“{SelectedConsumableItem.CategoryName}”，请填写新规格。";
    }

    /// <summary>
    /// 创建新的耗材基础信息，并自动选中新建耗材。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreateConsumableItem))]
    private async Task CreateConsumableItemAsync()
    {
        try
        {
            IsBusy = true;

            var created = await _consumableItemService.CreateAsync(
                new ConsumableItemDto
                {
                    CategoryName = NewConsumableCategoryName,
                    ModelName = NewConsumableModelName
                });

            ConsumableSearchKeyword = string.Empty;

            await RefreshConsumableItemsAsync();

            SelectedConsumableItem = ConsumableItems
                .FirstOrDefault(x => x.Id == created.Id);

            NewConsumableCategoryName = string.Empty;
            NewConsumableModelName = string.Empty;

            StatusMessage = "新耗材已创建并选中，可继续录入入库信息。";
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
    /// 保存当前入库记录。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveInboundRecord))]
    private async Task SaveInboundRecordAsync()
    {
        if (SelectedConsumableItem is null)
        {
            StatusMessage = "请选择需要入库的耗材。";
            return;
        }

        if (SelectedInboundBy is null)
        {
            StatusMessage = "请选择入账人。";
            return;
        }

        try
        {
            IsBusy = true;

            await _inboundRecordService.CreateAsync(
                new ConsumableInboundRecordDto
                {
                    ConsumableItemId = SelectedConsumableItem.Id,
                    BatchNumber = InboundBatchNumber,
                    ExpirationDate = InboundExpirationDate,
                    Quantity = InboundQuantity,
                    InboundDate = InboundDate,
                    InboundById = SelectedInboundBy.Id
                });

            await RefreshRecentInboundRecordsAsync();

            InboundBatchNumber = string.Empty;
            InboundExpirationDate = null;
            InboundQuantity = 1;
            InboundDate = DateTime.Today;

            StatusMessage = "入库记录已保存。";
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
    /// 刷新耗材列表。
    /// </summary>
    private async Task RefreshConsumableItemsAsync()
    {
        var items = await _consumableItemService.GetListAsync(
            ConsumableSearchKeyword);

        ConsumableItems = new ObservableCollection<ConsumableItemDto>(items);
    }

    /// <summary>
    /// 刷新有效检验师列表。
    /// </summary>
    private async Task RefreshActiveLabTechniciansAsync()
    {
        var items = await _labTechnicianService.GetListAsync(
            keyword: null,
            includeInactive: false);

        ActiveLabTechnicians = new ObservableCollection<LabTechnicianDto>(items);
    }

    /// <summary>
    /// 刷新最近入库记录。
    /// </summary>
    private async Task RefreshRecentInboundRecordsAsync()
    {
        var items = await _inboundRecordService.GetRecentListAsync();

        RecentInboundRecords = new ObservableCollection<ConsumableInboundRecordDto>(items);
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
    /// 判断是否可以搜索耗材。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSearchConsumableItems()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以准备空白新建耗材。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanPrepareBlankConsumableItem()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以复制当前耗材并准备新建规格。
    /// </summary>
    /// <returns>如果已选择耗材且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanPrepareCopiedConsumableItem()
    {
        return !IsBusy && SelectedConsumableItem is not null;
    }

    /// <summary>
    /// 判断是否可以创建新耗材。
    /// </summary>
    /// <returns>
    /// 如果类目和型号均不为空，且当前未处于忙碌状态，则返回 true。
    /// </returns>
    private bool CanCreateConsumableItem()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(NewConsumableCategoryName) &&
               !string.IsNullOrWhiteSpace(NewConsumableModelName);
    }

    /// <summary>
    /// 判断是否可以保存入库记录。
    /// </summary>
    /// <returns>
    /// 如果已选择耗材、入账人、批号，数量大于 0，且当前未处于忙碌状态，则返回 true。
    /// </returns>
    private bool CanSaveInboundRecord()
    {
        return !IsBusy &&
               SelectedConsumableItem is not null &&
               SelectedInboundBy is not null &&
               !string.IsNullOrWhiteSpace(InboundBatchNumber) &&
               InboundQuantity > 0 &&
               InboundDate != default;
    }
}