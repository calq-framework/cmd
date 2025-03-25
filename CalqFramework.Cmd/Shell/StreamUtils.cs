namespace CalqFramework.Cmd.Shell {
    internal class StreamUtils {
        public static async Task RelayInput(TextWriter processInput, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken) {
            try {
                if (Environment.UserInteractive && ReferenceEquals(inputReader, Console.OpenStandardInput())) {
                    while (!cancellationToken.IsCancellationRequested) {
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
                    // TODO relay block by block
                    var buffer = new char[1];

                    while (!cancellationToken.IsCancellationRequested) {
                        var bytesRead = await inputReader.ReadAsync(buffer, cancellationToken);
                        var keyChar = buffer[0];
                        if (bytesRead == 0 || keyChar == -1 || keyChar == '\uffff') {
                            break;
                        }
                        processInput.Write(keyChar);

                        await Task.Delay(1);
                    }
                }
            } finally {
                processInput.Close(); // in case input stream reached EOF close input stream to signal EOF to the process
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
