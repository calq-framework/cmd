namespace CalqFramework.Cmd.Shell {

    public class CommandLineWorker(ShellScript shellScript, Stream? inputStream) : ProcessWorkerBase(shellScript, inputStream) {
        internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
            int spaceIndex = script.IndexOf(' ');
            string command = script.Substring(0, spaceIndex);
            string arguments = script.Substring(spaceIndex + 1);
            return new ProcessExecutionInfo(command, arguments);
        }
    }
}
