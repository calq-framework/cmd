namespace CalqFramework.Cmd.TerminalComponents {
    internal class TerminalLogger : ITerminalLogger {
        public void LogRun(ShellScript shellScript, TextWriter outputWriter) {
            outputWriter.WriteLine($"\nDIR: {shellScript.WorkingDirectory}");
            if (!shellScript.Script.Contains('\n')) {
                outputWriter.WriteLine($"RUN: {shellScript.Script}");
            } else {
                outputWriter.WriteLine($"RUN:\n{shellScript.Script}");
            }
        }
    }
}
