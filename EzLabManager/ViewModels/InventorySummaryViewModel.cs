using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EzLabManager.Dtos;
using EzLabManager.Services;

namespace EzLabManager.ViewModels;

/// <summary>
/// 库存汇总页面的 ViewModel。
/// </summary>
/// <remarks>
/// 该 ViewModel 负责展示按耗材汇总的库存、按批次汇总的库存明细，
/// 并承担耗材基础信息的新建、复制新规格、修改和删除功能。
/// </remarks>
public partial class InventorySummaryViewModel : ObservableObject
{
    private readonly IInventorySummaryService _inventorySummaryService;
    private readonly IConsumableItemService _consumableItemService;

    /// <summary>
    /// 初始化 <see cref="InventorySummaryViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="inventorySummaryService">库存汇总业务服务。</param>
    /// <param name="consumableItemService">耗材基础信息业务服务。</param>
    public InventorySummaryViewModel(
        IInventorySummaryService inventorySummaryService,
        IConsumableItemService consumableItemService)
    {
        _inventorySummaryService = inventorySummaryService;
        _consumableItemService = consumableItemService;
    }

    /// <summary>
    /// 按耗材汇总的库存列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<InventoryItemSummaryDto> itemSummaries = new();

    /// <summary>
    /// 按批次汇总的库存明细列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<InventoryBatchDto> batchSummaries = new();

    /// <summary>
    /// 当前选中的耗材库存汇总项。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteConsumableItemCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrepareCopiedConsumableItemCommand))]
    private InventoryItemSummaryDto? selectedItemSummary;

    /// <summary>
    /// 库存搜索关键词。
    /// </summary>
    [ObservableProperty]
    private string inventorySearchKeyword = string.Empty;

    /// <summary>
    /// 是否包含零库存耗材和已出完批次。
    /// </summary>
    [ObservableProperty]
    private bool includeZeroStock = true;

    /// <summary>
    /// 当前编辑的耗材主键。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingExistingConsumableItem))]
    private int? editingConsumableItemId;

    /// <summary>
    /// 当前编辑的耗材类目。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConsumableItemCommand))]
    private string editingConsumableCategoryName = string.Empty;

    /// <summary>
    /// 当前编辑的耗材型号或规格。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConsumableItemCommand))]
    private string editingConsumableModelName = string.Empty;

    /// <summary>
    /// 当前是否正在执行异步操作。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(SearchInventoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrepareBlankConsumableItemCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrepareCopiedConsumableItemCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveConsumableItemCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteConsumableItemCommand))]
    private bool isBusy;

    /// <summary>
    /// 当前状态提示。
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// 表示当前是否正在编辑已有耗材。
    /// </summary>
    public bool IsEditingExistingConsumableItem => EditingConsumableItemId is > 0;

    /// <summary>
    /// 初始化库存汇总页面数据。
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await RefreshAsync();

        ClearConsumableItemEditor();

        StatusMessage = "库存汇总已加载。";
    }

    /// <summary>
    /// 刷新库存汇总页面数据。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;

            await RefreshInventorySummariesAsync();

            StatusMessage = "库存汇总已刷新。";
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
    /// 根据关键词搜索库存信息。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearchInventory))]
    private async Task SearchInventoryAsync()
    {
        await RefreshInventorySummariesAsync();

        StatusMessage = string.IsNullOrWhiteSpace(InventorySearchKeyword)
            ? "已显示全部库存。"
            : $"已按关键词“{InventorySearchKeyword.Trim()}”搜索库存。";
    }

    /// <summary>
    /// 准备从空白信息新建耗材。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPrepareBlankConsumableItem))]
    private void PrepareBlankConsumableItem()
    {
        SelectedItemSummary = null;

        ClearConsumableItemEditor();

        StatusMessage = "正在空白新建耗材。";
    }

    /// <summary>
    /// 准备复制当前耗材类目并新建一个新规格。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPrepareCopiedConsumableItem))]
    private void PrepareCopiedConsumableItem()
    {
        if (SelectedItemSummary is null)
        {
            StatusMessage = "请先选择一个耗材。";
            return;
        }

        EditingConsumableItemId = null;
        EditingConsumableCategoryName = SelectedItemSummary.CategoryName;
        EditingConsumableModelName = string.Empty;

        StatusMessage = $"已复制类目“{SelectedItemSummary.CategoryName}”，请填写新规格。";
    }

    /// <summary>
    /// 保存当前编辑的耗材基础信息。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveConsumableItem))]
    private async Task SaveConsumableItemAsync()
    {
        try
        {
            IsBusy = true;

            var dto = new ConsumableItemDto
            {
                Id = EditingConsumableItemId ?? 0,
                CategoryName = EditingConsumableCategoryName,
                ModelName = EditingConsumableModelName
            };

            int targetId;

            if (IsEditingExistingConsumableItem)
            {
                await _consumableItemService.UpdateAsync(dto);

                targetId = dto.Id;

                StatusMessage = "耗材基础信息已更新。";
            }
            else
            {
                var created = await _consumableItemService.CreateAsync(dto);

                targetId = created.Id;

                StatusMessage = "耗材基础信息已新增。";
            }

            await RefreshInventorySummariesAsync();

            SelectedItemSummary = ItemSummaries
                .FirstOrDefault(x => x.ConsumableItemId == targetId);
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
    /// 删除当前选中的耗材基础信息。
    /// </summary>
    /// <remarks>
    /// 如果该耗材已经存在入库记录，服务层会拒绝删除。
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanDeleteConsumableItem))]
    private async Task DeleteConsumableItemAsync()
    {
        if (SelectedItemSummary is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            await _consumableItemService.DeleteAsync(
                SelectedItemSummary.ConsumableItemId);

            await RefreshInventorySummariesAsync();

            ClearConsumableItemEditor();

            StatusMessage = "耗材基础信息已删除。";
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
    /// 当前选中的耗材汇总项变化时，同步耗材编辑区。
    /// </summary>
    /// <param name="value">新的耗材汇总项。</param>
    partial void OnSelectedItemSummaryChanged(
        InventoryItemSummaryDto? value)
    {
        if (value is null)
        {
            return;
        }

        EditingConsumableItemId = value.ConsumableItemId;
        EditingConsumableCategoryName = value.CategoryName;
        EditingConsumableModelName = value.ModelName;
    }

    /// <summary>
    /// 是否包含零库存发生变化时刷新库存列表。
    /// </summary>
    /// <param name="value">是否包含零库存。</param>
    partial void OnIncludeZeroStockChanged(
        bool value)
    {
        _ = RefreshInventorySummariesAsync();
    }

    /// <summary>
    /// 刷新库存汇总和批次明细。
    /// </summary>
    private async Task RefreshInventorySummariesAsync()
    {
        var selectedItemId = SelectedItemSummary?.ConsumableItemId;

        var itemSummaries = await _inventorySummaryService.GetItemSummariesAsync(
            InventorySearchKeyword,
            IncludeZeroStock);

        var batchSummaries = await _inventorySummaryService.GetBatchSummariesAsync(
            InventorySearchKeyword,
            IncludeZeroStock);

        ItemSummaries = new ObservableCollection<InventoryItemSummaryDto>(itemSummaries);
        BatchSummaries = new ObservableCollection<InventoryBatchDto>(batchSummaries);

        if (selectedItemId is not null)
        {
            SelectedItemSummary = ItemSummaries
                .FirstOrDefault(x => x.ConsumableItemId == selectedItemId.Value);
        }
    }

    /// <summary>
    /// 清空耗材基础信息编辑区。
    /// </summary>
    private void ClearConsumableItemEditor()
    {
        EditingConsumableItemId = null;
        EditingConsumableCategoryName = string.Empty;
        EditingConsumableModelName = string.Empty;
    }

    /// <summary>
    /// 判断是否可以刷新。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanRefresh()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以搜索库存。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSearchInventory()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以空白新建耗材。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanPrepareBlankConsumableItem()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以复制当前耗材并新建新规格。
    /// </summary>
    /// <returns>如果已选中耗材且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanPrepareCopiedConsumableItem()
    {
        return !IsBusy && SelectedItemSummary is not null;
    }

    /// <summary>
    /// 判断是否可以保存耗材基础信息。
    /// </summary>
    /// <returns>如果类目和型号均不为空，且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSaveConsumableItem()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(EditingConsumableCategoryName) &&
               !string.IsNullOrWhiteSpace(EditingConsumableModelName);
    }

    /// <summary>
    /// 判断是否可以删除当前选中耗材。
    /// </summary>
    /// <returns>如果已选中耗材且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanDeleteConsumableItem()
    {
        return !IsBusy && SelectedItemSummary is not null;
    }
}