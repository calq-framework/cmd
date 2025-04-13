namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellWorkerErrorHandler ErrorHandler { get; }
        TextReader? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        ShellWorkerBase CreateShellWorker(ShellScript shellScript, CancellationToken cancellationToken = default);
        ShellWorkerBase CreateShellWorker(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default);
        string MapToInternalPath(string hostPath);
    }
}