using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Windows11MaintenanceCenter.Core;
using Windows11MaintenanceCenter.Services;

namespace Windows11MaintenanceCenter.Views;

public sealed partial class DiagnosticsPage : Page
{
    private readonly DiagnosticsService _svc = new(new CommandRunner(), new Logger());
    private DispatcherTimer? _timer;
    private Stopwatch? _stopwatch;

    public DiagnosticsPage() => InitializeComponent();

    private void Write(string text) =>
        DispatcherQueue.TryEnqueue(() => Output.Text += text + Environment.NewLine);

    private async Task Run(
        Func<Action<string>, CancellationToken, Task<CommandResult>> operation,
        string operationName)
    {
        if (_timer is not null) return;

        var info = OperationCatalog.Get(operationName);
        StatusLabel.Text = "IN ESECUZIONE";
        StatusSummary.Text = $"{info.Title} — {info.WhatItDoes}";
        StatusDetail.Text = $"Sicurezza: {info.Safety}   |   Durata indicativa: {info.Estimate}";
        Elapsed.Text = "Tempo trascorso: 00:00";
        Progress.IsIndeterminate = true;
        Output.Text = "";

        _stopwatch = Stopwatch.StartNew();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) =>
        {
            if (_stopwatch is not null)
                Elapsed.Text = $"Tempo trascorso: {_stopwatch.Elapsed:mm\\:ss}";
        };
        _timer.Start();

        try
        {
            var result = await operation(Write, default);
            var view = OperationPresentationHelper.Create(result);
            StatusLabel.Text = view.StateLabel;
            StatusSummary.Text = $"{view.Title} — {view.Summary}";
            StatusDetail.Text = $"{view.Detail}   Durata effettiva: {result.Duration:mm\\:ss}.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "NEGATIVO";
            StatusSummary.Text = "Il programma non ha completato l'operazione.";
            StatusDetail.Text = ex.Message;
            Write("Errore tecnico: " + ex);
        }
        finally
        {
            _timer?.Stop();
            _timer = null;
            if (_stopwatch is not null)
                Elapsed.Text = $"Tempo trascorso: {_stopwatch.Elapsed:mm\\:ss}";
            Progress.IsIndeterminate = false;
            Progress.Value = 100;
        }
    }

    private void Registry_Click(object s, RoutedEventArgs e) => _ = Run(_svc.RegistryInventory, "Registry inventory");
    private void Hive_Click(object s, RoutedEventArgs e) => _ = Run(_svc.HiveInventory, "Registry hive inventory");
    private void Shadow_Click(object s, RoutedEventArgs e) => _ = Run(_svc.ShadowInventory, "Shadow Copy inventory");
    private void HiveTest_Click(object s, RoutedEventArgs e) => _ = Run(_svc.HiveLoadTest, "SYSTEM hive load test");
}
