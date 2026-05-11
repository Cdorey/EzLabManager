using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EzLabManager.Dtos;
using EzLabManager.Models;
using EzLabManager.Services;
using System.Collections.ObjectModel;

namespace EzLabManager.ViewModels;

/// <summary>
/// 检验师管理页面的 ViewModel。
/// </summary>
/// <remarks>
/// 该 ViewModel 负责检验师列表展示、搜索、新增、修改、删除或停用。
/// 它不直接访问数据库，而是通过 <see cref="ILabTechnicianService"/> 调用业务服务层。
/// </remarks>
public partial class LabTechnicianViewModel : ObservableObject
{
    private readonly ILabTechnicianService _labTechnicianService;

    /// <summary>
    /// 初始化 <see cref="LabTechnicianViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="labTechnicianService">检验师信息业务服务。</param>
    public LabTechnicianViewModel(
        ILabTechnicianService labTechnicianService)
    {
        _labTechnicianService = labTechnicianService;
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
    /// 当前是否正在执行异步操作。
    /// </summary>
    [ObservableProperty]
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
    /// 表示当前编辑区是否正在编辑已有检验师。
    /// </summary>
    public bool IsEditingExistingLabTechnician => EditingLabTechnicianId is > 0;

    /// <summary>
    /// 初始化检验师管理页面数据。
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await RefreshLabTechnicianListAsync();

        ClearLabTechnicianEditor();

        StatusMessage = "检验师列表已加载。";
    }

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
        _ = RefreshLabTechnicianListAsync();
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