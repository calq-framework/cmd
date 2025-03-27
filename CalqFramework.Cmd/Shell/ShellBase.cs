using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public abstract string MapToInternalPath(string hostPath);

    public void Run(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        RunAsync(script, processRunConfiguration, null, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        await RunAsync(script, processRunConfiguration, null, cancellationToken);
    }

    public async Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, ShellWorkerBase? pipedShellWorker, CancellationToken cancellationToken = default) {
        using var worker = CreateShellWorker(script, processRunConfiguration, pipedShellWorker, cancellationToken);

        var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var relayOutputTask = StreamUtils.RelayStream(worker.StandardOutput, processRunConfiguration.Out, relayOutputCts.Token);

        try {
            await worker.WaitForSuccess();
        } catch {
            relayOutputCts.Cancel();
            throw;
        }

        await relayOutputTask;

        cancellationToken.ThrowIfCancellationRequested();
    }
    public ShellWorkerBase Start(string script, IProcessStartConfiguration processStartConfiguration, ShellWorkerBase? pipedShellWorker, CancellationToken cancellationToken = default) {
        var worker = CreateShellWorker(script, processStartConfiguration, pipedShellWorker, cancellationToken);

        return worker;
    }

    internal abstract ShellWorkerBase CreateShellWorker(string script, IProcessStartConfiguration processStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default); // TODO protected
}
