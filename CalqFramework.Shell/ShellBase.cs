using System.Diagnostics;
using System.Text;

namespace CalqFramework.Shell;

public abstract class ShellBase : IShell
{
    // TODO create interceptor class?
    private static async Task<string> RelayStream(StreamReader reader, TextWriter writer)
    {
        var bufferArray = new char[4096];
        var output = new StringBuilder(); // TODO accept array of TextWriter and use StringWriter?

        while (true)
        {
            bool isRead = false;
            int bytesRead = 0;
            try
            {
                Array.Clear(bufferArray);
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
                bytesRead = await reader.ReadAsync(bufferArray, cancellationTokenSource.Token);
                isRead = true;
            }
            catch (OperationCanceledException)
            {
                try {
                    isRead = false;
                    bytesRead = Array.IndexOf(bufferArray, '\0');
                    if (bytesRead > 0) {
                        await writer.WriteAsync(new string(bufferArray, 0, bytesRead).Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
                        //await writer.WriteAsync(bufferArray);
                        await writer.FlushAsync();
                        output.Append(bufferArray);
                        continue;
                    }
                } catch (Exception ex) {
                    // TODO remove? this should never be reached
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    break;
                }
            }

            if (isRead && bytesRead == 0) {
                break;
            }

            if (bytesRead > 0) {
                await writer.WriteAsync(new string(bufferArray, 0, bytesRead).Replace("\r\n", "\n").Replace("\n", Environment.NewLine)); // TODO extract newline replacement logic
                //await writer.WriteAsync(bufferArray, 0, bytesRead);
                output.Append(bufferArray, 0, bytesRead);
            }
        }

        await writer.FlushAsync();
        return output.ToString();
    }

    protected abstract Process InitializeProcess(string script);

    public string CMD(string script)
    {
        string AddLineNumbers(string input)
        {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}")); // TODO allow for \r\n ?
        }

        using var process = InitializeProcess(script);
        var outputReaderTask = Task.Run(async () => await RelayStream(process.StandardOutput, Console.Out));
        var errorReaderTask = Task.Run(async () => await RelayStream(process.StandardError, TextWriter.Null));

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
        var output = outputReaderTask.Result;
        var error = errorReaderTask.Result;
        while (keyReaderTask.Status == TaskStatus.Running)
        {
            keyReaderTask.Wait(1);
        }

        if (process.ExitCode != 0)
        {
            throw new CommandExecutionException($"\n{AddLineNumbers(script)}\n\nError:\n{error}", process.ExitCode); // TODO extract formatting logic
        }

        return output;
    }
}
