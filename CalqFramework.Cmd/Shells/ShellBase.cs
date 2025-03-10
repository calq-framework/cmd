using CalqFramework.Cmd.Execution;
using System.Diagnostics;

namespace CalqFramework.Cmd.Shells;

public abstract class ShellBase : IShell {
    private static async Task RelayStream(StreamReader reader, TextWriter writer, CancellationToken cancellationToken) {
        var bufferArray = new char[4096];

        while (!cancellationToken.IsCancellationRequested) {
            bool isRead = false;
            int bytesRead = 0;
            try {
                Array.Clear(bufferArray);
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
                bytesRead = await reader.ReadAsync(bufferArray, cancellationTokenSource.Token);
                isRead = true;
            } catch (OperationCanceledException) {
                isRead = false;
                bytesRead = Array.IndexOf(bufferArray, '\0');
                if (bytesRead > 0) {
                    await writer.WriteAsync(bufferArray, 0, bytesRead);
                    await writer.FlushAsync();
                    continue;
                }
            }

            if (isRead && bytesRead == 0) {
                break;
            }

            if (bytesRead > 0) {
                await writer.WriteAsync(bufferArray, 0, bytesRead);
            }
        }

        await writer.FlushAsync();
    }

    private static async Task RelayInput(Process process, TextReader inputReader, TextWriter outputWriter) {
        var processInput = process.StandardInput;
        if (Environment.UserInteractive && ReferenceEquals(inputReader, Console.OpenStandardInput())) {
            while (!process.HasExited) {
                if (Console.KeyAvailable) {
                    var keyChar = Console.ReadKey(true).KeyChar;
                    if (keyChar == '\r') { // windows enterkey is \r and deletes what was typed because of that
                        keyChar = '\n';
                    }
                    outputWriter.Write(keyChar);
                    processInput.Write(keyChar);
                }
                await Task.Delay(1);
            }
        } else {
            while (!process.HasExited) {
                if (inputReader.Peek() != -1) {
                    var keyChar = (char)inputReader.Read();
                    processInput.Write(keyChar);
                }
                await Task.Delay(1);
            }
        }
    }

    internal Process InitializeProcess(string workingDirectory, ProcessExecutionInfo scriptExecutionInfo) {
        ProcessStartInfo psi = new ProcessStartInfo {
            WorkingDirectory = workingDirectory,
            FileName = scriptExecutionInfo.FileName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = scriptExecutionInfo.Arguments
        };

        var process = new Process { StartInfo = psi };
        process.Start();

        return process;
    }

    public async Task ExecuteAsync(string workingDirectory, string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}")); // TODO allow for \r\n ?
        }

        var scriptExecutionInfo = GetProcessExecutionInfo(workingDirectory, script);
        using var process = InitializeProcess(workingDirectory, scriptExecutionInfo);

        cancellationToken.Register(process.Kill);

        var errorWriter = new StringWriter();

        var relayInputTaskCts = new CancellationTokenSource();
        var relayInputTask = RelayInput(process, inputReader, outputWriter).WaitAsync(relayInputTaskCts.Token);

        var relayOutputTask = RelayStream(process.StandardOutput, outputWriter, cancellationToken);
        var relayErrorTask = RelayStream(process.StandardError, errorWriter, cancellationToken);

        await process.WaitForExitAsync();
        relayInputTaskCts.Cancel();
        cancellationToken.ThrowIfCancellationRequested();

        await relayOutputTask;
        await relayErrorTask;

        var error = errorWriter.ToString();

        // stderr might contain diagnostics/info instead of error message so don't throw just because not empty
        if (process.ExitCode != 0) {
            if (string.IsNullOrEmpty(error) && outputWriter is StringWriter) {
                error = outputWriter.ToString();
            }
            throw new CommandExecutionException($"\n{AddLineNumbers(script)}\n\nError:\n{error}", process.ExitCode);
        }
    }

    internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);

    public abstract string GetInternalPath(string hostPath);
}
