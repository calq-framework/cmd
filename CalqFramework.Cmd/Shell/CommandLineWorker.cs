
namespace CalqFramework.Cmd.Shell {
    public class CommandLineWorker : ProcessWorkerBase {
        public CommandLineWorker(ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default) : base(shellScript, inputStream, cancellationToken) {
        }

        internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
            int spaceIndex = script.IndexOf(' ');
            var command = script.Substring(0, spaceIndex);
            var arguments = script.Substring(spaceIndex + 1);
            return new ProcessExecutionInfo(command, arguments);
        }
    }
}
