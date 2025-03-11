using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd;
public static class Terminal {

    public static LocalTerminalContext LocalTerminal { get; } = new LocalTerminalContext();

    public static void CD(string path) {
        LocalTerminal.WorkingDirectory = Path.GetFullPath(Path.Combine(LocalTerminal.WorkingDirectory, path));
    }

    public static string CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public static string CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDAsync(script, inputReader, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task<string> CMDAsync(string script, CancellationToken cancellationToken = default) {
        return await CMDAsync(script, TextReader.Null, cancellationToken);
    }

    public static async Task<string> CMDAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        var output = new StringWriter();
        await LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, inputReader, output, cancellationToken);
        return output.ToString();
    }




    public static Command CMDP(string script) {
        return new Command(LocalTerminal.Shell, LocalTerminal.WorkingDirectory, script, LocalTerminal.In);
    }




    public static void RUN(string script, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, LocalTerminal.In, LocalTerminal.Out, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, LocalTerminal.In, outputWriter, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, inputReader, LocalTerminal.Out, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, inputReader, outputWriter, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, LocalTerminal.In, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, inputReader, LocalTerminal.Out, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, LocalTerminal.In, outputWriter, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await LocalTerminal.Shell.ExecuteAsync(LocalTerminal.WorkingDirectory, script, inputReader, outputWriter, cancellationToken);
    }

    public class LocalTerminalContext {
        private readonly TextReader _defaultIn;
        private readonly TextWriter _defaultOut;
        private readonly IShell _defaultShell;
        private readonly string _defaultWorkingDirectory;

        private readonly AsyncLocal<TextReader> _localIn;
        private readonly AsyncLocal<TextWriter> _localOut;
        private readonly AsyncLocal<IShell> _localShell;
        private readonly AsyncLocal<string> _localWorkingDirectory;

        public LocalTerminalContext() {
            _defaultIn = Console.In;
            _defaultOut = Console.Out;
            _defaultShell = new CommandLine();
            _defaultWorkingDirectory = Environment.CurrentDirectory;
            _localIn = new AsyncLocal<TextReader>();
            _localOut = new AsyncLocal<TextWriter>();
            _localShell = new AsyncLocal<IShell>();
            _localWorkingDirectory = new AsyncLocal<string>();
        }

        public TextReader In {
            get {
                _localIn.Value ??= _defaultIn;
                return _localIn.Value!;
            }
            set => _localIn.Value = value;
        }

        public TextWriter Out {
            get {
                _localOut.Value ??= _defaultOut;
                return _localOut.Value!;
            }
            set => _localOut.Value = value;
        }

        public IShell Shell {
            get {
                _localShell.Value ??= _defaultShell;
                return _localShell.Value!;
            }
            set => _localShell.Value = value;
        }

        public string WorkingDirectory {
            get {
                _localWorkingDirectory.Value ??= _defaultWorkingDirectory;
                return _localWorkingDirectory.Value!;
            }
            set => _localWorkingDirectory.Value = value;
        }
    }
}
