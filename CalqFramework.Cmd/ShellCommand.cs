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
        private Process? AssociatedProcess { get; set; }
        private bool HasStarted { get; set; } // TODO remove
        private StringWriter Out { get; init; }

        /// <summary>
        /// Holds ref of Piped process preventing it from being finalized and terminated.
        /// </summary>
        private ShellCommand? PipedShellCommand { get; init; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.Output;
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            if (b.HasStarted) {
                // TODO throw
            }

            var c = new ShellCommand(b.Shell, b.Script, b.ProcessRunConfiguration) {
                PipedShellCommand = a,
                Out = b.Out
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
                        var pipedProcesses = PipedShellCommand != null && PipedShellCommand.HasStarted == false ? PipedShellCommand.StartCore() : null;
                        var pipedIn = pipedProcesses?.Last().StandardOutput ?? (PipedShellCommand != null ? PipedShellCommand.AssociatedProcess?.StandardOutput ?? (TextReader)new StringReader(PipedShellCommand.Output) : ProcessRunConfiguration.In);
                        await Shell.RunAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = pipedIn, Out = Out }, cancellationToken);
                        _output = localOutput = Out.ToString();
                    }
                } finally {
                    _hasStartedSemaphore.Release();
                }
            }
            return ShellCommandPostprocessor.ProcessOutput(localOutput);
        }

        public Process Start(CancellationToken cancellationToken = default) {
            Process? result = null;
            AssertNotStarted();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMicroseconds(1)); // queued means HasStarted = true
            try {
                _hasStartedSemaphore.Wait(cancellationTokenSource.Token);
                try {
                    AssertNotStarted();
                    HasStarted = true;
                    var pipedProcesses = PipedShellCommand != null && PipedShellCommand.HasStarted == false ? PipedShellCommand.StartCore() : null;
                    var pipedIn = pipedProcesses?.Last().StandardOutput ?? (PipedShellCommand != null ? PipedShellCommand.AssociatedProcess?.StandardOutput ?? (TextReader)new StringReader(PipedShellCommand.Output) : ProcessRunConfiguration.In);
                    result = AssociatedProcess = Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = pipedIn }, cancellationToken);
                } finally {
                    _hasStartedSemaphore.Release();
                }
            } catch (OperationCanceledException) {
                throw new InvalidOperationException("ShellCommand has already started");
            }
            return result;
        }

        public override string ToString() {
            return Output;
        }

        private void AssertNotStarted() {
            if (HasStarted) {
                throw new InvalidOperationException("ShellCommand has already started");
            }
        }

        private IList<Process> StartCore(CancellationToken cancellationToken = default) {
            IList<Process> pipedProcesses;
            AssertNotStarted();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMicroseconds(1)); // queued means HasStarted = true
            try {
                _hasStartedSemaphore.Wait(cancellationTokenSource.Token);
                try {
                    AssertNotStarted();
                    HasStarted = true;
                    if (PipedShellCommand == null) {
                        pipedProcesses = new List<Process>();
                        AssociatedProcess = Shell.Start(Script, ProcessRunConfiguration, cancellationToken);
                        pipedProcesses.Add(AssociatedProcess);
                    } else {
                        pipedProcesses = PipedShellCommand != null && PipedShellCommand.HasStarted == false ? PipedShellCommand.StartCore() : null;
                        var pipedIn = pipedProcesses?.Last().StandardOutput ?? (PipedShellCommand != null ? PipedShellCommand.AssociatedProcess?.StandardOutput ?? (TextReader)new StringReader(PipedShellCommand.Output) : ProcessRunConfiguration.In);
                        AssociatedProcess = Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = pipedIn }, cancellationToken);
                        pipedProcesses.Add(AssociatedProcess);
                    }

                } finally {
                    _hasStartedSemaphore.Release();
                }
            } catch (OperationCanceledException) {
                throw new InvalidOperationException("ShellCommand has already started");
            }
            return pipedProcesses;
        }
    }
}
