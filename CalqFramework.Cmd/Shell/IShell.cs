namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        public IShellWorkerErrorHandler ErrorHandler { get; }
        IShellCommandPostprocessor Postprocessor { get; }

        ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, CancellationToken cancellationToken = default);

        string MapToInternalPath(string hostPath);
    }
}