namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellScriptExceptionFactory ExceptionFactory { get; init; } = new ShellScriptExceptionFactory();
    public Stream? In { get; init; } = Console.OpenStandardInput();
    public IShellScriptPostprocessor Postprocessor { get; init; } = new ShellScriptPostprocessor();

    public IShellWorker CreateShellWorker(ShellScript shellScript) {
        return CreateShellWorker(shellScript, this.In);
    }

    public abstract IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream);

    public abstract string MapToHostPath(string internalPth);

    public abstract string MapToInternalPath(string hostPath);
}
