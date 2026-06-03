using CalqFramework.Cmd.ShellComponents;

namespace CalqFramework.Cmd.Shell;

/// <summary>
///     Base class for transparent shell decorators that delegate all operations
///     to an inner shell while adding cross-cutting behavior.
///     No virtual methods — path delegation is correct by construction.
///     Only CreateShellWorker is abstract (the interception point).
/// </summary>
public abstract class ShellDecoratorBase(IShell inner) : IShell {
    protected IShell Inner { get; } = inner;

    public IShellScriptExceptionFactory ExceptionFactory => Inner.ExceptionFactory;

    public Stream? In => Inner.In;

    public IShellScriptPostprocessor Postprocessor => Inner.Postprocessor;

    public IShellWorker CreateShellWorker(ShellScript shellScript, bool disposeOnCompletion = true) =>
        CreateShellWorker(shellScript, In, disposeOnCompletion);

    public abstract IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true);

    public string MapToHostPath(string internalPath) => Inner.MapToHostPath(internalPath);

    public string MapToInternalPath(string hostPath) => Inner.MapToInternalPath(hostPath);
}
