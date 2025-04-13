namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellWorkerErrorHandler ErrorHandler { get; init; } = new ShellWorkerErrorHandler();
    public TextReader? In { get; init; } = Console.In;
    public IShellScriptPostprocessor Postprocessor { get; init; } = new ShellScriptPostprocessor();

    public ShellWorkerBase CreateShellWorker(ShellScript shellScript, CancellationToken cancellationToken = default) {
        return CreateShellWorker(shellScript, this.In, cancellationToken);
    }

    public abstract ShellWorkerBase CreateShellWorker(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default);
    public abstract string MapToInternalPath(string hostPath);
}
