using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ghbvft6.Calq.Terminal;

internal abstract class Shell : IShell
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
                if (bufferArray[0] != '\0')
                {
                    await writer.WriteAsync(new string(bufferArray).Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
                    //await writer.WriteAsync(bufferArray);
                    await writer.FlushAsync();
                    output.Append(bufferArray);
                    Array.Clear(bufferArray);
                }
            }
        }

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
        var cts = new CancellationTokenSource();
        var keyReaderTask = Task.Run(() => // TODO extract this logic
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    char keyChar = keyInfo.KeyChar;
                    input.Write(keyChar);
                }
                Task.Delay(1).Wait();
            }
        });

        process.WaitForExit();
        cts.Cancel();
        var output = outputReaderTask.Result;
        var error = errorReaderTask.Result;
        keyReaderTask.Wait();

        if (process.ExitCode != 0)
        {
            throw new CommandExecutionException($"\n{AddLineNumbers(script)}\n\nError:\n{error}", process.ExitCode); // TODO extract formatting logic
        }

        return output;
    }
}
