namespace CalqFramework.Cmd.TerminalComponents {
    internal class TerminalLogger : ITerminalLogger {
        public void LogRun(ShellCommand shellCommand, TextWriter outputWriter) {
            outputWriter.WriteLine($"\nDIR: {shellCommand.WorkingDirectory}");
            if (!shellCommand.Script.Contains('\n')) {
                outputWriter.WriteLine($"RUN: {shellCommand.Script}");
            } else {
                outputWriter.WriteLine($"RUN:\n{shellCommand.Script}");
            }
        }
    }
}
