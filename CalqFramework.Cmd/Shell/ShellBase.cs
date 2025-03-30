namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellCommandPostprocessor Postprocessor { get; init; } = new ShellCommandPostprocessor();

    public abstract string MapToInternalPath(string hostPath);
    public abstract ShellWorkerBase CreateShellWorker(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default);
}
