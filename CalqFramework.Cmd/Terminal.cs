using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd;
public static class Terminal {
    private readonly static IShell _defaultShell;
    private readonly static AsyncLocal<IShell> _localShell;

    static Terminal() {
        _defaultShell = new CommandLine();
        _localShell = new AsyncLocal<IShell>();
    }

    public static IShell LocalShell {
        get {
            _localShell.Value ??= _defaultShell;
            return _localShell.Value!;
        }
        set => _localShell.Value = value;
    }
    public static void CD(string path) {
        LocalShell.CurrentDirectory = Path.GetFullPath(Path.Combine(LocalShell.CurrentDirectory, path));
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
        await LocalShell.ExecuteAsync(script, inputReader, output, cancellationToken);
        return output.ToString();
    }

    public static void RUN(string script, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalShell.ExecuteAsync(script, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }
    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalShell.ExecuteAsync(script, outputWriter, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalShell.ExecuteAsync(script, inputReader, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalShell.ExecuteAsync(script, inputReader, outputWriter, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        await LocalShell.ExecuteAsync(script, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        await LocalShell.ExecuteAsync(script, inputReader, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await LocalShell.ExecuteAsync(script, outputWriter, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        await LocalShell.ExecuteAsync(script, inputReader, outputWriter, cancellationToken);
    }
}
