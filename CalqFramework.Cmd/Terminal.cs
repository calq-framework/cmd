using CalqFramework.Cmd.TerminalComponents;

namespace CalqFramework.Cmd;
public static class Terminal {
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    public static void CD(string path) {
        LocalTerminal.WorkingDirectory = Path.GetFullPath(Path.Combine(LocalTerminal.WorkingDirectory, path));
    }

    public static string CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public static string CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDV(script).Evaluate(inputReader, cancellationTokenSource.Token);
    }

    public static Task<string> CMDAsync(string script, CancellationToken cancellationToken = default) {
        return CMDAsync(script, TextReader.Null, cancellationToken);
    }

    public static Task<string> CMDAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        return CMDV(script).EvaluateAsync(inputReader, cancellationToken);
    }

    public static ShellCommand CMDV(string script) {
        return new ShellCommand(LocalTerminal.Shell, script);
    }

    public static void RUN(string script, TimeSpan? timeout = null) {
        RUN(script, LocalTerminal.Shell.In, LocalTerminal.Out, timeout);
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        RUN(script, inputReader, LocalTerminal.Out, timeout);
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        RUN(script, LocalTerminal.Shell.In, outputWriter, timeout);
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        var cmd = CMDV(script);
        LocalTerminal.TerminalLogger.LogRun(cmd, outputWriter);
        cmd.Run(inputReader, outputWriter, cancellationTokenSource.Token);
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.Shell.In, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        await RUNAsync(script, inputReader, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.Shell.In, outputWriter, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        var cmd = CMDV(script);
        LocalTerminal.TerminalLogger.LogRun(cmd, outputWriter);
        await cmd.RunAsync(inputReader, outputWriter, cancellationToken);
    }
}
