using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell {
    public class CommandLineWorker : ShellWorkerBase {
        public CommandLineWorker(string script, IProcessStartConfiguration processStartConfiguration, CancellationToken cancellationToken = default) : base(script, processStartConfiguration, cancellationToken) {
        }

        internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
            int spaceIndex = script.IndexOf(' ');
            var command = script.Substring(0, spaceIndex);
            var arguments = script.Substring(spaceIndex + 1);
            return new ProcessExecutionInfo(command, arguments);
        }
    }
}
