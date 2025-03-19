using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        private readonly SemaphoreSlim _hasStartedSemaphore = new SemaphoreSlim(1, 1);

        private volatile string? _output;

        public ShellCommand(IShell shell, string script, IProcessRunConfiguration processRunConfiguration) {
            Shell = shell;
            Script = script;
            ProcessRunConfiguration = processRunConfiguration;
            In = processRunConfiguration.In;
            Out = new StringWriter();
        }

        private ShellCommand(IShell shell, string script, IProcessRunConfiguration processRunConfiguration, TextReader inputReader, StringWriter outputWriter) : this(shell, script, processRunConfiguration) {
            In = inputReader;
            Out = outputWriter;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Output {
            get {
                return GetOutputAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public IShellCommandPostprocessor ShellCommandPostprocessor { get; init; } = new ShellCommandPostprocessor();
        private bool HasStarted { get; set; }
        private TextReader In { get; }
        private StringWriter Out { get; }

        /// <summary>
        /// Holds ref of Piped process preventing it from being finalized and terminated.
        /// </summary>
        private Process? PipedProcesses { get; init; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.Output;
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            TextReader cIn;
            Process? cPipedProcess;

            if (a.HasStarted) {
                cIn = new StringReader(a.Output);
                cPipedProcess = null;
            } else {
                var aProcess = a.Start();
                cIn = aProcess.StandardOutput;
                cPipedProcess = aProcess;
            }

            var c = new ShellCommand(b.Shell, b.Script, b.ProcessRunConfiguration, cIn, b.Out) {
                ShellCommandPostprocessor = b.ShellCommandPostprocessor,
                PipedProcesses = cPipedProcess
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
                        await Shell.RunAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = In, Out = Out }, cancellationToken);
                        _output = localOutput = Out.ToString();
                    }
                } finally {
                    if (PipedProcesses != null) {
                        // ShellCommand is the only owner of the piped process but ShellCommand is not IDisposable
                        PipedProcesses.Dispose();
                    }
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
                    result = Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = In, Out = Out }, cancellationToken);
                } finally {
                    // ShellCommand is the only owner of the piped process but ShellCommand is not IDisposable
                    if (PipedProcesses != null) {
                        if (result == null) {
                            PipedProcesses.Dispose();
                        } else {
                            // processed are terminated on dispose so if result is disposed so will be PipedProcess
                            result.EnableRaisingEvents = true;
                            result.Exited += (s, e) => {
                                PipedProcesses.Dispose();
                            };
                        }
                    }
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
    }
}
