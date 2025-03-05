using System.Diagnostics;

namespace CalqFramework.Shell;

public abstract class ShellBase : IShell {
    private static async Task RelayStream(StreamReader reader, TextWriter writer) {
        var bufferArray = new char[4096];

        while (true) {
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

    internal Process InitializeProcess(ScriptExecutionInfo scriptExecutionInfo) {
        ProcessStartInfo psi = new ProcessStartInfo {
            WorkingDirectory = Environment.CurrentDirectory,
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

    private void CMD(string script, TextWriter outputWriter) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}")); // TODO allow for \r\n ?
        }

        var scriptExecutionInfo = GetScriptExecutionInfo(script);
        using var process = InitializeProcess(scriptExecutionInfo);
        var outputReaderTask = Task.Run(async () => await RelayStream(process.StandardOutput, outputWriter));
        var errorOutputWriter = new StringWriter();
        var errorReaderTask = Task.Run(async () => await RelayStream(process.StandardError, errorOutputWriter));

        var input = process.StandardInput;
        using var cts = new CancellationTokenSource();
        var keyReaderTask = Task.Run(async () => // TODO extract this logic
        {
            if (Environment.UserInteractive && ReferenceEquals(Console.In, Console.OpenStandardInput())) {
                while (!cts.Token.IsCancellationRequested) {
                    if (Console.KeyAvailable) {
                        var keyChar = Console.ReadKey(true).KeyChar;
                        if (keyChar == '\r') { // windows enterkey is \r and deletes what was typed because of that
                            keyChar = '\n';
                        }
                        Console.Write(keyChar);
                        input.Write(keyChar);
                    }
                    await Task.Delay(1);
                }
            } else {
                while (!cts.Token.IsCancellationRequested) {
                    if (Console.In.Peek() != -1) {
                        var keyChar = (char)Console.Read();
                        input.Write(keyChar);
                    }
                    await Task.Delay(1);
                }
            }
        });

        process.WaitForExit();
        cts.Cancel();
        outputReaderTask.Wait();
        errorReaderTask.Wait();
        var error = errorOutputWriter.ToString();
        while (keyReaderTask.Status == TaskStatus.Running) {
            keyReaderTask.Wait(1);
        }

        if (process.ExitCode != 0) {
            throw new CommandExecutionException($"\n{AddLineNumbers(script)}\n\nError:\n{error}", process.ExitCode); // TODO extract formatting logic
        }
    }

    public string CMD(string script) {
        var output = new StringWriter();
        CMD(script, output);
        return output.ToString();
    }

    public void RUN(string script) {
        CMD(script, Console.Out);
    }

    internal abstract ScriptExecutionInfo GetScriptExecutionInfo(string script);
}
