namespace Windows11MaintenanceCenter.Core;

public static class ResultInterpreter
{
    public static OperationState Sfc(string output, int exitCode) =>
        exitCode == 0 ? OperationState.Success : OperationState.Warning;

    public static OperationState Reboot(string output) =>
        output.Contains("REBOOT_REQUIRED", StringComparison.OrdinalIgnoreCase)
            ? OperationState.RebootRequired
            : OperationState.Success;
}
