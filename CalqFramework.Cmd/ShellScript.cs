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

        public string Evaluate(Stream? inputStream, CancellationToken cancellationToken = default) {
            return EvaluateAsync(inputStream).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> EvaluateAsync(Stream? inputStream, CancellationToken cancellationToken = default) {
            using var worker = await Start(inputStream, cancellationToken);
            using var reader = new StreamReader(worker.StandardOutput);
            var output = await reader.ReadToEndAsync();

            await worker.WaitForSuccess(output);

            cancellationToken.ThrowIfCancellationRequested();

            return Shell.Postprocessor.ProcessOutput(output);
        }

        public async Task<string> EvaluateAsync(CancellationToken cancellationToken = default) {
            return await EvaluateAsync(null, cancellationToken);
        }

        public void Run(Stream outputStream, CancellationToken cancellationToken = default) {
            RunAsync(outputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Run(Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
            RunAsync(inputStream, outputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task RunAsync(Stream outputStream, CancellationToken cancellationToken = default) {
            await RunAsync(Shell.In, outputStream, cancellationToken);
        }

        public async Task RunAsync(Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
            using var worker = await Start(inputStream, cancellationToken);

            var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var relayOutputTask = StreamUtils.RelayStream(worker.StandardOutput, outputStream, relayOutputCts.Token);

            await relayOutputTask;

            try {
                await worker.WaitForSuccess(outputStream.ToString());
            } catch {
                relayOutputCts.Cancel();
                throw;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public async Task<IShellWorker> Start(CancellationToken cancellationToken = default) {
            var worker = Shell.CreateShellWorker(this, cancellationToken);
            await worker.Start();
            return worker;
        }

        public async Task<IShellWorker> Start(Stream? inputStream, CancellationToken cancellationToken = default) {
            var worker = Shell.CreateShellWorker(this, inputStream, cancellationToken);
            await worker.Start();
            return worker;
        }

        public override string ToString() {
            return Evaluate();
        }
    }
}
