using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.TerminalComponents.Contexts {
    public class ShellCommandRunConfigurationContext : IShellCommandRunConfiguration {
        private readonly IShellCommandStartConfiguration _defaultShellCommandStartConfiguration = new ShellCommandStartConfiguration();

        private readonly AsyncLocal<TextReader> _localIn = new();
        private readonly AsyncLocal<TextWriter> _localInWriter = new();
        private readonly AsyncLocal<TextWriter> _localOut = new();
        private readonly AsyncLocal<string> _localWorkingDirectory = new();

        public TextReader In {
            get {
                _localIn.Value ??= _defaultShellCommandStartConfiguration.In;
                return _localIn.Value!;
            }
            set => _localIn.Value = value;
        }

        public TextWriter InInterceptor {
            get {
                _localInWriter.Value ??= _defaultShellCommandStartConfiguration.InInterceptor;
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
