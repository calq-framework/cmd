using CalqFramework.Cmd.TerminalComponents;

namespace CalqFramework.Cmd;

/// <summary>
/// Provides shell-style scripting APIs for C#. Supports CMD/RUN for command execution,
/// LocalTerminal for context management, and CD/PWD for directory operations.
/// Settings are stored in AsyncLocal for thread/task isolation.
/// </summary>

public static class Terminal {
    /// <summary>
    /// Context for terminal configuration using AsyncLocal storage.
    /// Each thread/task maintains its own Shell, Out stream, and TerminalLogger.
    /// </summary>
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    /// <summary>
    /// Gets the current working directory mapped to the shell's internal path format.
    /// Automatically handles WSL path mapping on Windows.
    /// </summary>
    public static string PWD {
        get {
            ShellScript.LocalWorkingDirectory.Value ??= Environment.CurrentDirectory;
            return LocalTerminal.Shell.MapToInternalPath(ShellScript.LocalWorkingDirectory.Value!);
        }
        private set => ShellScript.LocalWorkingDirectory.Value = LocalTerminal.Shell.MapToHostPath(value);
    }

    /// <summary>
    /// Changes the current working directory. Works like Unix cd command.
    /// </summary>
    public static void CD(string path) {
        PWD = Path.GetFullPath(Path.Combine(PWD, path));
    }

    /// <summary>
    /// Executes a shell command and returns the output as a string.
    /// Trims the last newline by default.
    /// </summary>
    public static string CMD(string script, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDV(script).Evaluate(cancellationTokenSource.Token);
    }

    public static string CMD(string script, Stream? inputStream, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDV(script).Evaluate(inputStream, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Asynchronously executes a shell command and returns the output as a string.
    /// </summary>
    public static Task<string> CMDAsync(string script, CancellationToken cancellationToken = default) {
        return CMDV(script).EvaluateAsync(cancellationToken);
    }

    public static Task<string> CMDAsync(string script, Stream? inputStream, CancellationToken cancellationToken = default) {
        return CMDV(script).EvaluateAsync(inputStream, cancellationToken);
    }

    /// <summary>
    /// Creates a ShellScript object for pipeline operations and advanced control.
    /// Use with | operator for command chaining.
    /// </summary>
    public static ShellScript CMDV(string script) {
        return new ShellScript(LocalTerminal.Shell, script);
    }

    /// <summary>
    /// Executes a shell command with input/output stream handling.
    /// Reads from LocalTerminal.Shell.In and writes to LocalTerminal.Out by default.
    /// </summary>
    public static void RUN(string script, TimeSpan? timeout = null) {
        RUN(script, LocalTerminal.Shell.In, LocalTerminal.Out, timeout);
    }

    public static void RUN(string script, Stream? inputStream, TimeSpan? timeout = null) {
        RUN(script, inputStream, LocalTerminal.Out, timeout);
    }

    public static void RUN(string script, Stream? inputStream, Stream outputStream, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        ShellScript cmd = CMDV(script);
        LocalTerminal.TerminalLogger.LogRun(cmd);
        cmd.Run(inputStream, outputStream, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Asynchronously executes a shell command with stream handling.
    /// </summary>
    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.Shell.In, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, Stream? inputStream, CancellationToken cancellationToken = default) {
        await RUNAsync(script, inputStream, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
        ShellScript cmd = CMDV(script);
        LocalTerminal.TerminalLogger.LogRun(cmd);
        await cmd.RunAsync(inputStream, outputStream, cancellationToken);
    }
}
