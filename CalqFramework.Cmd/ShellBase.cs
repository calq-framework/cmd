using System.Diagnostics;

namespace CalqFramework.Cmd;

public abstract class ShellBase : IShell {
    public readonly AsyncLocal<string> _currentDirectory;
    public TextReader In { get; init; }
    public TextWriter Out { get; init; }
    public string CurrentDirectory { get => _currentDirectory.Value!; private set => _currentDirectory.Value = value; }

    public ShellBase() {
        _currentDirectory = new AsyncLocal<string>();
        In = Console.In;
        Out = Console.Out;
        CurrentDirectory = Environment.CurrentDirectory;
    }

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

    internal Process InitializeProcess(ScriptExecutionInfo scriptExecutionInfo) {
        ProcessStartInfo psi = new ProcessStartInfo {
            WorkingDirectory = CurrentDirectory,
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

    private async Task ExecuteAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}")); // TODO allow for \r\n ?
        }

        var scriptExecutionInfo = GetScriptExecutionInfo(script);
        using var process = InitializeProcess(scriptExecutionInfo);

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

    public string CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public string CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return CMDAsync(script, inputReader, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<string> CMDAsync(string script, CancellationToken cancellationToken = default) {
        return await CMDAsync(script, TextReader.Null, cancellationToken);
    }

    public async Task<string> CMDAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        var output = new StringWriter();
        await ExecuteAsync(script, inputReader, output, cancellationToken);
        return output.ToString();
    }

    public void RUN(string script, TimeSpan? timeout = null) {
        RUN(script, this.In, timeout);
    }

    public void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        ExecuteAsync(script, inputReader, this.Out, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    internal abstract ScriptExecutionInfo GetScriptExecutionInfo(string script);

    public void CD(string path) {
        CurrentDirectory = Path.GetFullPath(Path.Combine(CurrentDirectory, path));
    }

    public abstract string GetLocalPath(string path);
}
