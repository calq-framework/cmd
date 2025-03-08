namespace CalqFramework.Cmd;
public class ShellUtil {
    private readonly static IShell _defaultShell;
    private readonly static AsyncLocal<IShell> _localShell;

    public static IShell LocalShell {
        get {
            _localShell.Value ??= _defaultShell;
            return _localShell.Value!;
        }
        set => _localShell.Value = value;
    }

    static ShellUtil() {
        _defaultShell = new CommandLine();
        _localShell = new AsyncLocal<IShell>();
    }

    public static string CMD(string script, TextReader? inputReader = null) {
        return LocalShell.CMD(script, inputReader);
    }

    public static void RUN(string script, TextReader? inputReader = null) {
        LocalShell.RUN(script, inputReader);
    }

    public static void CD(string path) {
        LocalShell.CD(path);
    }
}
