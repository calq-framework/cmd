namespace CalqFramework.Shell;
public class ShellUtil {
    public static IShell Shell { get; private set; }

    static ShellUtil() {
        Shell = new CommandLine();
    }

    public static void SetShell(IShell shell) {
        Shell = shell;
    }

    public static string CMD(string script, TextReader? inputReader = null) {
        return Shell.CMD(script, inputReader);
    }

    public static void RUN(string script, TextReader? inputReader = null) {
        Shell.RUN(script, inputReader);
    }

    public static void CD(string path) {
        Shell.CD(path);
    }

    public static string GetLocalPath(string path) {
        return Shell.GetLocalPath(path);
    }
}
