using System.Windows;
using EzLabManager.ViewModels;

namespace EzLabManager;

/// <summary>
/// EzLabManager 主窗口。
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    /// <summary>
    /// 初始化 <see cref="MainWindow"/> 类的新实例。
    /// </summary>
    /// <param name="viewModel">主窗口 ViewModel。</param>
    public MainWindow(
        MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// 窗口加载完成后初始化 ViewModel 数据。
    /// </summary>
    /// <param name="sender">事件发送方。</param>
    /// <param name="e">事件参数。</param>
    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;

        await _viewModel.LoadAsync();
    }
}