﻿using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd {
    public class Command {
        private static readonly SemaphoreSlim _valueSemaphore = new SemaphoreSlim(1, 1);

        private volatile string? _value;

        private Command(IShell shell, string script, IProcessRunConfiguration processRunConfiguration, TextReader input, StringWriter output) : this(shell, script, processRunConfiguration) {
            In = input;
            Out = output;
        }

        public Command(IShell shell, string script, IProcessRunConfiguration processRunConfiguration) {
            Shell = shell;
            Script = script;
            ProcessRunConfiguration = processRunConfiguration;
            In = processRunConfiguration.In;
            Out = new StringWriter();
        }

        private TextReader In { get; }
        private StringWriter Out { get; }
        private IProcessRunConfiguration ProcessRunConfiguration { get; }
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(Command obj) {
            return obj.GetValueAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static Command operator |(Command a, Command b) {
            var cIn = new StringReader(a.GetValueAsync().ConfigureAwait(false).GetAwaiter().GetResult()); // TODO asynchronously relay stream in loop instead of converting to string (fire a task and forget)
            var c = new Command(b.Shell, b.Script, b.ProcessRunConfiguration, cIn, b.Out);
            return c;
        }

        public async Task<string> GetValueAsync(CancellationToken cancellationToken = default) {
            var localValue = _value;
            if (localValue == null) {
                await _valueSemaphore.WaitAsync();
                try {
                    localValue = _value;
                    if (localValue == null) {
                        await Shell.ExecuteAsync(Script, new ProcessRunConfiguration(ProcessRunConfiguration) { In = In, Out = Out }, cancellationToken);
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
