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
    private readonly ILabTechnicianService _labTechnicianService;

    /// <summary>
    /// 初始化 <see cref="MainWindowViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="consumableItemService">耗材基础信息业务服务。</param>
    /// <param name="labTechnicianService">检验师信息业务服务。</param>
    public MainWindowViewModel(
        IConsumableItemService consumableItemService,
        ILabTechnicianService labTechnicianService)
    {
        _consumableItemService = consumableItemService;
        _labTechnicianService = labTechnicianService;
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
    [NotifyCanExecuteChangedFor(nameof(SearchLabTechniciansCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewLabTechnicianCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveLabTechnicianCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteLabTechnicianCommand))]
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
        await RefreshLabTechnicianListAsync();

        ClearEditor();
        ClearLabTechnicianEditor();

        StatusMessage = "基础数据已加载。";
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

    /// <summary>
    /// 检验师列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LabTechnicianDto> labTechnicians = new();

    /// <summary>
    /// 当前选中的检验师。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteLabTechnicianCommand))]
    private LabTechnicianDto? selectedLabTechnician;

    /// <summary>
    /// 检验师搜索关键词。
    /// </summary>
    [ObservableProperty]
    private string labTechnicianSearchKeyword = string.Empty;

    /// <summary>
    /// 检验师列表是否包含已停用人员。
    /// </summary>
    [ObservableProperty]
    private bool includeInactiveLabTechnicians = true;

    /// <summary>
    /// 当前编辑的检验师主键。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingExistingLabTechnician))]
    private int? editingLabTechnicianId;

    /// <summary>
    /// 当前编辑的检验师姓名。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveLabTechnicianCommand))]
    private string editingLabTechnicianName = string.Empty;

    /// <summary>
    /// 当前编辑的检验师工号。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveLabTechnicianCommand))]
    private string editingLabTechnicianEmployeeNumber = string.Empty;

    /// <summary>
    /// 当前编辑的检验师是否有效。
    /// </summary>
    [ObservableProperty]
    private bool editingLabTechnicianIsActive = true;

    /// <summary>
    /// 表示当前编辑区是否正在编辑已有检验师。
    /// </summary>
    public bool IsEditingExistingLabTechnician => EditingLabTechnicianId is > 0;

    /// <summary>
    /// 根据关键词搜索检验师。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearchLabTechnicians))]
    private async Task SearchLabTechniciansAsync()
    {
        await RefreshLabTechnicianListAsync();

        StatusMessage = string.IsNullOrWhiteSpace(LabTechnicianSearchKeyword)
            ? "已显示检验师列表。"
            : $"已按关键词“{LabTechnicianSearchKeyword.Trim()}”搜索检验师。";
    }

    /// <summary>
    /// 切换到新增检验师状态。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanNewLabTechnician))]
    private void NewLabTechnician()
    {
        SelectedLabTechnician = null;

        ClearLabTechnicianEditor();

        StatusMessage = "正在新增检验师。";
    }

    /// <summary>
    /// 保存当前编辑的检验师信息。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveLabTechnician))]
    private async Task SaveLabTechnicianAsync()
    {
        try
        {
            IsBusy = true;

            var dto = new LabTechnicianDto
            {
                Id = EditingLabTechnicianId ?? 0,
                Name = EditingLabTechnicianName,
                EmployeeNumber = EditingLabTechnicianEmployeeNumber,
                IsActive = EditingLabTechnicianIsActive
            };

            int targetId;

            if (IsEditingExistingLabTechnician)
            {
                await _labTechnicianService.UpdateAsync(dto);

                targetId = dto.Id;

                StatusMessage = "检验师信息已更新。";
            }
            else
            {
                var created = await _labTechnicianService.CreateAsync(dto);

                targetId = created.Id;

                StatusMessage = "检验师信息已新增。";
            }

            await RefreshLabTechnicianListAsync();

            SelectedLabTechnician = LabTechnicians
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
    /// 删除或停用当前选中的检验师。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteLabTechnician))]
    private async Task DeleteLabTechnicianAsync()
    {
        if (SelectedLabTechnician is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var id = SelectedLabTechnician.Id;

            await _labTechnicianService.DeleteOrDeactivateAsync(id);

            await RefreshLabTechnicianListAsync();

            ClearLabTechnicianEditor();

            StatusMessage = "检验师已删除；如存在历史记录，则已设置为无效。";
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
    /// 当选中的检验师发生变化时，同步编辑区内容。
    /// </summary>
    /// <param name="value">新的选中检验师。</param>
    partial void OnSelectedLabTechnicianChanged(
        LabTechnicianDto? value)
    {
        if (value is null)
        {
            return;
        }

        EditingLabTechnicianId = value.Id;
        EditingLabTechnicianName = value.Name;
        EditingLabTechnicianEmployeeNumber = value.EmployeeNumber;
        EditingLabTechnicianIsActive = value.IsActive;
    }

    /// <summary>
    /// 当是否包含已停用检验师发生变化时刷新检验师列表。
    /// </summary>
    /// <param name="value">是否包含已停用检验师。</param>
    partial void OnIncludeInactiveLabTechniciansChanged(
        bool value)
    {
        _ = SearchLabTechniciansAsync();
    }

    /// <summary>
    /// 刷新检验师列表。
    /// </summary>
    private async Task RefreshLabTechnicianListAsync()
    {
        try
        {
            IsBusy = true;

            var items = await _labTechnicianService.GetListAsync(
                LabTechnicianSearchKeyword,
                IncludeInactiveLabTechnicians);

            LabTechnicians = new ObservableCollection<LabTechnicianDto>(items);
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
    /// 清空检验师编辑区。
    /// </summary>
    private void ClearLabTechnicianEditor()
    {
        EditingLabTechnicianId = null;
        EditingLabTechnicianName = string.Empty;
        EditingLabTechnicianEmployeeNumber = string.Empty;
        EditingLabTechnicianIsActive = true;
    }

    /// <summary>
    /// 判断是否可以搜索检验师。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanSearchLabTechnicians()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以进入新增检验师状态。
    /// </summary>
    /// <returns>如果当前未处于忙碌状态，则返回 true。</returns>
    private bool CanNewLabTechnician()
    {
        return !IsBusy;
    }

    /// <summary>
    /// 判断是否可以保存检验师信息。
    /// </summary>
    /// <returns>
    /// 如果姓名和工号均不为空，且当前未处于忙碌状态，则返回 true。
    /// </returns>
    private bool CanSaveLabTechnician()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(EditingLabTechnicianName) &&
               !string.IsNullOrWhiteSpace(EditingLabTechnicianEmployeeNumber);
    }

    /// <summary>
    /// 判断是否可以删除或停用当前选中的检验师。
    /// </summary>
    /// <returns>如果已选中检验师，且当前未处于忙碌状态，则返回 true。</returns>
    private bool CanDeleteLabTechnician()
    {
        return !IsBusy && SelectedLabTechnician is not null;
    }
}