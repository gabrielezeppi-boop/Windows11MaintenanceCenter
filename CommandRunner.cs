using System.Diagnostics;
using System.Text;

namespace Windows11MaintenanceCenter.Core;

public sealed class CommandRunner
{
    public async Task<CommandResult> RunAsync(
        string operation,
        string fileName,
        string arguments,
        Func<string, Task>? onOutput = null,
        int timeoutMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var started = DateTime.UtcNow;
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                output.AppendLine(e.Data);
                _ = onOutput?.Invoke(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                error.AppendLine(e.Data);
                _ = onOutput?.Invoke("[stderr] " + e.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMinutes(timeoutMinutes));
            await process.WaitForExitAsync(timeout.Token);
            // Ensure asynchronous stdout/stderr handlers have drained their final lines.
            process.WaitForExit();

            var state = process.ExitCode == 0 ? OperationState.Success : OperationState.Failed;
            return new CommandResult(operation, process.ExitCode, output.ToString(), error.ToString(),
                state, DateTime.UtcNow - started);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) { try { process.Kill(true); } catch { } }
            return new CommandResult(operation, -1, output.ToString(), error.ToString(),
                OperationState.Failed, DateTime.UtcNow - started, true);
        }
        catch (Exception ex)
        {
            return new CommandResult(operation, -2, output.ToString(), error.ToString() + Environment.NewLine + ex,
                OperationState.Failed, DateTime.UtcNow - started);
        }
    }

    public Task<CommandResult> PowerShellAsync(
        string operation,
        string script,
        Func<string, Task>? onOutput = null,
        int timeoutMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        return RunAsync(operation, "powershell.exe",
            $"-NoLogo -NoProfile -NonInteractive -EncodedCommand {encoded}",
            onOutput, timeoutMinutes, cancellationToken);
    }
}
