namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Process worker for command line execution (cmd.exe on Windows, shell on Unix).
    /// Handles command parsing and process creation for basic shell commands.
    /// </summary>

    public class CommandLineWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) : ProcessWorkerBase(shellScript, inputStream, disposeOnCompletion) {
        internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
            int spaceIndex = script.IndexOf(' ');
            if (spaceIndex == -1) {
                // Single-word command with no arguments
                return new ProcessExecutionInfo(script, "");
            }
            
            string command = script.Substring(0, spaceIndex);
            string arguments = script.Substring(spaceIndex + 1);
            return new ProcessExecutionInfo(command, arguments);
        }
    }
}
