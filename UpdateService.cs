using Windows11MaintenanceCenter.Core;

namespace Windows11MaintenanceCenter.Services;

public sealed class UpdateService
{
    private readonly CommandRunner _runner;
    private readonly Logger _log;
    public UpdateService(CommandRunner runner, Logger log){_runner=runner;_log=log;}

    private async Task<CommandResult> Run(string op,string script,Action<string> o,int timeout=120,CancellationToken ct=default)
    {
        _log.Write($"START {op}");
        async Task Sink(string line){_log.Write(line);o(line);await Task.CompletedTask;}
        var r=await _runner.PowerShellAsync(op,script,Sink,timeout,ct);
        _log.Write($"END {op} | ExitCode={r.ExitCode}");
        return r;
    }

    public Task<CommandResult> DetectProviders(Action<string> o,CancellationToken ct=default) =>
        Run("Update provider inventory", @"
foreach($c in 'winget','choco','scoop'){
  $cmd=Get-Command $c -ErrorAction SilentlyContinue
  if($cmd){Write-Output ""$c | PRESENT | $($cmd.Source)""}
  else{Write-Output ""$c | NOT_INSTALLED""}
}
$pswu=Get-Module -ListAvailable -Name PSWindowsUpdate
if($pswu){Write-Output ""PSWindowsUpdate | PRESENT | $($pswu.Version | Select-Object -First 1)""}
else{Write-Output 'PSWindowsUpdate | NOT_INSTALLED'}
",o,30,ct);

    public Task<CommandResult> WingetUpgrade(Action<string> o,CancellationToken ct=default) =>
        Run("WinGet user-approved upgrade", @"
if(-not (Get-Command winget.exe -ErrorAction SilentlyContinue)){Write-Output 'WinGet not installed.'; exit 2}
winget.exe source update
winget.exe upgrade --all --accept-package-agreements --accept-source-agreements
",o,180,ct);

    public Task<CommandResult> WindowsUpdate(Action<string> o,CancellationToken ct=default) =>
        Run("Windows Update user-approved", @"
$pswu=Get-Module -ListAvailable -Name PSWindowsUpdate
if(-not $pswu){Write-Output 'PSWindowsUpdate not installed. No module is installed automatically.'; exit 2}
Import-Module PSWindowsUpdate -Force
Get-WindowsUpdate -MicrosoftUpdate
Install-WindowsUpdate -MicrosoftUpdate -AcceptAll -IgnoreReboot
if((Get-WURebootStatus).RebootRequired){Write-Output 'REBOOT_REQUIRED'}
",o,240,ct);
}
