using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class CommandLine : ShellBase {
    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default) {
        return new CommandLineWorker(shellScript, inputReader, cancellationToken);
    }
}
