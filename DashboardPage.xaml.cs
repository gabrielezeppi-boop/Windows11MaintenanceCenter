using Microsoft.UI.Xaml;

namespace Windows11MaintenanceCenter.Views;

public sealed partial class DashboardPage : Microsoft.UI.Xaml.Controls.Page
{
    public DashboardPage() => InitializeComponent();

    private void Health_Click(object sender, RoutedEventArgs e) =>
        (App.MainWindow as MainWindow)?.ContentFrame.Navigate(typeof(HealthPage));

    private void Updates_Click(object sender, RoutedEventArgs e) =>
        (App.MainWindow as MainWindow)?.ContentFrame.Navigate(typeof(UpdatesPage));

    private void Diagnostics_Click(object sender, RoutedEventArgs e) =>
        (App.MainWindow as MainWindow)?.ContentFrame.Navigate(typeof(DiagnosticsPage));
}
