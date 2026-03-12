namespace CalqFramework.Cmd.ShellComponents;

public class ShellScriptPostprocessor : IShellScriptPostprocessor {
    public string ProcessOutput(string output) => output.TrimEnd();
}
