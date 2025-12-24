using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

/// <summary>
/// Command line shell implementation for Windows cmd.exe or Unix shell.
/// Provides basic process execution without special path mapping.
/// </summary>

public class CommandLine : ShellBase {

    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) {
        return new CommandLineWorker(shellScript, inputStream, disposeOnCompletion);
    }

    public override string MapToHostPath(string internalPath) {
        return Path.GetFullPath(internalPath); ;
    }

    public override string MapToInternalPath(string hostPath) {
        return Path.GetFullPath(hostPath); ;
    }
}
