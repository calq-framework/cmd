using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

public class CommandLine : ShellBase {

    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        return new CommandLineWorker(shellScript, inputStream);
    }

    public override string MapToHostPath(string internalPath) {
        return Path.GetFullPath(internalPath); ;
    }

    public override string MapToInternalPath(string hostPath) {
        return Path.GetFullPath(hostPath); ;
    }
}
