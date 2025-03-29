namespace CalqFramework.Cmd.TerminalComponents {
    internal class TerminalLogger : ITerminalLogger {
        public void Log(string script, IShellCommandRunConfiguration runConfiguration) {
            runConfiguration.Out.WriteLine($"\nDIR: {runConfiguration.WorkingDirectory}");
            if (!script.Contains('\n')) {
                runConfiguration.Out.WriteLine($"RUN: {script}");
            } else {
                runConfiguration.Out.WriteLine($"RUN:\n{script}");
            }
        }
    }
}
