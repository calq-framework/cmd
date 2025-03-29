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
        return new ShellCommand(LocalTerminal.Shell, script) {
            ShellCommandStartConfiguration = new ShellCommandStartConfiguration(LocalTerminal.ShellCommandRunConfiguration) {
                In = inputReader
            },
            ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor
        }.GetOutput();
    }

    public static Task<string> CMDAsync(string script, TimeSpan? timeout = null) {
        return CMDAsync(script, TextReader.Null, timeout);
    }

    public static Task<string> CMDAsync(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script) {
            ShellCommandStartConfiguration = new ShellCommandStartConfiguration(LocalTerminal.ShellCommandRunConfiguration) {
                In = inputReader
            },
            ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor
        }.GetOutputAsync();
    }

    public static ShellCommand CMDV(string script, TimeSpan? timeout = null) {
        return CMDV(script, TextReader.Null, timeout);
    }

    public static ShellCommand CMDV(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script) {
            ShellCommandStartConfiguration = new ShellCommandStartConfiguration(LocalTerminal.ShellCommandRunConfiguration) {
                In = inputReader
            },
            ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor
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
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ShellCommandRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        new ShellCommand(LocalTerminal.Shell, script) {
            ShellCommandStartConfiguration = new ShellCommandStartConfiguration(LocalTerminal.ShellCommandRunConfiguration) {
                In = inputReader
            }
        }.Run(outputWriter, cancellationTokenSource.Token);
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
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ShellCommandRunConfiguration);
        await new ShellCommand(LocalTerminal.Shell, script) {
            ShellCommandStartConfiguration = new ShellCommandStartConfiguration(LocalTerminal.ShellCommandRunConfiguration) {
                In = inputReader
            }
        }.RunAsync(outputWriter, cancellationToken);
    }
}