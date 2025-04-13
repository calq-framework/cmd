using CalqFramework.Cmd.TerminalComponents;

namespace CalqFramework.Cmd;
public static class Terminal {
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    public static string PWD {
        get {
            ShellScript.LocalWorkingDirectory.Value ??= Environment.CurrentDirectory;
            return ShellScript.LocalWorkingDirectory.Value!;
        }
        private set => ShellScript.LocalWorkingDirectory.Value = value;
    }

    public static void CD(string path) {
        PWD = Path.GetFullPath(Path.Combine(PWD, path));
    }

    public static string CMD(string script, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDV(script).Evaluate(cancellationTokenSource.Token);
    }

    public static string CMD(string script, TextReader? inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDV(script).Evaluate(inputReader, cancellationTokenSource.Token);
    }

    public static Task<string> CMDAsync(string script, CancellationToken cancellationToken = default) {
        return CMDV(script).EvaluateAsync(cancellationToken);
    }

    public static Task<string> CMDAsync(string script, TextReader? inputReader, CancellationToken cancellationToken = default) {
        return CMDV(script).EvaluateAsync(inputReader, cancellationToken);
    }

    public static ShellScript CMDV(string script) {
        return new ShellScript(LocalTerminal.Shell, script);
    }

    public static void RUN(string script, TimeSpan? timeout = null) {
        RUN(script, LocalTerminal.Shell.In, LocalTerminal.Out, timeout);
    }

    public static void RUN(string script, TextReader? inputReader, TimeSpan? timeout = null) {
        RUN(script, inputReader, LocalTerminal.Out, timeout);
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        RUN(script, null, outputWriter, timeout);
    }

    public static void RUN(string script, TextReader? inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        var cmd = CMDV(script);
        LocalTerminal.TerminalLogger.LogRun(cmd, outputWriter);
        cmd.Run(inputReader, outputWriter, cancellationTokenSource.Token);
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.Shell.In, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader? inputReader, CancellationToken cancellationToken = default) {
        await RUNAsync(script, inputReader, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.Shell.In, outputWriter, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader? inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        var cmd = CMDV(script);
        LocalTerminal.TerminalLogger.LogRun(cmd, outputWriter);
        await cmd.RunAsync(inputReader, outputWriter, cancellationToken);
    }
}
