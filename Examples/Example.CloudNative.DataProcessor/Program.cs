using System.Text;
using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Example.CloudNative.DataProcessor;
using static CalqFramework.Cmd.Terminal;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CalqCmdController).Assembly);
builder.Services.AddCalqCmdController(new DataProcessingCommands());

var app = builder.Build();
app.MapControllers();
app.Run();

namespace Example.CloudNative.DataProcessor {
    /// <summary>
    ///     Command target — methods become executable commands via CalqCmdController.
    ///     ProcessParallel calls ProcessChunk via LocalTool (distributed HTTP execution).
    /// </summary>
    public class DataProcessingCommands {
        /// <summary>Processes input data in parallel chunks via distributed LocalTool calls.</summary>
        public async Task<string> ProcessParallel() {
            if (LocalTerminal.Shell.In == null) {
                return "No input stream provided";
            }

            using var reader = new StreamReader(LocalTerminal.Shell.In);
            var inputData = await reader.ReadToEndAsync();

            var lines = inputData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var chunks = lines.Chunk(Math.Max(1, lines.Length / Environment.ProcessorCount));

            // Each chunk calls ProcessChunk on itself via HTTP (cloud-native distributed execution)
            LocalTerminal.Shell = new LocalTool();
            var tasks = chunks.Select(async chunk => {
                var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join('\n', chunk)));
                return await CMDAsync(nameof(ProcessChunk), inputStream);
            });

            var results = await Task.WhenAll(tasks);
            return string.Join('\n', results);
        }

        /// <summary>Processes a single chunk of data.</summary>
        public async Task<string> ProcessChunk() {
            if (LocalTerminal.Shell.In == null) {
                return "Empty chunk";
            }

            using var reader = new StreamReader(LocalTerminal.Shell.In);
            var data = await reader.ReadToEndAsync();
            await Task.Delay(100);
            return $"Processed: {data.Trim().ToUpper()} [PID: {Environment.ProcessId}, TID: {Environment.CurrentManagedThreadId}]";
        }
    }
}
