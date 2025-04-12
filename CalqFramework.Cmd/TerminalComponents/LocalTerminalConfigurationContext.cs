using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.TerminalComponents {
    public class LocalTerminalConfigurationContext {
        private readonly AsyncLocal<TextWriter> _localOut = new();
        private readonly AsyncLocal<IShell> _localShell = new();
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger = new();

        public TextWriter Out {
            get {
                _localOut.Value ??= Console.Out;
                return _localOut.Value!;
            }
            set => _localOut.Value = value;
        }


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
