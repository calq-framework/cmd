namespace CalqFramework.Cmd.TerminalComponents.ShellCommandComponents {
    public class ShellCommandPostprocessor : IShellCommandPostprocessor {
        public string ProcessOutput(string output) {
            return output.TrimEnd();
        }
    }
}
