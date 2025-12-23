namespace CalqFramework.Cmd.Shell {

    public interface IShell {
        IShellScriptExceptionFactory ExceptionFactory { get; }
        Stream? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        IShellWorker CreateShellWorker(ShellScript shellScript, bool disposeOnCompletion = true);

        IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true);

        string MapToHostPath(string internalPath);

        string MapToInternalPath(string hostPath);
    }
}
