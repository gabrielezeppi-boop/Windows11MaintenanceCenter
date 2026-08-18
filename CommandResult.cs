namespace Windows11MaintenanceCenter.Core;

public enum OperationState { Success, Warning, Failed, Skipped, NotApplicable, RebootRequired }

public sealed record CommandResult(
    string Operation,
    int ExitCode,
    string Output,
    string Error,
    OperationState State,
    TimeSpan Duration,
    bool TimedOut = false);
