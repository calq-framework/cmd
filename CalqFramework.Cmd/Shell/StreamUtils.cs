using System.Reflection;
using System.Runtime.InteropServices;

namespace CalqFramework.Cmd.Shell {
    internal class StreamUtils {
        // assumes one of the following
        // https://github.com/dotnet/runtime/blob/464e5fe6fbe499012445cbd6371010748b89dba3/src/libraries/System.Console/src/System/ConsolePal.Unix.ConsoleStream.cs#L13
        // https://github.com/dotnet/runtime/blob/464e5fe6fbe499012445cbd6371010748b89dba3/src/libraries/System.Console/src/System/ConsolePal.Windows.cs#L1149
        private static bool HasMatchingUnderlyingStream(Stream stream1, Stream stream2) {
            if (stream1.GetType() != stream2.GetType()) {
                return false;
            }

            var handleField1 = stream1.GetType().GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var handleField2 = stream2.GetType().GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var handle1 = handleField1.GetValue(stream1)!;
            var handle2 = handleField2.GetValue(stream2)!;

            return (handle1, handle2) switch {
                (IntPtr ptr1, IntPtr ptr2) => ptr1 == ptr2,
                (SafeHandle sh1, SafeHandle sh2) => sh1.DangerousGetHandle() == sh2.DangerousGetHandle(),
                _ => handle1?.Equals(handle2) ?? false
            };
        }

        private static bool IsStandardInputStream(TextReader reader) {
            if (reader is StreamReader sr) {
                using var standardInput = Console.OpenStandardInput();
                return HasMatchingUnderlyingStream(sr.BaseStream, standardInput);
            }

            var readerType = reader.GetType();
            if (readerType.FullName == "System.IO.StdInReader") { // this is used on linux
                return true;
            }

            if (readerType.FullName == "System.IO.SyncTextReader") {
                var innerReaderField = readerType.GetField("_in", BindingFlags.NonPublic | BindingFlags.Instance);
                if (innerReaderField != null) {
                    var innerReader = (innerReaderField.GetValue(reader) as TextReader)!;
                    return IsStandardInputStream(innerReader);
                }
            }

            return false;
        }

        private static bool IsInputRedirected(TextReader reader) {
            return Console.IsInputRedirected || IsStandardInputStream(reader) == false;
        }

        public static async Task RelayInput(TextWriter processInput, StreamReader inputReader, CancellationToken cancellationToken) {
            var isInputRedirected = IsInputRedirected(inputReader);
            if (isInputRedirected == false) {
                while (!cancellationToken.IsCancellationRequested) {
                    if (Console.KeyAvailable) {
                        var keyChar = Console.ReadKey(false).KeyChar;
                        if (keyChar == '\r') { // windows enterkey is \r which returns carriage back to the beginning of the line instead of starting a new line
                            Console.WriteLine();
                            keyChar = '\n';
                        }
                        processInput.Write(keyChar);
                    }

                    await Task.Delay(1);
                }
            } else {
                // TODO relay block by block
                var buffer = new char[1];

                while (!cancellationToken.IsCancellationRequested) {
                    var bytesRead = await inputReader.ReadAsync(buffer, cancellationToken);
                    var keyChar = buffer[0];
                    if (bytesRead == 0 || keyChar == '\uffff') { // '\uffff' == -1
                        processInput.Close(); // in case input stream reached EOF close input stream to signal EOF to the process
                        break;
                    }
                    processInput.Write(keyChar);

                    await Task.Delay(1);
                }
            }
        }

        public static async Task RelayStream(TextReader reader, TextWriter writer, CancellationToken cancellationToken) {
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

                await Task.Delay(1);
            }

            await writer.FlushAsync();
        }
    }
}
