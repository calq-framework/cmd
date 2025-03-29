using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.Contexts;

namespace CalqFramework.Cmd;
public static class Terminal {
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    public static void CD(string path) {
        LocalTerminal.ProcessRunConfiguration.WorkingDirectory = Path.GetFullPath(Path.Combine(LocalTerminal.ProcessRunConfiguration.WorkingDirectory, path));
    }

    public static string CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public static string CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor }.GetOutput();
    }

    public static Task<string> CMDAsync(string script, TimeSpan? timeout = null) {
        return CMDAsync(script, TextReader.Null, timeout);
    }

    public static Task<string> CMDAsync(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor }.GetOutputAsync();
    }

    public static ShellCommand CMDV(string script, TimeSpan? timeout = null) {
        return CMDV(script, TextReader.Null, timeout);
    }

    public static ShellCommand CMDV(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor };
    }
    public static void RUN(string script, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration)).Run(LocalTerminal.ProcessRunConfiguration.Out, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }).Run(LocalTerminal.ProcessRunConfiguration.Out, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration)).Run(outputWriter, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }).Run(outputWriter, cancellationTokenSource.Token);
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration)).RunAsync(LocalTerminal.ProcessRunConfiguration.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }).RunAsync(LocalTerminal.ProcessRunConfiguration.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration)).RunAsync(outputWriter, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }).RunAsync(outputWriter, cancellationToken);
    }
}
