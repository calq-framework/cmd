using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.SystemProcess;

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
        return new Command(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { CommandProcessor = LocalTerminal.CommandProcessor }.Value;
    }

    public static Task<string> CMDAsync(string script, TimeSpan? timeout = null) {
        return CMDAsync(script, TextReader.Null, timeout);
    }

    public static Task<string> CMDAsync(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new Command(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { CommandProcessor = LocalTerminal.CommandProcessor }.GetValueAsync();
    }

    public static Command CMDV(string script, TimeSpan? timeout = null) {
        return CMDV(script, TextReader.Null, timeout);
    }

    public static Command CMDV(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new Command(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { CommandProcessor = LocalTerminal.CommandProcessor };
    }
    public static void RUN(string script, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Execute(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration), cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Execute(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { Out = outputWriter }, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Execute(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Execute(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader, Out = outputWriter }, cancellationTokenSource.Token);
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration), cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { Out = outputWriter }, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.ExecuteAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader, Out = outputWriter }, cancellationToken);
    }

    public class LocalTerminalConfigurationContext {
        private readonly ICommandProcessor _defaultCommandProcessor;
        private readonly IShell _defaultShell;
        private readonly ITerminalLogger _defaultTerminalLogger;

        private readonly AsyncLocal<ICommandProcessor> _localCommandProcessor;
        private readonly AsyncLocal<IShell> _localShell;
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger;

        public LocalTerminalConfigurationContext() {
            _defaultShell = new CommandLine();
            _defaultCommandProcessor = new CommandProcessor();
            _defaultTerminalLogger = new TerminalLogger();

            _localShell = new AsyncLocal<IShell>();
            _localCommandProcessor = new AsyncLocal<ICommandProcessor>();
            _localTerminalLogger = new AsyncLocal<ITerminalLogger>();
        }

        public ICommandProcessor CommandProcessor {
            get {
                _localCommandProcessor.Value ??= _defaultCommandProcessor;
                return _localCommandProcessor.Value!;
            }
            set => _localCommandProcessor.Value = value;
        }

        public ProcessRunConfigurationContext ProcessRunConfiguration { get; } = new ProcessRunConfigurationContext();

        public IShell Shell {
            get {
                _localShell.Value ??= _defaultShell;
                return _localShell.Value!;
            }
            set => _localShell.Value = value;
        }

        public ITerminalLogger TerminalLogger {
            get {
                _localTerminalLogger.Value ??= _defaultTerminalLogger;
                return _localTerminalLogger.Value!;
            }
            set => _localTerminalLogger.Value = value;
        }
        public class ProcessRunConfigurationContext : IProcessRunConfiguration {
            private readonly IProcessRunConfiguration _defaultProcessRunConfiguration;

            private readonly AsyncLocal<IProcessErrorHandler> _localErrorHandler;
            private readonly AsyncLocal<TextReader> _localIn;
            private readonly AsyncLocal<TextWriter> _localOut;
            private readonly AsyncLocal<string> _localWorkingDirectory;

            public ProcessRunConfigurationContext() {
                _defaultProcessRunConfiguration = new ProcessRunConfiguration();

                _localIn = new AsyncLocal<TextReader>();
                _localOut = new AsyncLocal<TextWriter>();
                _localWorkingDirectory = new AsyncLocal<string>();
                _localErrorHandler = new AsyncLocal<IProcessErrorHandler>();
            }

            public IProcessErrorHandler ErrorHandler {
                get {
                    _localErrorHandler.Value ??= _defaultProcessRunConfiguration.ErrorHandler;
                    return _localErrorHandler.Value!;
                }
                set => _localErrorHandler.Value = value;
            }

            public TextReader In {
                get {
                    _localIn.Value ??= _defaultProcessRunConfiguration.In;
                    return _localIn.Value!;
                }
                set => _localIn.Value = value;
            }

            public TextWriter Out {
                get {
                    _localOut.Value ??= _defaultProcessRunConfiguration.Out;
                    return _localOut.Value!;
                }
                set => _localOut.Value = value;
            }


            public string WorkingDirectory {
                get {
                    _localWorkingDirectory.Value ??= _defaultProcessRunConfiguration.WorkingDirectory;
                    return _localWorkingDirectory.Value!;
                }
                set => _localWorkingDirectory.Value = value;
            }
        }
    }
}
