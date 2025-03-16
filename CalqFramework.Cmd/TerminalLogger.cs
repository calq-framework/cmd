using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd {
    internal class TerminalLogger : ITerminalLogger {
        public void Log(string script, IProcessRunConfiguration processRunConfiguration) {
            processRunConfiguration.Out.WriteLine($"\nDIR: {processRunConfiguration.WorkingDirectory}");
            if (!script.Contains('\n')) {
                processRunConfiguration.Out.WriteLine($"RUN: {script}");
            } else {
                processRunConfiguration.Out.WriteLine($"RUN:\n{script}");
            }
        }
    }
}