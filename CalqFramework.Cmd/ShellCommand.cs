using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        public ShellCommand(IShell shell, string script, IProcessRunConfiguration processRunConfiguration) { // TODO change to IProcessStartConfiguration
            Shell = shell;
            Script = script;
            ProcessRunConfiguration = processRunConfiguration;
        }

        public IShellCommandPostprocessor ShellCommandPostprocessor { get; init; } = new ShellCommandPostprocessor();
        private ShellCommand? PipedShellCommand { get; init; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.GetOutput();
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            var c = new ShellCommand(b.Shell, b.Script, b.ProcessRunConfiguration) {
                PipedShellCommand = a
            };

            return c;
        }

        public string GetOutput(CancellationToken cancellationToken = default) {
            return GetOutputAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> GetOutputAsync(CancellationToken cancellationToken = default) {
            TextReader inputReader;
            ShellWorker? pipedProcess = null;
            if (PipedShellCommand != null) {
                pipedProcess = PipedShellCommand.Start();
                inputReader = pipedProcess.StandardOutput;
            } else {
                inputReader = ProcessRunConfiguration.In;
            }
            var outWriter = new StringWriter();
            await Shell.RunAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = inputReader, Out = outWriter }, pipedProcess, cancellationToken);
            var output = outWriter.ToString();
            return ShellCommandPostprocessor.ProcessOutput(output);
        }

        public ShellWorker Start(CancellationToken cancellationToken = default) {
            TextReader inputReader;
            ShellWorker? pipedProcess = null;
            if (PipedShellCommand != null) {
                pipedProcess = PipedShellCommand.Start();
                inputReader = pipedProcess.StandardOutput;
            } else {
                inputReader = ProcessRunConfiguration.In;
            }
            return Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = inputReader }, pipedProcess, cancellationToken);
        }

        public override string ToString() {
            return GetOutput();
        }
    }
}
