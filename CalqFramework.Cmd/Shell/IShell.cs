namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellWorkerErrorHandler ErrorHandler { get; }
        TextReader? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        IShellWorker CreateShellWorker(ShellScript shellScript, CancellationToken cancellationToken = default);
        IShellWorker CreateShellWorker(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default);
        string MapToInternalPath(string hostPath);
    }
}