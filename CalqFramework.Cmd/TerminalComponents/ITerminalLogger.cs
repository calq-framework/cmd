namespace CalqFramework.Cmd.TerminalComponents {
    public interface ITerminalLogger {
        public void Log(string script, IShellCommandRunConfiguration runConfiguration);
    }
}