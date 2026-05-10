using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EzLabManager.Dtos;
using EzLabManager.Models;
using EzLabManager.Services;
using System.Collections.ObjectModel;

namespace EzLabManager.ViewModels;

/// <summary>
/// 主窗口 ViewModel。
/// </summary>
/// <remarks>
/// 当前 ViewModel 负责耗材基础信息的列表展示、搜索、新增、修改和删除。
/// 它不直接访问 EF Core DbContext，而是通过 <see cref="IConsumableItemService"/>
/// 调用业务服务层。
/// </remarks>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IConsumableItemService _consumableItemService;

    /// <summary>
    /// 初始化 <see cref="MainWindowViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="consumableItemService">耗材基础信息业务服务。</param>
    public MainWindowViewModel(
        IConsumableItemService consumableItemService)
    {
        _consumableItemService = consumableItemService;
    }

    /// <summary>
    /// 耗材基础信息列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ConsumableItemDto> consumableItems = new();

    /// <summary>
    /// 当前选中的耗材。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private ConsumableItemDto? selectedConsumableItem;

    /// <summary>
    /// 搜索关键词。
    /// </summary>
    [ObservableProperty]
    private string searchKeyword = string.Empty;

    /// <summary>
    /// 当前编辑的耗材主键。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingExistingItem))]
    private int? editingId;

    /// <summary>
    /// 当前编辑的耗材类目。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string editingCategoryName = string.Empty;

    /// <summary>
    /// 当前编辑的耗材型号或规格。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string editingModelName = string.Empty;

    /// <summary>
    /// 当前是否正在执行异步操作。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewCommand))]
    private bool isBusy;

    /// <summary>
    /// 当前状态提示。
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// 表示当前编辑区是否正在编辑已有耗材。
    /// </summary>
    public bool IsEditingExistingItem => EditingId is > 0;

    /// <summary>
    /// 初始化页面数据。
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await RefreshListAsync();

        ClearEditor();

        StatusMessage = "耗材基础信息已加载。";
    }

    /// <summary>
    /// 根据关键词搜索耗材基础信息。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync()
    {
        await RefreshListAsync();

        StatusMessage = string.IsNullOrWhiteSpace(SearchKeyword)
            ? "已显示全部耗材。"
            : $"已按关键词“{SearchKeyword.Trim()}”搜索。";
    }

    /// <summary>
    /// 切换到新增耗材状态。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanNew))]
    private void New()
    {
        SelectedConsumableItem = null;

        ClearEditor();

        StatusMessage = "正在新增耗材。";
    }

    /// <summary>
    /// 保存当前编辑的耗材基础信息。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            var dto = new ConsumableItemDto
            {
                Id = EditingId ?? 0,
                CategoryName = EditingCategoryName,
                ModelName = EditingModelName
            };

            int targetId;

            if (IsEditingExistingItem)
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

            await RefreshListAsync();

            SelectedConsumableItem = ConsumableItems
                .FirstOrDefault(x => x.Id == targetId);
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
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedConsumableItem is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var id = SelectedConsumableItem.Id;

            await _consumableItemService.DeleteAsync(id);

            await RefreshListAsync();

            ClearEditor();

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
    /// 当选中的耗材发生变化时，同步编辑区内容。
    /// </summary>
    /// <param name="value">新的选中耗材。</param>
    partial void OnSelectedConsumableItemChanged(
        ConsumableItemDto? value)
    {
        if (value is null)
        {
            return;
        }

        EditingId = value.Id;
        EditingCategoryName = value.CategoryName;
        EditingModelName = value.ModelName;
    }

    /// <summary>
    /// 刷新耗材基础信息列表。
    /// </summary>
    private async Task RefreshListAsync()
    {
        try
        {
            IsBusy = true;

            var items = await _consumableItemService
                .GetListAsync(SearchKeyword);

            ConsumableItems = new ObservableCollection<ConsumableItemDto>(items);
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
    /// 清空编辑区。
    /// </summary>
    private void ClearEditor()
    {
        EditingId = null;
        EditingCategoryName = string.Empty;
        EditingModelName = string.Empty;
    }

    /// <summary>
    /// 判断是否可以执行搜索。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSearch()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以进入新增状态。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanNew()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以保存当前编辑内容。
    /// </summary>
    /// <returns>如果类目和型号均不为空，且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSave()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(EditingCategoryName) &&
               !string.IsNullOrWhiteSpace(EditingModelName);
    }

    /// <summary>
    /// 判断是否可以删除当前选中耗材。
    /// </summary>
    /// <returns>如果已选中耗材，且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanDelete()
    {
        return !IsBusy && SelectedConsumableItem is not null;
    }
}