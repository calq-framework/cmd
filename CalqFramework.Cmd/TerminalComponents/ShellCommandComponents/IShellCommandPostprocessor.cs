namespace CalqFramework.Cmd.TerminalComponents.ShellCommandComponents {
    public interface IShellCommandPostprocessor {
        string ProcessOutput(string output);
    }
}