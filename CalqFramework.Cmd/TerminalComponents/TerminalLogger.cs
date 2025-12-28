namespace CalqFramework.Cmd.TerminalComponents {

    /// <summary>
    /// Default implementation of ITerminalLogger that outputs to a TextWriter.
    /// Formats single-line commands as "RUN: command" and multi-line commands with a newline separator.
    /// </summary>
    public class TerminalLogger : ITerminalLogger {
        
        /// <summary>
        /// TextWriter where log output will be written. Defaults to Console.Out.
        /// </summary>
        public TextWriter Out { get; init; } = Console.Out;

        public void Log(ShellScript shellScript) {
            if (!shellScript.Script.Contains('\n')) {
                Out.WriteLine($"RUN: {shellScript.Script}");
            } else {
                Out.WriteLine($"RUN:\n{shellScript.Script}");
            }
        }
    }
}
