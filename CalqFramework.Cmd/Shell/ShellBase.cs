using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {

    static ShellBase() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            var outputWriter = new StringWriter();
            using var process = new RunnableProcess();
            process.Run(new ProcessExecutionInfo("bash", "-c \"uname -s\""), new ProcessRunConfiguration() { In = TextReader.Null, Out = outputWriter }).ConfigureAwait(false).GetAwaiter().GetResult();
            IsRunningBashOnWSL = outputWriter.ToString().TrimEnd() switch {
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
        RunAsync(script, processRunConfiguration, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        var processExecutionInfo = GetProcessExecutionInfo(processRunConfiguration.WorkingDirectory, script);
        using var process = new RunnableProcess();

        try {
            await process.Run(processExecutionInfo, processRunConfiguration, cancellationToken);
        } catch (ProcessExecutionException ex) {
            string message;
            if (string.IsNullOrEmpty(ex.Message) && processRunConfiguration.Out is StringWriter stringOutputWriter) {
                message = stringOutputWriter.ToString();
            } else {
                message = ex.Message;
            }
            throw new ShellCommandExecutionException(ex.ExitCode, $"\n{AddLineNumbers(script)}\n\nExit code:\n{ex.ExitCode}\n\nError:\n{message}", ex); // TODO formalize error handling
        }
    }

    public async Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, RunnableProcess? pipedProcess, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        var processExecutionInfo = GetProcessExecutionInfo(processRunConfiguration.WorkingDirectory, script);
        using var process = new RunnableProcess() { PipedProcess = pipedProcess };

        try {
            await process.Run(processExecutionInfo, processRunConfiguration, cancellationToken);
        } catch (ProcessExecutionException ex) {
            string message;
            if (string.IsNullOrEmpty(ex.Message) && processRunConfiguration.Out is StringWriter stringOutputWriter) {
                message = stringOutputWriter.ToString();
            } else {
                message = ex.Message;
            }
            throw new ShellCommandExecutionException(ex.ExitCode, $"\n{AddLineNumbers(script)}\n\nExit code:\n{ex.ExitCode}\n\nError:\n{message}", ex); // TODO formalize error handling
        }
    }

    public RunnableProcess Start(string script, IProcessStartConfiguration processStartConfiguration, CancellationToken cancellationToken = default) {
        var processExecutionInfo = GetProcessExecutionInfo(processStartConfiguration.WorkingDirectory, script);
        var process = new RunnableProcess();

        process.Start(processExecutionInfo, processStartConfiguration, cancellationToken);

        return process;
    }

    public RunnableProcess Start(string script, IProcessStartConfiguration processStartConfiguration, RunnableProcess? pipedProcess, CancellationToken cancellationToken = default) {
        var processExecutionInfo = GetProcessExecutionInfo(processStartConfiguration.WorkingDirectory, script);
        var process = new RunnableProcess() { PipedProcess = pipedProcess };

        process.Start(processExecutionInfo, processStartConfiguration, cancellationToken);

        return process;
    }

    public RunnableProcess Start(string script, IProcessRunConfiguration processRunConfiguration, RunnableProcess? pipedProcess, CancellationToken cancellationToken = default) {
        var processExecutionInfo = GetProcessExecutionInfo(processRunConfiguration.WorkingDirectory, script);
        var process = new RunnableProcess() { PipedProcess = pipedProcess };

        process.Start(processExecutionInfo, processRunConfiguration, cancellationToken);

        return process;
    }

    internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
}
