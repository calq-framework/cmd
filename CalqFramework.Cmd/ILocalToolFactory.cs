using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd;

/// <summary>
/// Factory interface for creating underlying shells used by LocalTool
/// </summary>
public interface ILocalToolFactory
{
    /// <summary>
    /// Creates the underlying shell instance for LocalTool
    /// </summary>
    /// <returns>Shell instance that LocalTool will delegate to</returns>
    IShell CreateLocalTool();
}