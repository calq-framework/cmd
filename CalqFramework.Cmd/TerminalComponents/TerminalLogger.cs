namespace CalqFramework.Cmd.TerminalComponents {

    public class TerminalLogger : ITerminalLogger {
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
