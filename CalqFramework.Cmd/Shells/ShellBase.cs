using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shells;

public abstract class ShellBase : IShell {

    public async Task ExecuteAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        var processRunInfo = GetProcessRunInfo(processRunConfiguration.WorkingDirectory, script);
        using var processRunner = new ProcessRunner();

        try {
            await processRunner.Run(processRunInfo, processRunConfiguration, cancellationToken);
        } catch (ProcessExecutionException ex) {
            throw new CommandExecutionException(ex.ExitCode, $"\n{AddLineNumbers(script)}\n\nError:\n{ex.Message}", ex);
        }
    }

    public abstract string GetInternalPath(string hostPath);

    internal abstract ProcessRunInfo GetProcessRunInfo(string workingDirectory, string script);
}
