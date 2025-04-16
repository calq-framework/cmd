namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellWorkerErrorHandler ErrorHandler { get; }
        Stream? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        IShellWorker CreateShellWorker(ShellScript shellScript, CancellationToken cancellationToken = default);
        IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default);
        string MapToInternalPath(string hostPath);
    }
}