using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.TerminalComponents.Contexts;
namespace CalqFramework.Cmd;
public static class Terminal {
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    public static void CD(string path) {
        LocalTerminal.ShellCommandRunConfiguration.WorkingDirectory = Path.GetFullPath(Path.Combine(LocalTerminal.ShellCommandRunConfiguration.WorkingDirectory, path));
    }

    public static string CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public static string CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDV(script, inputReader).Evaluate(cancellationTokenSource.Token);
    }

    public static Task<string> CMDAsync(string script, CancellationToken cancellationToken = default) {
        return CMDAsync(script, TextReader.Null, cancellationToken);
    }

    public static Task<string> CMDAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        return CMDV(script, inputReader).EvaluateAsync(cancellationToken);
    }

    public static ShellCommand CMDV(string script) {
        return CMDV(script, TextReader.Null);
    }

    public static ShellCommand CMDV(string script, TextReader inputReader) {
        return new ShellCommand(LocalTerminal.Shell, script) {
            ShellCommandStartConfiguration = new ShellCommandStartConfiguration(LocalTerminal.ShellCommandRunConfiguration) {
                In = inputReader
            }
        };
    }

    public static void RUN(string script, TimeSpan? timeout = null) {
        RUN(script, LocalTerminal.ShellCommandRunConfiguration.In, LocalTerminal.ShellCommandRunConfiguration.Out, timeout);
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        RUN(script, inputReader, LocalTerminal.ShellCommandRunConfiguration.Out, timeout);
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        RUN(script, LocalTerminal.ShellCommandRunConfiguration.In, outputWriter, timeout);
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.LogRun(script, LocalTerminal.ShellCommandRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        CMDV(script, inputReader).Run(outputWriter, cancellationTokenSource.Token);
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.ShellCommandRunConfiguration.In, LocalTerminal.ShellCommandRunConfiguration.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        await RUNAsync(script, inputReader, LocalTerminal.ShellCommandRunConfiguration.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await RUNAsync(script, LocalTerminal.ShellCommandRunConfiguration.In, outputWriter, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.LogRun(script, LocalTerminal.ShellCommandRunConfiguration);
        await CMDV(script, inputReader).RunAsync(outputWriter, cancellationToken);
    }
}
