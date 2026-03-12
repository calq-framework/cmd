using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CalqFramework.Cmd.Shell;

/// <summary>
///     Utility methods for stream handling and input/output relay operations.
///     Provides functionality for relaying data between streams and detecting console input redirection.
/// </summary>
internal class StreamUtils {
    /// <summary>
    ///     Relays input from a stream to a process's standard input stream.
    ///     Handles both console input (character by character with encoding) and redirected input (binary buffered).
    /// </summary>
    public static async Task RelayInput(Stream processInputStream, Stream inputStream,
        CancellationToken cancellationToken) {
        bool isInputRedirected = IsInputRedirected(inputStream);
        if (!isInputRedirected) {
            // Console keyboard input - read chars and encode to UTF-8 bytes
            while (!cancellationToken.IsCancellationRequested) {
                if (Console.KeyAvailable) {
                    char keyChar = Console.ReadKey(false).KeyChar;
                    if (keyChar == '\r') {
                        // windows enterkey is \r which returns carriage back to the beginning of the line instead of starting a new line
                        Console.WriteLine();
                        keyChar = '\n';
                    }

                    byte[] charBytes = System.Text.Encoding.UTF8.GetBytes(new[] { keyChar });
                    await processInputStream.WriteAsync(charBytes.AsMemory(), cancellationToken);
                    await processInputStream.FlushAsync(cancellationToken);
                }

                await Task.Delay(1, cancellationToken);
            }
        } else {
            // Redirected input - binary copy
            byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
            try {
                while (!cancellationToken.IsCancellationRequested) {
                    int bytesRead;
                    try {
                        bytesRead = await inputStream.ReadAsync(buffer.AsMemory(0, 4096), cancellationToken);
                    } catch {
                        processInputStream
                            .Close(); // in case input stream reached EOF close input stream to signal EOF to the process
                        throw;
                    }

                    if (bytesRead == 0) {
                        processInputStream
                            .Close(); // in case input stream reached EOF close input stream to signal EOF to the process
                        break;
                    }

                    await processInputStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    await processInputStream.FlushAsync(cancellationToken);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    // assumes one of the following
    // https://github.com/dotnet/runtime/blob/464e5fe6fbe499012445cbd6371010748b89dba3/src/libraries/System.Console/src/System/ConsolePal.Unix.ConsoleStream.cs#L13
    // https://github.com/dotnet/runtime/blob/464e5fe6fbe499012445cbd6371010748b89dba3/src/libraries/System.Console/src/System/ConsolePal.Windows.cs#L1149
    /// <summary>
    ///     Checks if two streams have the same underlying handle using reflection.
    ///     Used to determine if streams represent the same resource (e.g., console input).
    /// </summary>
    /// <returns>True if streams have matching underlying handles</returns>
    private static bool HasMatchingUnderlyingStream(Stream stream1, Stream stream2) {
        if (stream1.GetType() != stream2.GetType()) {
            return false;
        }

        FieldInfo handleField1 = stream1.GetType().GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance)!;
        FieldInfo handleField2 = stream2.GetType().GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance)!;
        object handle1 = handleField1.GetValue(stream1)!;
        object handle2 = handleField2.GetValue(stream2)!;

        return (handle1, handle2) switch {
            (IntPtr ptr1, IntPtr ptr2) => ptr1 == ptr2,
            (SafeHandle sh1, SafeHandle sh2) => sh1.DangerousGetHandle() == sh2.DangerousGetHandle(),
            _ => handle1?.Equals(handle2) ?? false
        };
    }

    /// <summary>
    ///     Determines if input is redirected by checking console redirection status and stream type.
    /// </summary>
    /// <returns>True if input is redirected from console</returns>
    private static bool IsInputRedirected(Stream stream) =>
        Console.IsInputRedirected || !IsStandardInputStream(stream);

    /// <summary>
    ///     Checks if a Stream represents the standard console input stream.
    ///     Uses reflection to compare underlying stream handles across different .NET implementations.
    /// </summary>
    /// <returns>True if the stream represents standard console input</returns>
    private static bool IsStandardInputStream(Stream stream) {
        using Stream standardInput = Console.OpenStandardInput();
        return HasMatchingUnderlyingStream(stream, standardInput);
    }
}
