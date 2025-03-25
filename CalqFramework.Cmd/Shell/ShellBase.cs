using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {

    static ShellBase() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            using var worker = new ShellWorker(new ProcessExecutionInfo("bash", @"-c ""uname -s"""), new ProcessRunConfiguration() { In = TextReader.Null });
            IsRunningBashOnWSL = worker.StandardOutput.ReadToEnd().TrimEnd() switch {
                "Linux" => true,
                "Darwin" => true,
                _ => false
            };
        } else {
            IsRunningBashOnWSL = false;
        }
    }

    internal static bool IsRunningBashOnWSL { get; }

    internal abstract bool IsUsingWSL { get; }

    public string MapToInternalPath(string hostPath) {
        if (IsUsingWSL) {
            return WSLUtils.WindowsToWslPath(hostPath);
        }

        return hostPath;
    }

    public void Run(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        RunAsync(script, processRunConfiguration, null, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        await RunAsync(script, processRunConfiguration, null, cancellationToken);
    }

    public async Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, ShellWorker? pipedShellWorker, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        var processExecutionInfo = GetProcessExecutionInfo(processRunConfiguration.WorkingDirectory, script);

        using var worker = new ShellWorker(processExecutionInfo, processRunConfiguration, cancellationToken) {
            PipedWorker = pipedShellWorker
        };

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
    public ShellWorker Start(string script, IProcessStartConfiguration processStartConfiguration, ShellWorker? pipedShellWorker, CancellationToken cancellationToken = default) {
        var processExecutionInfo = GetProcessExecutionInfo(processStartConfiguration.WorkingDirectory, script);

        var worker = new ShellWorker(processExecutionInfo, processStartConfiguration, cancellationToken) {
            PipedWorker = pipedShellWorker
        };

        return worker;
    }


    internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
}
