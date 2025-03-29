using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class ProcessStartConfigurationContext : IShellCommandRunConfiguration {
        private readonly IProcessStartConfiguration _defaultProcessStartConfiguration;

        private readonly AsyncLocal<IShellWorkerErrorHandler> _localErrorHandler;
        private readonly AsyncLocal<TextReader> _localIn;
        private readonly AsyncLocal<TextWriter> _localInWriter;
        private readonly AsyncLocal<TextWriter> _localOut;
        private readonly AsyncLocal<string> _localWorkingDirectory;

        public ProcessStartConfigurationContext() {
            _defaultProcessStartConfiguration = new ProcessStartConfiguration();

            _localIn = new AsyncLocal<TextReader>();
            _localInWriter = new AsyncLocal<TextWriter>();
            _localOut = new AsyncLocal<TextWriter>();
            _localWorkingDirectory = new AsyncLocal<string>();
            _localErrorHandler = new AsyncLocal<IShellWorkerErrorHandler>();
        }

        public IShellWorkerErrorHandler ErrorHandler {
            get {
                _localErrorHandler.Value ??= _defaultProcessStartConfiguration.ErrorHandler;
                return _localErrorHandler.Value!;
            }
            set => _localErrorHandler.Value = value;
        }

        public TextReader In {
            get {
                _localIn.Value ??= _defaultProcessStartConfiguration.In;
                return _localIn.Value!;
            }
            set => _localIn.Value = value;
        }

        public TextWriter InWriter {
            get {
                _localInWriter.Value ??= _defaultProcessStartConfiguration.InWriter;
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
                _localWorkingDirectory.Value ??= _defaultProcessStartConfiguration.WorkingDirectory;
                return _localWorkingDirectory.Value!;
            }
            set => _localWorkingDirectory.Value = value;
        }
    }
}
