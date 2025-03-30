namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellCommandPostprocessor Postprocessor { get; }

        string MapToInternalPath(string hostPath);
        ShellWorkerBase CreateShellWorker(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default);
    }
}