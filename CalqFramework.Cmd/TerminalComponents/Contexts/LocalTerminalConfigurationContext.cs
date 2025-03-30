using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class LocalTerminalConfigurationContext {
        private readonly IShell _defaultShell;
        private readonly ITerminalLogger _defaultTerminalLogger;

        private readonly AsyncLocal<IShell> _localShell;
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger;

        public LocalTerminalConfigurationContext() {
            _defaultShell = new CommandLine();
            _defaultTerminalLogger = new TerminalLogger();

            _localShell = new AsyncLocal<IShell>();
            _localTerminalLogger = new AsyncLocal<ITerminalLogger>();
        }

        public ShellCommandRunConfigurationContext ShellCommandRunConfiguration { get; } = new ShellCommandRunConfigurationContext();

        public IShell Shell {
            get {
                _localShell.Value ??= _defaultShell;
                return _localShell.Value!;
            }
            set => _localShell.Value = value;
        }

        public ITerminalLogger TerminalLogger {
            get {
                _localTerminalLogger.Value ??= _defaultTerminalLogger;
                return _localTerminalLogger.Value!;
            }
            set => _localTerminalLogger.Value = value;
        }
    }
}
