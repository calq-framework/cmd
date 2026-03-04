namespace CalqFramework.Cmd.Shell;

public class ShellScriptPostprocessor : IShellScriptPostprocessor {
    public string ProcessOutput(string output) => output.TrimEnd();
}
