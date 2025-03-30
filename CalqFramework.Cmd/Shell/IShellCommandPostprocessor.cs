namespace CalqFramework.Cmd.Shell {
    public interface IShellCommandPostprocessor {
        string ProcessOutput(string output);
    }
}