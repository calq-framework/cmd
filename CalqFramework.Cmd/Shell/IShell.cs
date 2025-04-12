namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellWorkerErrorHandler ErrorHandler { get; }
        TextReader? In { get; }
        IShellCommandPostprocessor Postprocessor { get; }

        ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, CancellationToken cancellationToken = default);
        ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, TextReader? inputReader, CancellationToken cancellationToken = default);
        string MapToInternalPath(string hostPath);
    }
}