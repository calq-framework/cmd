using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.TerminalComponents {
    public interface IShellCommandRunConfiguration : IProcessStartConfiguration {
        TextWriter Out { get; }
    }
}