namespace CalqFramework.Cmd.Shell {

    public class CommandLineWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) : ProcessWorkerBase(shellScript, inputStream, disposeOnCompletion) {
        internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
            int spaceIndex = script.IndexOf(' ');
            string command = script.Substring(0, spaceIndex);
            string arguments = script.Substring(spaceIndex + 1);
            return new ProcessExecutionInfo(command, arguments);
        }
    }
}
