namespace CalqFramework.Cmd.TerminalComponents {

    /// <summary>
    /// Default implementation of ITerminalLogger that outputs shell script information to a TextWriter.
    /// Formats single-line commands as "RUN: command" and multi-line commands with a newline separator.
    /// </summary>
    public class TerminalLogger : ITerminalLogger {
        
        /// <summary>
        /// Gets or sets the TextWriter where log output will be written.
        /// Defaults to Console.Out for standard console output.
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
