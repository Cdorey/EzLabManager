namespace EzLabManager.ViewModels;

/// <summary>
/// 主窗口 ViewModel。
/// </summary>
/// <remarks>
/// 该 ViewModel 作为主窗口外壳使用。
/// 具体业务页面由各自独立的 ViewModel 负责。
/// </remarks>
public class MainWindowViewModel
{
    /// <summary>
    /// 初始化 <see cref="MainWindowViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="inventorySummaryViewModel">库存汇总页面 ViewModel。</param>
    /// <param name="labTechnicianViewModel">检验师管理页面 ViewModel。</param>
    /// <param name="inboundRecordViewModel">耗材入库页面 ViewModel。</param>
    /// <param name="outboundRecordViewModel">耗材出库页面 ViewModel。</param>
    public MainWindowViewModel(
        InventorySummaryViewModel inventorySummaryViewModel,
        LabTechnicianViewModel labTechnicianViewModel,
        InboundRecordViewModel inboundRecordViewModel,
        OutboundRecordViewModel outboundRecordViewModel)
    {
        InventorySummaryViewModel = inventorySummaryViewModel;
        LabTechnicianViewModel = labTechnicianViewModel;
        InboundRecordViewModel = inboundRecordViewModel;
        OutboundRecordViewModel = outboundRecordViewModel;
    }

    /// <summary>
    /// 库存汇总页面 ViewModel。
    /// </summary>
    public InventorySummaryViewModel InventorySummaryViewModel { get; }

    /// <summary>
    /// 检验师管理页面 ViewModel。
    /// </summary>
    public LabTechnicianViewModel LabTechnicianViewModel { get; }

    /// <summary>
    /// 耗材入库页面 ViewModel。
    /// </summary>
    public InboundRecordViewModel InboundRecordViewModel { get; }

    /// <summary>
    /// 耗材出库页面 ViewModel。
    /// </summary>
    public OutboundRecordViewModel OutboundRecordViewModel { get; }

    /// <summary>
    /// 初始化所有子页面数据。
    /// </summary>
    public async Task LoadAsync()
    {
        await InventorySummaryViewModel.LoadAsync();
        await LabTechnicianViewModel.LoadAsync();
        await InboundRecordViewModel.LoadAsync();
        await OutboundRecordViewModel.LoadAsync();
    }
}