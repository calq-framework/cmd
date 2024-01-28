using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalqFramework.Terminal;

internal abstract class ShellBase : IShell
{
    // TODO create interceptor class?
    private static async Task<string> RelayStream(StreamReader reader, TextWriter writer)
    {
        var bufferArray = new char[4096];
        var buffer = new Memory<char>(bufferArray);
        var output = new StringBuilder(); // TODO accept array of TextWriter and use StringWriter?

        while (true)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
                var bytesRead = await reader.ReadAsync(buffer, cancellationTokenSource.Token);

                if (bytesRead == 0)
                {
                    break;
                }

                await writer.WriteAsync(new string(bufferArray, 0, bytesRead).Replace("\r\n", "\n").Replace("\n", Environment.NewLine)); // TODO extract newline replacement logic
                //await writer.WriteAsync(bufferArray, 0, bytesRead);
                output.Append(bufferArray, 0, bytesRead);
            }
            catch (OperationCanceledException)
            {
                try {
                    if (bufferArray[0] != '\0') {
                        await writer.WriteAsync(new string(bufferArray).Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
                        //await writer.WriteAsync(bufferArray);
                        await writer.FlushAsync();
                        output.Append(bufferArray);
                        Array.Clear(bufferArray);
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
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
