using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.SystemProcess;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class Command {
        private static readonly SemaphoreSlim _valueSemaphore = new SemaphoreSlim(1, 1);

        private volatile string? _value;

        public Command(IShell shell, string script, IProcessRunConfiguration processRunConfiguration) {
            Shell = shell;
            Script = script;
            ProcessRunConfiguration = processRunConfiguration;
            In = processRunConfiguration.In;
            Out = new StringWriter();
        }

        private Command(IShell shell, string script, IProcessRunConfiguration processRunConfiguration, TextReader inputReader, StringWriter outputWriter) : this(shell, script, processRunConfiguration) {
            In = inputReader;
            Out = outputWriter;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Output {
            get {
                return GetOutputAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public ICommandOutputPostprocessor OutputPostprocessor { get; init; } = new CommandOutputPostprocessor();
        private TextReader In { get; }
        private StringWriter Out { get; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(Command obj) {
            return obj.Output;
        }

        public static Command operator |(Command a, Command b) {
            var cIn = new StringReader(a.Output); // TODO asynchronously relay stream in loop instead of converting to string (fire a task and forget)
            var c = new Command(b.Shell, b.Script, b.ProcessRunConfiguration, cIn, b.Out) { OutputPostprocessor = b.OutputPostprocessor };
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
            return OutputPostprocessor.ProcessValue(localValue);
        }

        public Process Start(CancellationToken cancellationToken = default) {
            return Shell.Start(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = In, Out = Out }, cancellationToken);
        }

        public override string ToString() {
            return Output;
        }
    }
}
