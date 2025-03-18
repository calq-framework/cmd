using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class LocalTerminalConfigurationContext {
        private readonly IShellCommandPostprocessor _defaultShellCommandPostprocessor;
        private readonly IShell _defaultShell;
        private readonly ITerminalLogger _defaultTerminalLogger;

        private readonly AsyncLocal<IShellCommandPostprocessor> _localShellCommandPostprocessor;
        private readonly AsyncLocal<IShell> _localShell;
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger;

        public LocalTerminalConfigurationContext() {
            _defaultShell = new CommandLine();
            _defaultShellCommandPostprocessor = new ShellCommandPostprocessor();
            _defaultTerminalLogger = new TerminalLogger();

            _localShell = new AsyncLocal<IShell>();
            _localShellCommandPostprocessor = new AsyncLocal<IShellCommandPostprocessor>();
            _localTerminalLogger = new AsyncLocal<ITerminalLogger>();
        }

        public IShellCommandPostprocessor ShellCommandPostprocessor {
            get {
                _localShellCommandPostprocessor.Value ??= _defaultShellCommandPostprocessor;
                return _localShellCommandPostprocessor.Value!;
            }
            set => _localShellCommandPostprocessor.Value = value;
        }

        public ProcessRunConfigurationContext ProcessRunConfiguration { get; } = new ProcessRunConfigurationContext();

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
