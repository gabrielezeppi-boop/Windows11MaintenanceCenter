using System.Text;

namespace Windows11MaintenanceCenter.Core;

public sealed class Logger
{
    private readonly string _path;
    private readonly object _gate = new();

    public Logger()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Windows11MaintenanceCenter", "Logs");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, $"Session-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    }

    public string FilePath => _path;

    public void Write(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        lock (_gate) File.AppendAllText(_path, line + Environment.NewLine, Encoding.UTF8);
    }
}
