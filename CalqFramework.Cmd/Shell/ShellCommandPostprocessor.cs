namespace CalqFramework.Cmd.Shell {
    public class ShellCommandPostprocessor : IShellCommandPostprocessor {
        public string ProcessOutput(string output) {
            return output.TrimEnd();
        }
    }
}
