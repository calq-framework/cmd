namespace CalqFramework.Cmd.Shell {
    public interface IShellScriptPostprocessor {
        string ProcessOutput(string output);
    }
}