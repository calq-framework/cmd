using Example.CloudNativePython.DataProcessor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CalqCmdController).Assembly);
builder.Services.AddPythonTool("tool.py");
builder.Services.AddCalqCmdController(provider => new DataProcessingCommands(provider.GetRequiredService<PythonTool>()));

var app = builder.Build();
await app.Services.StartPythonToolServerAsync();
app.MapControllers();
app.Run();

namespace Example.CloudNativePython.DataProcessor {
    /// <summary>
    ///     Command target — ProcessParallel splits input into chunks and calls ProcessChunk
    ///     via LocalTool (distributed HTTP). ProcessChunk delegates to Python via PythonTool.
    /// </summary>
    public class DataProcessingCommands {
        private readonly PythonTool _pythonTool;

        public DataProcessingCommands(PythonTool pythonTool) => _pythonTool = pythonTool;

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

        /// <summary>Processes a single chunk by delegating to Python.</summary>
        public string ProcessChunk() {
            LocalTerminal.Shell = _pythonTool;
            return CMD("process_chunk", LocalTerminal.Shell.In);
        }
    }
}
