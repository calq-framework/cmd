using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Example.CloudNativePython.DataProcessor;
using static CalqFramework.Cmd.Terminal;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CalqCmdController).Assembly);
builder.Services.AddPythonTool("tool.py");
builder.Services.AddCalqCmdController(provider => new DataProcessingCommands(provider.GetRequiredService<PythonTool>()));

WebApplication app = builder.Build();
await app.Services.StartPythonToolServerAsync();
app.MapControllers();
app.Run();

namespace Example.CloudNativePython.DataProcessor {
    /// <summary>
    ///     Command target — ProcessParallel splits input into chunks and calls ProcessChunk
    ///     via LocalTool (distributed HTTP). ProcessChunk delegates to Python via PythonTool.
    /// </summary>
    public class DataProcessingCommands(PythonTool pythonTool) {
        private readonly PythonTool _pythonTool = pythonTool;

        /// <summary>Processes input data in parallel chunks via distributed LocalTool calls.</summary>
        public async Task<string> ProcessParallel() {
            if (LocalTerminal.Shell.In == null) {
                return "No input stream provided";
            }

            using var reader = new StreamReader(LocalTerminal.Shell.In);
            string inputData = await reader.ReadToEndAsync();

            string[] lines = inputData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string[]> chunks = lines.Chunk(Math.Max(1, lines.Length / Environment.ProcessorCount));

            // Each chunk calls ProcessChunk on itself via HTTP (cloud-native distributed execution)
            LocalTerminal.Shell = new LocalTool();
            IEnumerable<Task<string>> tasks = chunks.Select(async chunk => {
                var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join('\n', chunk)));
                return await CMDAsync(nameof(ProcessChunk), inputStream);
            });

            string[] results = await Task.WhenAll(tasks);
            return string.Join('\n', results);
        }

        /// <summary>Processes a single chunk by delegating to Python.</summary>
        public string ProcessChunk() {
            LocalTerminal.Shell = _pythonTool;
            return CMD("process_chunk", LocalTerminal.Shell.In);
        }
    }
}
