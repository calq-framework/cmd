using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd {
    public class Command {
        private static readonly SemaphoreSlim _valueSemaphore = new SemaphoreSlim(1, 1);

        private volatile string? _value;

        public Command(IShell shell, string workingDirectory, string script, TextReader inReader) {
            In = inReader;
            Shell = shell;
            WorkingDirectory = workingDirectory;
            Script = script;
            Out = new StringWriter();
        }
        private TextReader In { get; }
        private StringWriter Out { get; }
        private string Script { get; }
        private IShell Shell { get; }
        private string WorkingDirectory { get; }

        public static implicit operator string(Command obj) {
            return obj.GetValueAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static Command operator |(Command a, Command b) {
            var cIn = new StringReader(a.GetValueAsync().ConfigureAwait(false).GetAwaiter().GetResult()); // TODO asynchronously relay stream in loop instead of converting to string (fire a task and forget)
            var c = new Command(b.Shell, b.WorkingDirectory, b.Script, cIn);
            return c;
        }

        public async Task<string> GetValueAsync(CancellationToken cancellationToken = default) {
            var localValue = _value;
            if (localValue == null) {
                await _valueSemaphore.WaitAsync();
                try {
                    localValue = _value;
                    if (localValue == null) {
                        await Shell.ExecuteAsync(WorkingDirectory, Script, In, Out, cancellationToken);
                        _value = localValue = Out.ToString();
                    }
                } finally {
                    _valueSemaphore.Release();
                }
            }
            return localValue;
        }
    }
}
