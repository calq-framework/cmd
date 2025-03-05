using System.Diagnostics;

namespace CalqFramework.Shell;

public abstract class ShellBase : IShell {
    public TextReader In { get; init; }
    public TextWriter Out { get; init; }

    public ShellBase() {
        In = Console.In;
        Out = Console.Out;
    }

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

    private void CMD(string script, TextReader inputReader, TextWriter outputWriter) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}")); // TODO allow for \r\n ?
        }

        var scriptExecutionInfo = GetScriptExecutionInfo(script);
        using var process = InitializeProcess(scriptExecutionInfo);
        var errorWriter = new StringWriter();

        var relayInputTaskCts = new CancellationTokenSource();
        var relayInputTask = Task.Run(async () => await RelayInput(process, inputReader, outputWriter), relayInputTaskCts.Token);
        var relayOutputTask = Task.Run(async () => await RelayStream(process.StandardOutput, outputWriter));
        var relayErrorTask = Task.Run(async () => await RelayStream(process.StandardError, errorWriter));

        process.WaitForExit();
        relayInputTaskCts.Cancel();
        relayOutputTask.Wait();
        relayErrorTask.Wait();

        var error = errorWriter.ToString();

        if (process.ExitCode != 0 || !string.IsNullOrEmpty(error)) {
            if (string.IsNullOrEmpty(error) && outputWriter is StringWriter) {
                error = outputWriter.ToString();
            }
            throw new CommandExecutionException($"\n{AddLineNumbers(script)}\n\nError:\n{error}", process.ExitCode);
        }
    }

    public string CMD(string script, TextReader? inputReader = null) {
        inputReader ??= TextReader.Null;
        var output = new StringWriter();
        CMD(script, inputReader, output);
        return output.ToString();
    }

    public void RUN(string script, TextReader? inputReader = null) {
        inputReader ??= this.In;
        CMD(script, inputReader, this.Out);
    }

    internal abstract ScriptExecutionInfo GetScriptExecutionInfo(string script);
}
