using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.TerminalComponents {
    public class LocalTerminalConfigurationContext {
        private readonly AsyncLocal<TextReader> _localIn = new();
        private readonly AsyncLocal<TextWriter> _localInWriter = new();
        private readonly AsyncLocal<TextWriter> _localOut = new();
        private readonly AsyncLocal<IShell> _localShell = new();
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger = new();
        private readonly AsyncLocal<string> _localWorkingDirectory = new();

        public TextReader In {
            get {
                _localIn.Value ??= Console.In;
                return _localIn.Value!;
            }
            set => _localIn.Value = value;
        }

        public TextWriter InInterceptor {
            get {
                _localInWriter.Value ??= Console.Out;
                return _localInWriter.Value!;
            }
            set => _localInWriter.Value = value;
        }

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

        public string WorkingDirectory {
            get {
                _localWorkingDirectory.Value ??= Environment.CurrentDirectory;
                return _localWorkingDirectory.Value!;
            }
            set => _localWorkingDirectory.Value = value;
        }
    }
}
