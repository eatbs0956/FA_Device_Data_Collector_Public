using Avalonia.Controls;
using Collector.Agent.ViewModels;

namespace Collector.Agent.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();

        // 当 DataContext 设置后，订阅 ViewModel 的滚动请求事件
        DataContextChanged += (_, _) =>
        {
            if (DataContext is LogViewModel vm)
            {
                vm.ScrollToBottomRequested += () =>
                {
                    // DataGrid 自动滚动到最后一行
                    var dataGrid = this.FindControl<DataGrid>("LogDataGrid");
                    if (dataGrid?.ItemsSource is System.Collections.IList items && items.Count > 0)
                    {
                        dataGrid.ScrollIntoView(items[items.Count - 1], null);
                    }
                };
            }
        };
    }
}
