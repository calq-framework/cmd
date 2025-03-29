using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.TerminalComponents {
    public interface IShellCommandRunConfiguration : IShellCommandStartConfiguration {
        TextWriter Out { get; }
    }
}