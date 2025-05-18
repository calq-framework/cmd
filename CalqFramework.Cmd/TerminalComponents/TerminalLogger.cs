namespace CalqFramework.Cmd.TerminalComponents {

    internal class TerminalLogger : ITerminalLogger {
        public TextWriter Out { get; init; } = Console.Out;

        public void LogRun(ShellScript shellScript) {
            Out.WriteLine($"\nDIR: {shellScript.WorkingDirectory}");
            if (!shellScript.Script.Contains('\n')) {
                Out.WriteLine($"RUN: {shellScript.Script}");
            } else {
                Out.WriteLine($"RUN:\n{shellScript.Script}");
            }
        }
    }
}