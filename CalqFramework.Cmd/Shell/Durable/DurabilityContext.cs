namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     AsyncLocal context for distributed propagation.
///     Set by RecordingShellWorker.InitializeAsync (client-side) and
///     LocalTerminalFilter (server-side, from incoming headers).
/// </summary>
public static class DurabilityContext {
    public static AsyncLocal<DurableWorkflowContext?> Current { get; } = new();
}
