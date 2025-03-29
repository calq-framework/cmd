using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class ShellCommandRunConfigurationContext : IShellCommandRunConfiguration {
        private readonly IShellCommandStartConfiguration _defaultShellCommandStartConfiguration;

        private readonly AsyncLocal<IShellWorkerErrorHandler> _localErrorHandler;
        private readonly AsyncLocal<TextReader> _localIn;
        private readonly AsyncLocal<TextWriter> _localInWriter;
        private readonly AsyncLocal<TextWriter> _localOut;
        private readonly AsyncLocal<string> _localWorkingDirectory;

        public ShellCommandRunConfigurationContext() {
            _defaultShellCommandStartConfiguration = new ShellCommandStartConfiguration();

            _localIn = new AsyncLocal<TextReader>();
            _localInWriter = new AsyncLocal<TextWriter>();
            _localOut = new AsyncLocal<TextWriter>();
            _localWorkingDirectory = new AsyncLocal<string>();
            _localErrorHandler = new AsyncLocal<IShellWorkerErrorHandler>();
        }

        public IShellWorkerErrorHandler ErrorHandler {
            get {
                _localErrorHandler.Value ??= _defaultShellCommandStartConfiguration.ErrorHandler;
                return _localErrorHandler.Value!;
            }
            set => _localErrorHandler.Value = value;
        }

        public TextReader In {
            get {
                _localIn.Value ??= _defaultShellCommandStartConfiguration.In;
                return _localIn.Value!;
            }
            set => _localIn.Value = value;
        }

        public TextWriter InWriter {
            get {
                _localInWriter.Value ??= _defaultShellCommandStartConfiguration.InWriter;
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


        public string WorkingDirectory {
            get {
                _localWorkingDirectory.Value ??= _defaultShellCommandStartConfiguration.WorkingDirectory;
                return _localWorkingDirectory.Value!;
            }
            set => _localWorkingDirectory.Value = value;
        }
    }
}
