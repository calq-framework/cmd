using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class LocalTerminalConfigurationContext {
        private readonly AsyncLocal<IShell> _localShell = new();
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger = new();

        public ShellCommandRunConfigurationContext ShellCommandRunConfiguration { get; } = new ShellCommandRunConfigurationContext();

        public IShell Shell {
            get {
                _localShell.Value ??= new CommandLine();
                return _localShell.Value!;
            }
            set => _localShell.Value = value;
        }

        public ITerminalLogger TerminalLogger {
            get {
                _localTerminalLogger.Value ??= new TerminalLogger();
                return _localTerminalLogger.Value!;
            }
            set => _localTerminalLogger.Value = value;
        }
    }
}
