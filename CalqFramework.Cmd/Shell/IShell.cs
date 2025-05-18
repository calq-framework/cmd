namespace CalqFramework.Cmd.Shell {

    public interface IShell {
        IShellScriptExceptionFactory ExceptionFactory { get; }
        Stream? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        IShellWorker CreateShellWorker(ShellScript shellScript);

        IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream);

        string MapToHostPath(string internalPath);

        string MapToInternalPath(string hostPath);
    }
}
