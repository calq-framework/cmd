using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CalqFramework.Cmd.AspNetCore")]

namespace CalqFramework.Cmd.Shells;

/// <summary>
/// Local tool shell that delegates to factory-created underlying shell.
/// </summary>
public class LocalTool : ShellBase
{
    private readonly IShell _underlyingShell;
    
    internal static ILocalToolFactory Factory { get; set; } = new LocalToolFactory();
    
    public LocalTool()
    {
        _underlyingShell = Factory.CreateLocalTool();
    }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true)
    {
        return _underlyingShell.CreateShellWorker(shellScript, inputStream, disposeOnCompletion);
    }

    public override string MapToHostPath(string internalPath)
    {
        return _underlyingShell.MapToHostPath(internalPath);
    }

    public override string MapToInternalPath(string hostPath)
    {
        return _underlyingShell.MapToInternalPath(hostPath);
    }
}