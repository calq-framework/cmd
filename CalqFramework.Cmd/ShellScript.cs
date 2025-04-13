using CalqFramework.Cmd.Shell;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellScript {
        public static AsyncLocal<string> LocalWorkingDirectory = new();

        public ShellScript(IShell shell, string script) {
            Shell = shell;
            Script = script;
            WorkingDirectory = LocalWorkingDirectory.Value ?? Environment.CurrentDirectory;
        }

        public ShellScript? PipedShellScript { get; private init; }
        public string Script { get; }
        public IShell Shell { get; }
        public string WorkingDirectory { get; init; }

        public static implicit operator string(ShellScript obj) {
            return obj.Evaluate();
        }

        public static ShellScript operator |(ShellScript a, ShellScript b) {
            var c = new ShellScript(b.Shell, b.Script) {
                WorkingDirectory = b.WorkingDirectory,
                PipedShellScript = a
            };

            return c;
        }

        public string Evaluate(CancellationToken cancellationToken = default) {
            return EvaluateAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public string Evaluate(TextReader? inputReader, CancellationToken cancellationToken = default) {
            return EvaluateAsync(inputReader).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> EvaluateAsync(TextReader? inputReader, CancellationToken cancellationToken = default) {
            using var worker = Start(inputReader, cancellationToken);
            var output = await worker.StandardOutput.ReadToEndAsync();

            await worker.WaitForSuccess(output);

            cancellationToken.ThrowIfCancellationRequested();

            return Shell.Postprocessor.ProcessOutput(output);
        }

        public async Task<string> EvaluateAsync(CancellationToken cancellationToken = default) {
            return await EvaluateAsync(null, cancellationToken);
        }

        public void Run(TextWriter outputWriter, CancellationToken cancellationToken = default) {
            RunAsync(outputWriter, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Run(TextReader? inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
            RunAsync(inputReader, outputWriter, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task RunAsync(TextWriter outputWriter, CancellationToken cancellationToken = default) {
            await RunAsync(Shell.In, outputWriter, cancellationToken);
        }

        public async Task RunAsync(TextReader? inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
            using var worker = Start(inputReader, cancellationToken);

            var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var relayOutputTask = StreamUtils.RelayStream(worker.StandardOutput, outputWriter, relayOutputCts.Token);

            await relayOutputTask;

            try {
                await worker.WaitForSuccess(outputWriter.ToString());
            } catch {
                relayOutputCts.Cancel();
                throw;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public IShellWorker Start(CancellationToken cancellationToken = default) {
            var worker = Shell.CreateShellWorker(this, cancellationToken);
            return worker;
        }

        public IShellWorker Start(TextReader? inputReader, CancellationToken cancellationToken = default) {
            var worker = Shell.CreateShellWorker(this, inputReader, cancellationToken);
            return worker;
        }

        public override string ToString() {
            return Evaluate();
        }
    }
}
