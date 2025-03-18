using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class ProcessRunConfigurationContext : IProcessRunConfiguration {
        private readonly IProcessRunConfiguration _defaultProcessRunConfiguration;

        private readonly AsyncLocal<IProcessErrorHandler> _localErrorHandler;
        private readonly AsyncLocal<TextReader> _localIn;
        private readonly AsyncLocal<TextWriter> _localInWriter;
        private readonly AsyncLocal<TextWriter> _localOut;
        private readonly AsyncLocal<string> _localWorkingDirectory;

        public ProcessRunConfigurationContext() {
            _defaultProcessRunConfiguration = new ProcessRunConfiguration();

            _localIn = new AsyncLocal<TextReader>();
            _localInWriter = new AsyncLocal<TextWriter>();
            _localOut = new AsyncLocal<TextWriter>();
            _localWorkingDirectory = new AsyncLocal<string>();
            _localErrorHandler = new AsyncLocal<IProcessErrorHandler>();
        }

        public IProcessErrorHandler ErrorHandler {
            get {
                _localErrorHandler.Value ??= _defaultProcessRunConfiguration.ErrorHandler;
                return _localErrorHandler.Value!;
            }
            set => _localErrorHandler.Value = value;
        }

        public TextReader In {
            get {
                _localIn.Value ??= _defaultProcessRunConfiguration.In;
                return _localIn.Value!;
            }
            set => _localIn.Value = value;
        }

        public TextWriter InWriter {
            get {
                _localInWriter.Value ??= _defaultProcessRunConfiguration.InWriter;
                return _localInWriter.Value!;
            }
            set => _localInWriter.Value = value;
        }

        public TextWriter Out {
            get {
                _localOut.Value ??= _defaultProcessRunConfiguration.Out;
                return _localOut.Value!;
            }
            set => _localOut.Value = value;
        }


        public string WorkingDirectory {
            get {
                _localWorkingDirectory.Value ??= _defaultProcessRunConfiguration.WorkingDirectory;
                return _localWorkingDirectory.Value!;
            }
            set => _localWorkingDirectory.Value = value;
        }
    }
}
