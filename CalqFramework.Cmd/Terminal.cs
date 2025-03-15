using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd;
public static class Terminal {
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    public static void CD(string path) {
        LocalTerminal.ProcessRunConfiguration.WorkingDirectory = Path.GetFullPath(Path.Combine(LocalTerminal.ProcessRunConfiguration.WorkingDirectory, path));
    }

    public static Command CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public static Command CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new Command(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader });
    }

    public static void RUN(string script, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration), cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { Out = outputWriter }, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader, Out = outputWriter }, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration), cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { Out = outputWriter }, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader, Out = outputWriter }, cancellationToken);
    }

    public class LocalTerminalConfigurationContext {
        private readonly IShell _defaultShell;

        private readonly AsyncLocal<IShell> _localShell;

        public LocalTerminalConfigurationContext() {
            _defaultShell = new CommandLine();

            _localShell = new AsyncLocal<IShell>();
        }

        public ProcessRunConfigurationContext ProcessRunConfiguration { get; } = new ProcessRunConfigurationContext();

        public IShell Shell {
            get {
                _localShell.Value ??= _defaultShell;
                return _localShell.Value!;
            }
            set => _localShell.Value = value;
        }

        public class ProcessRunConfigurationContext : IProcessRunConfiguration {
            private readonly ProcessRunConfiguration _defaultRunConfiguration;

            private readonly AsyncLocal<ProcessErrorHandler> _localErrorHandler;
            private readonly AsyncLocal<TextReader> _localIn;
            private readonly AsyncLocal<TextWriter> _localOut;
            private readonly AsyncLocal<string> _localWorkingDirectory;

            public ProcessRunConfigurationContext() {
                _defaultRunConfiguration = new ProcessRunConfiguration();

                _localIn = new AsyncLocal<TextReader>();
                _localOut = new AsyncLocal<TextWriter>();
                _localWorkingDirectory = new AsyncLocal<string>();
                _localErrorHandler = new AsyncLocal<ProcessErrorHandler>();
            }

            public ProcessErrorHandler ErrorHandler {
                get {
                    _localErrorHandler.Value ??= _defaultRunConfiguration.ErrorHandler;
                    return _localErrorHandler.Value!;
                }
                set => _localErrorHandler.Value = value;
            }

            public TextReader In {
                get {
                    _localIn.Value ??= _defaultRunConfiguration.In;
                    return _localIn.Value!;
                }
                set => _localIn.Value = value;
            }

            public TextWriter Out {
                get {
                    _localOut.Value ??= _defaultRunConfiguration.Out;
                    return _localOut.Value!;
                }
                set => _localOut.Value = value;
            }


            public string WorkingDirectory {
                get {
                    _localWorkingDirectory.Value ??= _defaultRunConfiguration.WorkingDirectory;
                    return _localWorkingDirectory.Value!;
                }
                set => _localWorkingDirectory.Value = value;
            }
        }
    }
}
