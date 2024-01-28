namespace CalqFramework.Shell;
public class ShellUtil
{
    public static IShell Shell { get; private set; }

    static ShellUtil()
    {
        Shell = new CommandLine();
    }

    public static void SetShell(IShell shell)
    {
        Shell = shell;
    }

    public static string CMD(string script)
    {
        return Shell.CMD(script);
    }
}
