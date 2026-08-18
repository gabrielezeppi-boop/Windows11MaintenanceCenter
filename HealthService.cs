using Windows11MaintenanceCenter.Core;

namespace Windows11MaintenanceCenter.Services;

public sealed class HealthService
{
    private readonly CommandRunner _runner;
    private readonly Logger _log;

    public HealthService(CommandRunner runner, Logger log)
    {
        _runner = runner;
        _log = log;
    }

    private async Task<CommandResult> Run(string op, string command, Action<string> output,
        int timeout = 60, CancellationToken ct = default)
    {
        _log.Write($"START {op}");
        async Task Sink(string line) { _log.Write(line); output(line); await Task.CompletedTask; }
        var r = await _runner.PowerShellAsync(op, command, Sink, timeout, ct);
        _log.Write($"END {op} | ExitCode={r.ExitCode} | State={r.State}");
        return r;
    }

    public Task<CommandResult> SystemInfo(Action<string> o, CancellationToken ct = default) =>
        Run("System information", @"
$os=Get-CimInstance Win32_OperatingSystem
Write-Output ""Windows=$($os.Caption)""
Write-Output ""Version=$($os.Version)""
Write-Output ""Build=$($os.BuildNumber)""
Write-Output ""Architecture=$env:PROCESSOR_ARCHITECTURE""
Write-Output ""LastBoot=$($os.LastBootUpTime)""
$disk=Get-CimInstance Win32_LogicalDisk -Filter ""DeviceID='$env:SystemDrive'""
Write-Output ""SystemDriveFreeGB=$([math]::Round($disk.FreeSpace/1GB,1))""
", o, 30, ct);

    public Task<CommandResult> DismCheck(Action<string> o, CancellationToken ct = default) =>
        Run("DISM CheckHealth", "DISM.exe /Online /Cleanup-Image /CheckHealth", o, 30, ct);

    public Task<CommandResult> DismScan(Action<string> o, CancellationToken ct = default) =>
        Run("DISM ScanHealth", "DISM.exe /Online /Cleanup-Image /ScanHealth", o, 90, ct);

    public async Task<CommandResult> Sfc(Action<string> o, CancellationToken ct = default)
    {
        var r = await Run("SFC /verifyonly", "sfc.exe /verifyonly", o, 90, ct);
        return r with { State = ResultInterpreter.Sfc(r.Output, r.ExitCode) };
    }

    public Task<CommandResult> ChkdskScan(Action<string> o, CancellationToken ct = default) =>
        Run("CHKDSK /scan", "chkdsk.exe $env:SystemDrive /scan", o, 90, ct);

    public Task<CommandResult> Services(Action<string> o, CancellationToken ct = default) =>
        Run("Service state", @"
$names='wuauserv','bits','WinDefend','AudioEndpointBuilder','Audiosrv','FontCache'
foreach($n in $names){
  $s=Get-Service -Name $n -ErrorAction SilentlyContinue
  if($null -eq $s){ Write-Output ""$n | NOT_PRESENT"" }
  else { Write-Output (""{0} | Status={1} | StartType={2}"" -f $s.Name,$s.Status,$s.StartType) }
}
", o, 30, ct);

    public Task<CommandResult> PendingReboot(Action<string> o, CancellationToken ct = default) =>
        Run("Pending reboot", @"
$paths=@(
'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending',
'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired'
)
$pending=$false
foreach($p in $paths){ if(Test-Path $p){$pending=$true; Write-Output ""PENDING: $p""}}
if(Test-Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager'){
 $v=(Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager' -Name PendingFileRenameOperations -ErrorAction SilentlyContinue)
 if($null -ne $v.PendingFileRenameOperations){$pending=$true; Write-Output 'PENDING: PendingFileRenameOperations'}
}
if($pending){Write-Output 'REBOOT_REQUIRED'}else{Write-Output 'NO_REBOOT_PENDING'}
", o, 30, ct);
}
