namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellScriptExceptionFactory ExceptionFactory { get; }
        Stream? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        IShellWorker CreateShellWorker(ShellScript shellScript);
        IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream);
        string MapToInternalPath(string hostPath);
        string MapToHostPath(string internalPth);
    }
}