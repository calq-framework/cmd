namespace CalqFramework.Cmd.TerminalComponents {
    public interface ITerminalLogger {
        public void LogRun(ShellCommand shellCommand, TextWriter outputWriter);
    }
}