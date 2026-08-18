using Windows11MaintenanceCenter.Core;

namespace Windows11MaintenanceCenter.Services;

public sealed class DiagnosticsService
{
    private readonly CommandRunner _runner;
    private readonly Logger _log;
    public DiagnosticsService(CommandRunner runner, Logger log){_runner=runner;_log=log;}

    private async Task<CommandResult> Run(string op, string script, Action<string> output, int timeout=60, CancellationToken ct=default)
    {
        _log.Write($"START {op}");
        async Task Sink(string line){_log.Write(line);output(line);await Task.CompletedTask;}
        var r=await _runner.PowerShellAsync(op,script,Sink,timeout,ct);
        _log.Write($"END {op} | ExitCode={r.ExitCode}");
        return r;
    }

    public Task<CommandResult> RegistryInventory(Action<string> o, CancellationToken ct=default) =>
        Run("Registry inventory", @"
Write-Output '--- Windows version ---'
Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' |
 Select-Object ProductName,DisplayVersion,CurrentBuild,UBR |
 Format-List
Write-Output '--- RegBack inventory ---'
$p=""$env:windir\System32\config\RegBack""
if(Test-Path $p){Get-ChildItem $p -Force | Select-Object Name,Length,LastWriteTime | Format-Table -AutoSize}
else{Write-Output 'RegBack directory not present.'}
", o, 30, ct);

    public Task<CommandResult> HiveInventory(Action<string> o, CancellationToken ct=default) =>
        Run("Registry hive inventory", @"
'SYSTEM','SOFTWARE','SAM','SECURITY','DEFAULT' | ForEach-Object {
  $p=""$env:windir\System32\config\$_""
  if(Test-Path $p){Get-Item $p | Select-Object Name,Length,LastWriteTime | Format-Table -AutoSize}
  else{Write-Output ""$_ | NOT_FOUND""}
}
", o, 30, ct);

    public Task<CommandResult> ShadowInventory(Action<string> o, CancellationToken ct=default) =>
        Run("Shadow Copy inventory", "vssadmin.exe list shadows", o, 60, ct);

    public Task<CommandResult> HiveLoadTest(Action<string> o, CancellationToken ct=default) =>
        Run("SYSTEM hive load test", @"
$test='HKLM\_WMC_TEST_SYSTEM'
try{
  reg.exe query $test 2>$null | Out-Null
  if($LASTEXITCODE -eq 0){throw 'Temporary hive key already exists.'}
  reg.exe load $test ""$env:windir\System32\config\SYSTEM""
  if($LASTEXITCODE -ne 0){throw ""reg load failed with exit $LASTEXITCODE""}
  Write-Output 'SYSTEM hive load: SUCCESS'
}
finally{
  reg.exe unload $test 2>$null | Out-Null
  reg.exe query $test 2>$null | Out-Null
  if($LASTEXITCODE -eq 0){Write-Output 'CRITICAL: temporary hive remains loaded.'}
  else{Write-Output 'Temporary hive unload: SUCCESS'}
}
", o, 30, ct);
}
