using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows11MaintenanceCenter.Views;

namespace Windows11MaintenanceCenter;

public sealed partial class MainWindow : Window
{
    public Frame ContentFrame => ContentFrameControl;
    public MainWindow()
    {
        InitializeComponent();
        Nav.SelectedItem = Nav.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        switch (item.Tag?.ToString())
        {
            case "Dashboard": ContentFrame.Navigate(typeof(DashboardPage)); break;
            case "Health": ContentFrame.Navigate(typeof(HealthPage)); break;
            case "Updates": ContentFrame.Navigate(typeof(UpdatesPage)); break;
            case "Diagnostics": ContentFrame.Navigate(typeof(DiagnosticsPage)); break;
            case "Logs": ContentFrame.Navigate(typeof(LogsPage)); break;
        }
    }
}
