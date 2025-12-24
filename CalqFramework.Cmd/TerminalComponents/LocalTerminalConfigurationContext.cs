using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.TerminalComponents {

    /// <summary>
    /// Manages terminal configuration using AsyncLocal storage for thread/task isolation.
    /// Each logical context maintains its own Shell, output stream, and logger settings.
    /// </summary>

    public class LocalTerminalConfigurationContext {
        private readonly AsyncLocal<Stream> _localOut = new();
        private readonly AsyncLocal<IShell> _localShell = new();
        private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger = new();

        /// <summary>
        /// Output stream for terminal operations. Defaults to Console.OpenStandardOutput().
        /// </summary>
        public Stream Out {
            get {
                _localOut.Value ??= Console.OpenStandardOutput();
                return _localOut.Value!;
            }
            set => _localOut.Value = value;
        }

        /// <summary>
        /// Shell implementation for command execution. Defaults to CommandLine shell.
        /// Can be set to Bash, PythonTool, HttpTool, or ShellTool.
        /// </summary>
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

        /// <summary>
        /// Host's absolute path of the current working directory.
        /// Mapped to shell's internal path format via PWD property.
        /// </summary>
        public static string WorkingDirectory {
            get {
                return ShellScript.LocalWorkingDirectory.Value!;
            }
            set => ShellScript.LocalWorkingDirectory.Value = value;
        }
    }
}
