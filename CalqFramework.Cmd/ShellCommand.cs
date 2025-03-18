using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        private static readonly SemaphoreSlim _valueSemaphore = new SemaphoreSlim(1, 1);

        private volatile string? _value;

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
        private TextReader In { get; }
        private StringWriter Out { get; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.Output;
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            var cIn = new StringReader(a.Output); // TODO asynchronously relay stream in loop instead of converting to string (fire a task and forget)
            var c = new ShellCommand(b.Shell, b.Script, b.ProcessRunConfiguration, cIn, b.Out) { ShellCommandPostprocessor = b.ShellCommandPostprocessor };
            return c;
        }

        public async Task<string> GetOutputAsync(CancellationToken cancellationToken = default) {
            var localValue = _value;
            if (localValue == null) {
                await _valueSemaphore.WaitAsync();
                try {
                    localValue = _value;
                    if (localValue == null) {
                        await Shell.RunAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = In, Out = Out }, cancellationToken);
                        _value = localValue = Out.ToString();
                    }
                } finally {
                    _valueSemaphore.Release();
                }
            }
            return ShellCommandPostprocessor.ProcessOutput(localValue);
        }

        public Process Start(CancellationToken cancellationToken = default) {
            return Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = In, Out = Out }, cancellationToken);
        }

        public override string ToString() {
            return Output;
        }
    }
}
