using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        private readonly SemaphoreSlim _outputSemaphore = new SemaphoreSlim(1, 1);

        private volatile string? _output;

        public ShellCommand(IShell shell, string script, IProcessRunConfiguration processRunConfiguration) { // TODO change to IProcessStartConfiguration
            Shell = shell;
            Script = script;
            ProcessRunConfiguration = processRunConfiguration;
            Out = new StringWriter();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Output {
            get {
                return GetOutputAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public IShellCommandPostprocessor ShellCommandPostprocessor { get; init; } = new ShellCommandPostprocessor();
        private StringWriter Out { get; }
        private ShellCommand? PipedShellCommand { get; init; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.Output;
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            Debug.Assert(b._output == null);

            var c = new ShellCommand(b.Shell, b.Script, b.ProcessRunConfiguration) {
                PipedShellCommand = a
            };

            return c;
        }

        public async Task<string> GetOutputAsync(CancellationToken cancellationToken = default) {
            var localOutput = _output;
            if (localOutput == null) {
                await _outputSemaphore.WaitAsync();
                try {
                    localOutput = _output;
                    if (localOutput == null) {
                        TextReader inputReader;
                        ShellWorker? pipedProcess = null;
                        if (PipedShellCommand != null) {
                            if (PipedShellCommand._output != null) {
                                inputReader = new StringReader(PipedShellCommand._output);
                            } else {
                                pipedProcess = PipedShellCommand.Start();
                                inputReader = pipedProcess.StandardOutput;
                            }
                        } else {
                            inputReader = ProcessRunConfiguration.In;
                        }
                        await Shell.RunAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = inputReader, Out = Out }, pipedProcess, cancellationToken);
                        _output = localOutput = Out.ToString();
                    }
                } finally {
                    _outputSemaphore.Release();
                }
            }
            return ShellCommandPostprocessor.ProcessOutput(localOutput);
        }

        public ShellWorker Start(CancellationToken cancellationToken = default) {
            TextReader inputReader;
            ShellWorker? pipedProcess = null;
            if (PipedShellCommand != null) {
                if (PipedShellCommand._output != null) {
                    inputReader = new StringReader(PipedShellCommand._output);
                } else {
                    pipedProcess = PipedShellCommand.Start();
                    inputReader = pipedProcess.StandardOutput;
                }
            } else {
                inputReader = ProcessRunConfiguration.In;
            }
            return Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = inputReader }, pipedProcess, cancellationToken);
        }

        public override string ToString() {
            return Output;
        }
    }
}
