namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellScriptExceptionFactory ExceptionFactory { get; init; } = new ShellScriptExceptionFactory();
    public Stream? In { get; init; } = null;
    public IShellScriptPostprocessor Postprocessor { get; init; } = new ShellScriptPostprocessor();

    public IShellWorker CreateShellWorker(ShellScript shellScript, bool disposeOnCompletion = true) {
        return CreateShellWorker(shellScript, In, disposeOnCompletion);
    }

    public abstract IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true);

    public abstract string MapToHostPath(string internalPath);

    public abstract string MapToInternalPath(string hostPath);
}
