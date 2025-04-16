namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellWorkerErrorHandler ErrorHandler { get; init; } = new ShellWorkerErrorHandler();
    public Stream? In { get; init; } = Console.OpenStandardInput();
    public IShellScriptPostprocessor Postprocessor { get; init; } = new ShellScriptPostprocessor();

    public IShellWorker CreateShellWorker(ShellScript shellScript, CancellationToken cancellationToken = default) {
        return CreateShellWorker(shellScript, this.In, cancellationToken);
    }

    public abstract IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default);
    public abstract string MapToInternalPath(string hostPath);
}
