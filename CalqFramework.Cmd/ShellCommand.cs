using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        private readonly SemaphoreSlim _hasStartedSemaphore = new SemaphoreSlim(1, 1);

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
        /// <summary>
        /// Disposed on WaitForExitAsync().
        /// </summary>
        private Process? AssociatedProcess { get; set; }
        private bool HasStarted { get; set; } // TODO remove
        private StringWriter Out { get; }
        private ShellCommand? PipedShellCommand { get; init; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.Output;
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            b.AssertNotStarted(); // sanity check

            var c = new ShellCommand(b.Shell, b.Script, b.ProcessRunConfiguration) {
                PipedShellCommand = a
            };

            return c;
        }

        public async Task<string> GetOutputAsync(CancellationToken cancellationToken = default) {
            var localOutput = _output;
            if (localOutput == null) {
                await _hasStartedSemaphore.WaitAsync();
                try {
                    localOutput = _output;
                    if (localOutput == null) {
                        AssertNotStarted();
                        HasStarted = true;
                        TextReader inputReader;
                        if (PipedShellCommand != null) {
                            if (PipedShellCommand._output != null) {
                                inputReader = new StringReader(PipedShellCommand._output);
                            } else {
                                var pipedProcess = PipedShellCommand.Start();
                                inputReader = pipedProcess.StandardOutput;
                            }
                        } else {
                            inputReader = ProcessRunConfiguration.In;
                        }
                        await Shell.RunAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = inputReader, Out = Out }, cancellationToken);
                        await WaitForExitAsync();
                        _output = localOutput = Out.ToString();
                    }
                } finally {
                    _hasStartedSemaphore.Release();
                }
            }
            return ShellCommandPostprocessor.ProcessOutput(localOutput);
        }

        public Process Start(CancellationToken cancellationToken = default) {
            AssertNotStarted();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMicroseconds(1)); // queued means HasStarted = true when out of queue
            try {
                _hasStartedSemaphore.Wait(cancellationTokenSource.Token);
                try {
                    AssertNotStarted();
                    HasStarted = true;
                    TextReader inputReader;
                    if (PipedShellCommand != null) {
                        if (PipedShellCommand._output != null) {
                            inputReader = new StringReader(PipedShellCommand._output);
                        } else {
                            var pipedProcess = PipedShellCommand.Start();
                            inputReader = pipedProcess.StandardOutput;
                        }
                    } else {
                        inputReader = ProcessRunConfiguration.In;
                    }
                    AssociatedProcess = Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = inputReader }, cancellationToken);
                } finally {
                    _hasStartedSemaphore.Release();
                }
            } catch (OperationCanceledException) {
                throw new InvalidOperationException("ShellCommand has already started");
            }
            return AssociatedProcess;
        }

        private async Task WaitForExitAsync() {
            if (PipedShellCommand != null) {
                await PipedShellCommand.WaitForExitAsync();
            }
            if (AssociatedProcess != null) {
                await AssociatedProcess.WaitForExitAsync();
                AssociatedProcess.Dispose();
            }
        }

        public override string ToString() {
            return Output;
        }

        private void AssertNotStarted() {
            if (HasStarted) {
                throw new InvalidOperationException("ShellCommand has already started");
            }
        }
    }
}
