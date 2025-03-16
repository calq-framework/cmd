using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd {
    public interface ITerminalLogger {
        public void Log(string script, IProcessRunConfiguration processRunConfiguration);
    }
}