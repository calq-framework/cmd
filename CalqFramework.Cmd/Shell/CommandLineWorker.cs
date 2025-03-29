namespace CalqFramework.Cmd.Shell {
    public class CommandLineWorker : ShellWorkerBase {
        public CommandLineWorker(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, CancellationToken cancellationToken = default) : base(script, shellCommandStartConfiguration, cancellationToken) {
        }

        internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
            int spaceIndex = script.IndexOf(' ');
            var command = script.Substring(0, spaceIndex);
            var arguments = script.Substring(spaceIndex + 1);
            return new ProcessExecutionInfo(command, arguments);
        }
    }
}
