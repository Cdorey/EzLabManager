using System.Windows.Controls;

namespace EzLabManager.Views;

/// <summary>
/// 耗材出库管理视图。
/// </summary>
/// <remarks>
/// 该视图仅负责界面展示和数据绑定。
/// 它不直接访问数据库，也不直接创建 ViewModel。
/// 当前 ViewModel 由外层 MainWindow.xaml 通过 DataContext 传入。
/// </remarks>
public partial class OutboundRecordView : UserControl
{
    /// <summary>
    /// 初始化 <see cref="OutboundRecordView"/> 类的新实例。
    /// </summary>
    public OutboundRecordView()
    {
        InitializeComponent();
    }
}