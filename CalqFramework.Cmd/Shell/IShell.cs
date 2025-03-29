namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        string MapToInternalPath(string hostPath);
        ShellWorkerBase CreateShellWorker(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default);
    }
}