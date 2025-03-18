using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.TerminalComponents {
    public interface ITerminalLogger {
        public void Log(string script, IProcessRunConfiguration processRunConfiguration);
    }
}