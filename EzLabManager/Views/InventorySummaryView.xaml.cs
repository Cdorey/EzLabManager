using System.Windows.Controls;

namespace EzLabManager.Views;

/// <summary>
/// 库存汇总视图。
/// </summary>
/// <remarks>
/// 该视图负责展示库存汇总、批次明细和耗材基础信息维护界面。
/// 它不直接访问数据库，也不直接创建 ViewModel。
/// 当前 ViewModel 由外层 MainWindow.xaml 通过 DataContext 传入。
/// </remarks>
public partial class InventorySummaryView : UserControl
{
    /// <summary>
    /// 初始化 <see cref="InventorySummaryView"/> 类的新实例。
    /// </summary>
    public InventorySummaryView()
    {
        InitializeComponent();
    }
}