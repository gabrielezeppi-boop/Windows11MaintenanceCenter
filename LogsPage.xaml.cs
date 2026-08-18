using Microsoft.UI.Xaml;
using System.Diagnostics;
namespace Windows11MaintenanceCenter.Views;
public sealed partial class LogsPage : Microsoft.UI.Xaml.Controls.Page
{
 public LogsPage(){InitializeComponent();}
 private void Open_Click(object s,RoutedEventArgs e)
 {
  var p=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Windows11MaintenanceCenter","Logs");
  Directory.CreateDirectory(p);
  Process.Start(new ProcessStartInfo("explorer.exe",p){UseShellExecute=true});
 }
}
