using CalqFramework.Cmd.Execution;

namespace CalqFramework.Cmd.Shells;

public abstract class ShellBase : IShell {

    public async Task ExecuteAsync(string workingDirectory, string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        var processExecutionInfo = GetProcessExecutionInfo(workingDirectory, script);
        using var processRunner = new ProcessRunner();

        var errorWriter = new StringWriter();

        int exitCode = await processRunner.Run(workingDirectory, processExecutionInfo, inputReader, outputWriter, errorWriter, cancellationToken);

        var error = errorWriter.ToString();

        // stderr might contain diagnostics/info instead of error message so don't throw just because not empty
        if (exitCode != 0) {
            if (string.IsNullOrEmpty(error) && outputWriter is StringWriter) {
                error = outputWriter.ToString();
            }
            throw new CommandExecutionException($"\n{AddLineNumbers(script)}\n\nError:\n{error}", exitCode);
        }
    }

    internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);

    public abstract string GetInternalPath(string hostPath);
}
