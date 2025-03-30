namespace CalqFramework.Cmd.TerminalComponents {
    public interface ITerminalLogger {
        public void LogRun(string script, IShellCommandRunConfiguration runConfiguration);
    }
}