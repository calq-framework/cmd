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
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        using var worker = CreateShellWorker(script, processRunConfiguration, pipedShellWorker, cancellationToken);

        var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var relayOutputTask = StreamUtils.RelayStream(worker.StandardOutput, processRunConfiguration.Out, relayOutputCts.Token);

        try {
            await worker.WaitForSuccess();
        } catch (ProcessExecutionException ex) {
            relayOutputCts.Cancel();

            string message;
            if (string.IsNullOrEmpty(ex.Message) && processRunConfiguration.Out is StringWriter stringOutputWriter) {
                message = stringOutputWriter.ToString();
            } else {
                message = ex.Message;
            }
            throw new ShellCommandExecutionException(ex.ExitCode, $"\n{AddLineNumbers(script)}\n\nExit code:\n{ex.ExitCode}\n\nError:\n{message}", ex); // TODO formalize error handling
        } catch (Exception) {
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
