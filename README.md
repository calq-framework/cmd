[![NuGet Version](https://img.shields.io/nuget/v/CalqFramework.Cmd?color=508cf0)](https://www.nuget.org/packages/CalqFramework.Cmd)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CalqFramework.Cmd?color=508cf0)](https://www.nuget.org/packages/CalqFramework.Cmd)
[![REUSE status](https://api.reuse.software/badge/github.com/calq-framework/cmd)](https://api.reuse.software/info/github.com/calq-framework/cmd)

# Calq CMD  
Calq CMD introduces distributed, shell-style scripting to C#, turning complex systems into simple scripts. Calq CMD makes it possible to build cross-platform tools, streaming data pipelines, parallel batch workloads, HPC processes, and AI-powered systems with unprecedented simplicity.

Easy cloud-native distributed computing - scale from single-process development to distributed microservices without changing your code.

Supports Bash on Windows via WSL and Cygwin/MinGW/MSYS2. Python interoperability provided via high-performance asynchronous HTTP/2 server.

## Shell-Style Scripting for C#
Calq CMD provides a set of static APIs that let you write C# in a style that mimics Unix shell scripts, with full async support, parallel pipeline execution, and automatic stream handling.
```csharp
string echo = CMD("echo Hello World");
RUN($"echo {echo}"); // prints "Hello World"
string output = CMDV("echo data") | CMDV("grep pattern");
```

## How Calq CMD Stacks Up

### Calq CMD vs. Managed Batch Services
| Feature | Calq CMD on Kubernetes | Managed Batch Services (Azure/Google/AWS) |
| :--- | :--- | :--- |
| **Runnable Workloads** | C#/Python Code & Scripts & Containers | Scripts & Containers |
| **Job Definition** | C# | Provider-Specific JSON/YAML |
| **Orchestration** | C# & Kubernetes CLI | Provider-Specific SDK/CLI |
| **Scripting Languages** | C# & Bash/PowerShell | Bash/PowerShell |
| **SDK Languages** | C# | All major languages |
| **Infrastructure as Code** | Terraform & Kubernetes Manifests | Terraform & Provider-Specific IaC |
| **Monitoring** | Kubernetes | Provider-Specific |
| **Distributed Computing** | ✅ | ✅ |
| **Composable Pipes**| ✅ | ✅ |
| **Stream Redirection** | ✅ | ✅ (via storage services) |
| **Real-Time Streaming** | ✅ | ❌ |
| **Open Source** | ✅ | ❌ |
| **Fully Local Development** | ✅ | ❌ |
| **On-Premise Deployment** | ✅ | ❌ |
| **Infrastructure Cost** | Underlying Resources | Underlying Resources |
| **Development Time** | Fast to Moderate | Moderate to Slow |

### Calq CMD vs. CliWrap
| Feature | Calq CMD | CliWrap |
| :--- | :--- | :--- |
| **Programming Model** | Shell-Style Scripting & Object Model | Fluent Builder Pattern |
| **Real-Time Streaming** | Direct Stream Control | Structured Event Stream |
| **Local Process Execution** | ✅ | ✅ |
| **Composable Pipes** | ✅ | ✅ |
| **Stream Redirection** | ✅ | ✅ |
| **Distributed Computing** | ✅ | ❌ |
| **Context-Aware Shell** | ✅ | ❌ |
| **Platform-Aware Shell** | ✅ | ❌ |
| **Shell Customization** | ✅ | ❌ |
| **Native Python Execution** | ✅ | ❌ |
| **Development Time**| Fast | Fast to Moderate |

### Calq CMD vs. Python Microservices
| Feature | Calq CMD | Python Microservices |
| :--- | :--- | :--- |
| **Project Model** | Single Application | Distributed System |
| **Deployment Artifacts** | Single | Multiple |
| **Real-Time Streaming** | ✅ | ✅ (via custom SSE or WebSocket) |
| **Sub-ms Latency** | ✅ | ❌ |
| **Development Time** | Fast | Moderate to Slow |

### Calq CMD via Vibe Coding vs. n8n
| Feature | Calq CMD via Vibe Coding | n8n |
| :--- | :--- | :--- |
| **Development Model** | Code-First C# | Visual Workflow Builder |
| **Custom Code Support** | Any language (via shell) | JavaScript & Python (in nodes) |
| **Integrations** | 500,000+ NuGet packages | 400+ pre-built visual nodes |
| **Open Source** | ✅ | ✅ |
| **Fully Local Development** | ✅ | ✅ |
| **On-Premise Deployment** | ✅ | ✅ |
| **Modular Development** | ✅ | ✅ (sub-workflows) |
| **AI Code Generation** | ✅ | ✅ (JSON templates) |
| **AI Debugging** | ✅ | ❌ |
| **AI Testing** | ✅ | ❌ |
| **Development Time** | Very Fast | Fast to Moderate |

## Usage - Calq CMD

### 1. Application Setup & Initialization

*How to bootstrap the terminal and configure the execution environment.*

#### How to Set Up LocalTerminal

`LocalTerminal` is the central configuration context for Calq CMD. It stores shell, output stream, and logger settings in `AsyncLocal<T>` for thread/task isolation.

```csharp
using static CalqFramework.Cmd.Terminal;

// Default configuration - CommandLine shell, Console.OpenStandardOutput(), TerminalLogger
LocalTerminal.Shell = new CommandLine();
LocalTerminal.Out = Console.OpenStandardOutput(); // default
LocalTerminal.TerminalLogger = new TerminalLogger(); // default, logs "RUN: command"
```

**Configuring the shell:**

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

// Use Bash (with automatic WSL path mapping on Windows)
LocalTerminal.Shell = new Bash();

// Use CommandLine (cmd.exe on Windows, /bin/sh on Unix)
LocalTerminal.Shell = new CommandLine();

// Use Bash with custom input stream
LocalTerminal.Shell = new Bash() {
    In = Console.OpenStandardInput()
};
```

**Suppressing RUN logging:**

```csharp
using CalqFramework.Cmd.TerminalComponents;

// Disable logging for RUN operations
LocalTerminal.TerminalLogger = new NullTerminalLogger();

// Custom logger output
LocalTerminal.TerminalLogger = new TerminalLogger() { Out = myTextWriter };
```

**Key points:**
- `LocalTerminal.Shell` defaults to `CommandLine`
- `LocalTerminal.Out` defaults to `Console.OpenStandardOutput()`
- `LocalTerminal.TerminalLogger` defaults to `TerminalLogger` which logs `"RUN: command"` to `Console.Out`
- All settings are `AsyncLocal` — each thread/task maintains its own configuration

See also: [How to Isolate Execution Contexts](#how-to-isolate-execution-contexts)

---

### 2. Execution & Task Orchestration

*How to run commands, chain pipelines, and execute parallel workloads.*

#### How to Execute Commands with CMD/RUN

Calq CMD provides static APIs that mimic Unix shell scripting.

**CMD — returns output as string:**

```csharp
using static CalqFramework.Cmd.Terminal;

string result = CMD("echo Hello World"); // "Hello World" (trailing newline trimmed)
```

**CMDAsync — async version:**

```csharp
string result = await CMDAsync("echo Hello World");
```

**CMD with custom input stream:**

```csharp
using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("input data"));
string result = CMD("cat", inputStream);
```

**RUN — streams to LocalTerminal.Out, reads from LocalTerminal.Shell.In:**

```csharp
RUN("echo Hello World"); // prints to LocalTerminal.Out
await RUNAsync("echo Hello World");
```

**RUN with custom streams:**

```csharp
using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("input"));
RUN("cat", inputStream); // custom input, output to LocalTerminal.Out
RUN("cat", inputStream, outputStream); // custom input and output
```

**CMDStream — returns output as a stream for real-time processing:**

```csharp
using var stream = CMDStream("tail -f logfile");
using var reader = new StreamReader(stream);
string line = reader.ReadLine();
```

**Key points:**
- `CMD` returns output as string with trailing newline trimmed by the shell's `Postprocessor`
- `RUN` reads from `LocalTerminal.Shell.In` and writes to `LocalTerminal.Out`; operations are logged via `LocalTerminal.TerminalLogger`
- `CMDStream`/`CMDStreamAsync` return `ShellWorkerOutputStream` for real-time stream processing
- All variants support optional `TimeSpan` timeout or `CancellationToken`

See also: [How to Chain Pipelines](#how-to-chain-pipelines), [How to Work with ShellScript Instances](#how-to-work-with-shellscript-instances)

#### How to Chain Pipelines

Use `CMDV` to create `ShellScript` instances and chain them with the `|` operator. Pipeline steps run in parallel.

```csharp
using static CalqFramework.Cmd.Terminal;

// Pipeline chaining — each step runs in parallel
string output = CMDV("echo data") | CMDV("grep pattern");

// Multi-step pipeline — returns "Hello World" after ~1 second (not 3)
string output = CMDV("echo Hello World") | CMDV("sleep 1; cat") | CMDV("sleep 1; cat") | CMDV("sleep 1; cat");
```

**ShellScript implicit string conversion:**

```csharp
ShellScript echoCommand = CMDV("echo hello, world");

// Implicit conversion to string triggers evaluation
if ("hello, world" == echoCommand) { /* true */ }

string output = echoCommand | CMDV("cut -d',' -f1"); // "hello"
```

**Key points:**
- `CMDV` creates a `ShellScript` without executing it
- The `|` operator creates a pipeline where each step runs in parallel
- `ShellScript` has implicit conversion to `string` that triggers evaluation
- Errors in any pipeline step throw `ShellScriptException`

See also: [How to Work with ShellScript Instances](#how-to-work-with-shellscript-instances)

#### How to Work with ShellScript Instances

For advanced control, work directly with `ShellScript` instances.

```csharp
using static CalqFramework.Cmd.Terminal;

var script = new ShellScript(LocalTerminal.Shell, "echo Hello World");

// Evaluate — returns output as string
string result = script.Evaluate();
string asyncResult = await script.EvaluateAsync();

// Run — streams output to a provided stream
script.Run(Console.OpenStandardOutput());
await script.RunAsync(outputStream);

// Run with custom input
using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("input data"));
string result = script.Evaluate(inputStream);
await script.RunAsync(inputStream, outputStream);

// Start — returns a worker for manual stream control
using var worker = await script.StartAsync();
using var reader = new StreamReader(worker.StandardOutput);
string line = await reader.ReadLineAsync();
```

**Automatic vs manual worker disposal:**

```csharp
// Auto-disposed when output stream reading completes (default)
var worker = await script.StartAsync();

// Manual disposal — caller controls lifecycle
using var worker = await script.StartAsync(disposeOnCompletion: false);
```

See also: [How to Execute Commands with CMD/RUN](#how-to-execute-commands-with-cmdrun)

#### How to Execute Parallel and Distributed Jobs

Use `Task.Run` or `Task.WhenAll` to execute workloads in parallel. `AsyncLocal` isolation ensures each task has its own `LocalTerminal` configuration.

```csharp
using static CalqFramework.Cmd.Terminal;

var chunks = data.Chunk(Environment.ProcessorCount);

var tasks = chunks.Select(async chunk => {
    var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join('\n', chunk)));
    return await CMDAsync("process-chunk", inputStream);
});

var results = await Task.WhenAll(tasks);
```

See also: [How to Isolate Execution Contexts](#how-to-isolate-execution-contexts), [How to Connect to Remote Nodes](#how-to-connect-to-remote-nodes)

---

### 3. Input/Output (I/O) & Stream Management

*How to handle data flowing in and out of commands.*

#### How to Handle Real-Time Data Streams

Use `CMDStream`/`CMDStreamAsync` for real-time processing of continuous data.

```csharp
using static CalqFramework.Cmd.Terminal;

// Stream command output in real-time
using var stream = CMDStream("tail -f logfile");
using var reader = new StreamReader(stream);
while (true) {
    string? line = reader.ReadLine();
    if (line == null) break;
    Console.WriteLine(line);
}

// Async streaming
using var stream = await CMDStreamAsync("tail -f logfile");
using var reader = new StreamReader(stream);
string? line = await reader.ReadLineAsync();
```

**Using workers for fine-grained stream control:**

```csharp
ShellScript cmd = CMDV("tail -F /var/log/messages") | CMDV("grep -i 'error'");
using var worker = await cmd.StartAsync();
using var reader = new StreamReader(worker.StandardOutput);
try {
    var line = await reader.ReadLineAsync();
} catch (ShellWorkerException ex) {
    var errorCode = ex.ErrorCode;
    var errorMessage = await worker.ReadErrorMessageAsync();
}
```

See also: [How to Execute Commands with CMD/RUN](#how-to-execute-commands-with-cmdrun)

#### How to Process Binary Data

All shells support binary input/output streams, preserving raw byte data without text encoding corruption.

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

// Binary data through Bash
byte[] imageData = File.ReadAllBytes("image.png");
LocalTerminal.Shell = new Bash() { In = new MemoryStream(imageData) };
using var outputStream = CMDStream("convert - -resize 50% -"); // ImageMagick resize
byte[] resizedImage = ReadAllBytes(outputStream);
```

**Binary data with CMDStream and custom input:**

```csharp
byte[] binaryInput = File.ReadAllBytes("data.bin");
using var inputStream = new MemoryStream(binaryInput);
using var outputStream = CMDStream("process-binary", inputStream);

byte[] buffer = new byte[4096];
int bytesRead = await outputStream.ReadAsync(buffer);
```

See also: [How to Handle Real-Time Data Streams](#how-to-handle-real-time-data-streams)

#### How to Redirect Standard Input/Output

Control input and output streams globally or per-command.

**Global redirection via LocalTerminal:**

```csharp
using static CalqFramework.Cmd.Terminal;

// Redirect shell input globally
LocalTerminal.Shell = new Bash() {
    In = new MemoryStream(Encoding.UTF8.GetBytes("input data"))
};

// Redirect terminal output globally
LocalTerminal.Out = new FileStream("output.log", FileMode.Create);

// Suppress output
LocalTerminal.Out = Stream.Null;
```

**Per-command redirection:**

```csharp
// CMD with custom input
string result = CMD("cat", new MemoryStream(Encoding.UTF8.GetBytes("hello")));

// RUN with custom input and output
RUN("process", inputStream, outputStream);
```

**Key points:**
- `LocalTerminal.Shell.In` is the default input for `RUN` operations (default: `null`)
- `LocalTerminal.Out` is the default output for `RUN` operations (default: `Console.OpenStandardOutput()`)
- Per-command overloads take precedence over global settings

See also: [How to Set Up LocalTerminal](#how-to-set-up-localterminal)

---

### 4. Data Processing & Serialization

*How to deserialize command output into typed objects.*

#### How to Deserialize Output to Strongly-Typed Objects

CMD and CMDAsync support automatic JSON deserialization using `System.Text.Json`.

```csharp
using static CalqFramework.Cmd.Terminal;

// Deserialize JSON output directly
var config = CMD<ConfigObject>("kubectl get configmap my-config -o json");
var users = await CMDAsync<List<User>>("curl -s https://api.example.com/users");

// Works with input streams too
using var inputStream = new MemoryStream();
var result = CMD<ApiResponse>("process-data", inputStream);
var asyncResult = await CMDAsync<ApiResponse>("process-data", inputStream);
```

**Using ShellScript instances:**

```csharp
var script = new ShellScript(LocalTerminal.Shell, "get-data");
var data = script.Evaluate<MyDataType>();
var asyncData = await script.EvaluateAsync<MyDataType>();
```

**Key points:**
- Throws `JsonException` when output is not valid JSON
- Returns `null` when the JSON output is `"null"`
- Uses `System.Text.Json.JsonSerializer.Deserialize<T>` internally

See also: [How to Execute Commands with CMD/RUN](#how-to-execute-commands-with-cmdrun)

---

### 5. State & Context Management

*How to maintain isolation and manage environments across concurrent tasks.*

#### How to Isolate Execution Contexts

`LocalTerminal` settings are stored in `AsyncLocal<T>`, so threads and tasks act like subshells — changes in one context don't leak into another.

```csharp
using static CalqFramework.Cmd.Terminal;

CD("/tmp");
Task.Run(() => {
    CD("/var");
    Console.WriteLine(PWD); // prints "/var"
});
Console.WriteLine(PWD); // prints "/tmp"
```

**Shell isolation across tasks:**

```csharp
LocalTerminal.Shell = new CommandLine();
IShell originalShell = LocalTerminal.Shell;

await Task.Run(() => {
    LocalTerminal.Shell = new Bash(); // only affects this task
    // Bash is active here
});

Assert.Equal(originalShell, LocalTerminal.Shell); // still CommandLine
```

**Key points:**
- `LocalTerminal.Shell`, `LocalTerminal.Out`, `LocalTerminal.TerminalLogger`, and `PWD` are all `AsyncLocal`
- Each `Task.Run` or async task inherits the parent's values but changes are isolated
- This enables safe parallel execution without locking or shared state

See also: [How to Set Up LocalTerminal](#how-to-set-up-localterminal)

#### How to Manage Working Directory and Path Mapping

`PWD` returns the working directory mapped to the shell's internal path format. `LocalTerminal.WorkingDirectory` returns the host's absolute path.

```csharp
using static CalqFramework.Cmd.Terminal;

// Working directory
Console.WriteLine(LocalTerminal.WorkingDirectory); // e.g. "C:\Users"
Console.WriteLine(PWD);                            // e.g. "/mnt/c/Users" (WSL)

// Change directory
CD("/tmp");
CD(".."); // relative paths work

// Programmatic path mapping
LocalTerminal.Shell.MapToInternalPath("C:\\temp");  // "/mnt/c/temp" (WSL)
LocalTerminal.Shell.MapToHostPath("/mnt/c/temp");   // "C:\temp" (WSL)
```

**Key points:**
- On Windows with WSL Bash, `PWD` automatically maps to WSL paths
- On Linux or with `CommandLine` shell, `PWD` and `WorkingDirectory` are effectively the same
- `CD` works like Unix `cd` — supports absolute and relative paths
- Path mapping is shell-specific: `Bash` handles WSL translation, `CommandLine` uses `Path.GetFullPath`

See also: [How to Isolate Execution Contexts](#how-to-isolate-execution-contexts)

---

### 6. Error Handling & Resource Management

*How to handle failures and manage resources.*

#### How to Handle Execution Failures

Failed commands throw `ShellScriptException` with exit codes and error details.

```csharp
using static CalqFramework.Cmd.Terminal;

try {
    CMD("nonexistent-command");
} catch (ShellScriptException ex) {
    Console.WriteLine($"Exit Code: {ex.ErrorCode}");
    Console.WriteLine($"Details: {ex.Message}");
    // Message contains: command text, error details, and stderr output
}
```

**Worker-level error handling with `ShellWorkerException`:**

```csharp
var script = new ShellScript(LocalTerminal.Shell, "failing-command");
using var worker = await script.StartAsync(disposeOnCompletion: false);
using var reader = new StreamReader(worker.StandardOutput);
try {
    string output = await reader.ReadToEndAsync();
} catch (ShellWorkerException ex) {
    Console.WriteLine($"Worker failed with code: {ex.ErrorCode}");
    string errorDetails = await worker.ReadErrorMessageAsync();
    Console.WriteLine($"Error output: {errorDetails}");
}
```

**Key points:**
- `ShellScriptException` is thrown by `CMD`, `RUN`, `Evaluate`, and `Run` methods — contains the full error context including command text and stderr
- `ShellWorkerException` is thrown when reading from `worker.StandardOutput` — provides raw exit code access
- Use `worker.ReadErrorMessageAsync()` to retrieve stderr content for diagnostics
- Pipeline errors in any step throw `ShellScriptException`

See also: [How to Execute Commands with CMD/RUN](#how-to-execute-commands-with-cmdrun)

#### How to Manage Automatic Resource Cleanup

Workers support automatic disposal and all spawned processes are tracked for cleanup.

**Automatic worker disposal:**

```csharp
// Auto-disposed when output stream reading reaches end-of-stream or stream is disposed
var worker = await cmd.StartAsync();

// Manual disposal — caller controls lifecycle
using var worker = await cmd.StartAsync(disposeOnCompletion: false);
```

**Automatic process cleanup:**

```csharp
// All spawned processes are automatically terminated when the application exits, preventing orphaned processes
CMD("long-running-command");
// If application exits unexpectedly, child processes are automatically killed
```

**Automatic output processing:**

```csharp
// CMD automatically trims trailing newlines via the default `ShellScriptPostprocessor`
string result = CMD("echo 'hello world'"); // "hello world" (no \n)
```

---

### 7. Extensibility & Customization

*How to customize shells and create wrappers.*

#### How to Create Custom Shell Wrappers with ShellTool

`ShellTool` wraps an existing shell and prepends a command to all executed scripts.

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

// All commands run with sudo
LocalTerminal.Shell = new ShellTool(new Bash(), "sudo");
RUN("apt update");

// All commands run as git subcommands
LocalTerminal.Shell = new ShellTool(new Bash(), "git");
RUN("commit -m 'automated message'");

// Nested wrappers
LocalTerminal.Shell = new ShellTool(new ShellTool(new Bash(), "sudo"), "docker");
RUN("ps"); // executes: sudo docker ps
```

See also: [How to Set Up LocalTerminal](#how-to-set-up-localterminal)

#### How to Connect to Remote Nodes

Use `HttpTool` to execute commands on remote HTTP servers that comply with the `HttpToolWorker` protocol.

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com/cmd/") };
LocalTerminal.Shell = new HttpTool(httpClient);

// Commands execute on the remote server
string result = CMD("MyCommand --param value");
RUN("ProcessData");
```

**Using LocalTool for seamless local-to-distributed execution:**

```csharp
// LocalTool automatically adapts based on context
LocalTerminal.Shell = new LocalTool();

// In development: executes locally via CommandLine shell
// In production with CalqCmdController: executes via HTTP
string result = CMD("echo Hello World");
```

**Key points:**
- `HttpTool` wraps `HttpClient` for remote command execution
- `LocalTool` uses `LocalToolFactory` to automatically choose between local and HTTP execution
- Same code works in both local development and distributed production environments

See also: [How to Execute Parallel and Distributed Jobs](#how-to-execute-parallel-and-distributed-jobs)

---
---

## Usage - Calq CMD with Python

### 1. Application Setup & Initialization

*How to set up PythonToolServer and configure PythonTool shell.*

#### How to Set Up PythonToolServer

`PythonToolServer` starts an HTTPS server that executes Python scripts compatible with [Python Fire](https://github.com/google/python-fire). `PythonTool` is the shell that communicates with it over HTTP/2.

**Python script (tool.py):**

```python
# tool.py
import fire

def add(x: int, y: int):
    return x + y

def upper(msg: str = "hello"):
    return msg.upper()

if __name__ == "__main__":
    fire.Fire() # ignored by PythonToolServer but required to run tool.py from console
```

**C# setup:**

```csharp
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

var pts = new PythonToolServer("tool.py");
using var worker = await pts.StartAsync();
LocalTerminal.Shell = new PythonTool(pts);
```

**Key points:**
- `PythonToolServer` embeds its own `server.py`, generates SSL certificates, and finds an available port automatically
- The Python script must be compatible with Python Fire
- `fire.Fire()` in `__main__` is ignored by PythonToolServer but allows running the script directly from console
- `PythonToolServer.Shell` defaults to `CommandLine` for starting the server process
- `StartAsync()` returns a worker — the server runs in the background

See also: [How to Execute Python Commands](#how-to-execute-python-commands)

---

### 2. Execution & Task Orchestration

*How to execute Python functions from C#.*

#### How to Execute Python Commands

Once `PythonTool` is configured, use the same `CMD`/`RUN` APIs to call Python functions.

```csharp
using static CalqFramework.Cmd.Terminal;

// Call Python functions like shell commands
RUN("add 9 1");           // prints "10"
RUN("upper --msg world"); // prints "WORLD"

// Get return values
string result = CMD("add 9 1"); // "10"
string upper = CMD("upper --msg world"); // "WORLD"
```

**Key points:**
- Python Fire argument syntax applies: positional args and `--named` args
- `CMD` returns the Python function's return value as a string
- `RUN` streams output to `LocalTerminal.Out`

See also: [How to Set Up PythonToolServer](#how-to-set-up-pythontoolserver)

---

### 3. Input/Output (I/O) & Stream Management

*How to handle streaming and binary data with Python.*

#### How to Stream Data with Python

PythonToolServer supports real-time streaming via Python async generators. For text input, the server consumes the entire stream before executing. Output is unbuffered and streamed directly over HTTP/2.

**Text streaming (Python):**

```python
# async-tool.py
import asyncio
import sys

async def test():
    for line in sys.stdin:
        await asyncio.sleep(1)
        yield line

if __name__ == "__main__":
    import fire
    fire.core._PrintResult = lambda component_trace, verbose=False, serialize=None: None
    value = fire.Fire()

    async def main():
        async for part in value:
            print(part, end="")

    asyncio.run(main())
```

**C# usage:**

```csharp
var pts = new PythonToolServer("async-tool.py");
using var worker = await pts.StartAsync();
LocalTerminal.Shell = new PythonTool(pts) {
    In = new MemoryStream(Encoding.ASCII.GetBytes(" one\n two\n three\n"))
};
RUN("test"); // prints each line every second
```

See also: [How to Process Binary Data with Python](#how-to-process-binary-data-with-python)

#### How to Process Binary Data with Python

Python scripts can access `sys.stdin.buffer` for raw byte streams. Binary output is streamed via async generators.

**Binary streaming (Python):**

```python
# binary-tool.py
import asyncio
import sys

async def test_binary():
    buffer = sys.stdin.buffer
    chunk_size = 4096

    while True:
        chunk = buffer.read(chunk_size)
        if not chunk:
            break
        yield chunk

if __name__ == "__main__":
    import fire
    fire.core._PrintResult = lambda component_trace, verbose=False, serialize=None: None
    value = fire.Fire()

    async def main():
        async for part in value:
            print(part, end="")

    asyncio.run(main())
```

**C# usage:**

```csharp
var pts = new PythonToolServer("binary-tool.py");
using var worker = await pts.StartAsync();

byte[] binaryData = new byte[256];
for (int i = 0; i < 256; i++) binaryData[i] = (byte)i;

LocalTerminal.Shell = new PythonTool(pts) {
    In = new MemoryStream(binaryData)
};
RUN("test_binary"); // streams binary data through Python and back
```

**Key points:**
- Text input: server consumes the entire stream before executing the Python function
- Binary input: use `sys.stdin.buffer` in Python for raw byte access
- Output: streamed in real-time via async generators over HTTP/2
- Python scripts requiring real-time input streaming (where input arrives progressively during execution) must be executed directly via `python` using `Bash` or `CommandLine` shells

See also: [How to Stream Data with Python](#how-to-stream-data-with-python)

---

### 4. Data Processing & Serialization

*Covered by [Calq CMD — How to Deserialize Output to Strongly-Typed Objects](#how-to-deserialize-output-to-strongly-typed-objects). JSON deserialization works identically with PythonTool.*

---

### 5. State & Context Management

*Covered by [Calq CMD — How to Isolate Execution Contexts](#how-to-isolate-execution-contexts). AsyncLocal isolation works identically with PythonTool.*

---

### 6. Error Handling & Resource Management

*How to handle Python execution failures.*

#### How to Handle Python Execution Failures

Python errors are surfaced through the same exception types as shell commands.

```csharp
try {
    RUN("nonexistent_function");
} catch (ShellScriptException ex) {
    Console.WriteLine($"Exit Code: {ex.ErrorCode}");
    Console.WriteLine($"Details: {ex.Message}");
}
```

**Reading detailed Python error messages via workers:**

```csharp
var script = new ShellScript(LocalTerminal.Shell, "test_throw_exception");
using var worker = await script.StartAsync(inputStream, disposeOnCompletion: false);
using var reader = new StreamReader(worker.StandardOutput);

try {
    string output = await reader.ReadToEndAsync();
} catch (ShellWorkerException ex) {
    string errorMessage = await worker.ReadErrorMessageAsync();
    // errorMessage contains the Python traceback and exception details
}
```

**Key points:**
- `ShellScriptException` wraps Python errors with exit code and stderr content
- `worker.ReadErrorMessageAsync()` retrieves the full Python traceback
- Partial output may be available before the error occurs (e.g., in streaming scenarios)

See also: [How to Handle Execution Failures](#how-to-handle-execution-failures)

---

### 7. Extensibility & Customization

*Covered by [Calq CMD — How to Connect to Remote Nodes](#how-to-connect-to-remote-nodes). PythonTool uses the same HttpToolWorker protocol as HttpTool.*

---
---

## Usage - Calq CMD ASP.NET Core

Calq CMD ASP.NET Core uses [Calq CLI](https://github.com/calq-framework/cli) for command parsing and execution. Command targets follow Calq CLI conventions for [submodules, subcommands, options, and parameters](https://github.com/calq-framework/cli?tab=readme-ov-file#2-command-structure--hierarchy).

### 1. Application Setup & Initialization

*How to register CalqCmdController and configure the web API.*

#### How to Register CalqCmdController

`AddCalqCmdController` registers the controller, command executor, distributed error cache, and `LocalHttpToolFactory` for `LocalTool` auto-discovery.

```csharp
using CalqFramework.Cmd.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Your command target object — methods become executable commands
var myCommandTarget = new MyCommands();

// Register CalqCmdController
builder.Services.AddCalqCmdController(myCommandTarget);

var app = builder.Build();
app.MapControllers();
app.Run();
```

**With configuration options:**

```csharp
builder.Services.AddCalqCmdController(myCommandTarget, options =>
{
    options.RoutePrefix = "api/cmd";                        // Custom route prefix (default: "CalqCmd")
    options.HttpClientTimeout = TimeSpan.FromMinutes(5);    // HTTP timeout for LocalTool
});
```

**With distributed error cache configuration:**

```csharp
builder.Services.AddCalqCmdController(myCommandTarget, null, cacheOptions =>
{
    cacheOptions.ErrorCacheExpiration = TimeSpan.FromHours(2);
    cacheOptions.ErrorCacheKeyPrefix = "MyApp.Errors:";
});
```

**Using a factory for the command target:**

```csharp
builder.Services.AddCalqCmdController(provider => {
    var config = provider.GetRequiredService<IConfiguration>();
    return new MyCommands(config);
});
```

**What `AddCalqCmdController` registers automatically:**
- `CalqCommandExecutor` (uses CalqFramework.Cli with as-is naming) as `ICalqCommandExecutor`
- `LocalHttpToolFactory` as `ILocalToolFactory` for `LocalTool` auto-discovery
- `LocalTerminalFilter` as a global action filter (configures `LocalTerminal.Out`, `Shell`, and `TerminalLogger` per request)
- `DistributedMemoryCache` as fallback if no `IDistributedCache` is registered

See also: [How to Use Built-in Shell Attributes](#how-to-use-built-in-shell-attributes), [How to Implement Custom Command Executors](#how-to-implement-custom-command-executors)

---

### 2. Execution & Task Orchestration

*How to execute commands via HTTP and route requests.*

#### How to Execute Commands via HTTP

CalqCmdController exposes commands via GET (query string) and POST (header) requests.

**Using query strings (GET — browser-friendly):**

```http
GET http://localhost:5000/CalqCmd?cmd=--help
GET http://localhost:5000/CalqCmd?cmd=Add --a 5 --b 3
```

**Using headers (POST — supports input streams):**

```http
POST http://localhost:5000/CalqCmd
cmd: Add --a 5 --b 3

POST http://localhost:5000/CalqCmd
cmd: --help
```

**Using C# client with HttpTool:**

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5000/CalqCmd/") };
LocalTerminal.Shell = new HttpTool(httpClient);

string result = CMD("Add --a 5 --b 3"); // "8"
string help = CMD("--help");             // lists available commands
```

**Using LocalTool for automatic discovery:**

```csharp
// LocalTool automatically discovers CalqCmdController endpoints
LocalTerminal.Shell = new LocalTool();
string result = CMD("MyCommand --param value");
```

See also: [How to Register CalqCmdController](#how-to-register-calqcmdcontroller)

#### How to Define Command Targets

Command target methods become executable commands via CalqCmdController. The default `CalqCommandExecutor` uses [Calq CLI](https://github.com/calq-framework/cli) and preserves method and parameter names as-is (no kebab-case transformation).

```csharp
public class MyCommands
{
    // String return — sent as text/plain
    public string ProcessData(string input) => input.ToUpper();

    // Numeric return — sent as JSON
    public int Add(int a, int b) => a + b;

    // Stream return — sent as application/octet-stream
    public Stream GetTestStream()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("stream content");
        return new MemoryStream(bytes);
    }

    // Void method — can write to LocalTerminal.Out or use RUN
    public void VoidMethodWithRUN()
    {
        LocalTerminal.TerminalLogger = new NullTerminalLogger();
        RUN("dotnet --version");
    }

    // Void method — write directly to response body
    public void VoidMethodWithDirectOutput()
    {
        var sw = new StreamWriter(LocalTerminal.Out);
        sw.Write("Direct output");
        sw.Flush();
    }

    // Async method with input stream
    public async Task<string> ProcessDataFromStream()
    {
        if (LocalTerminal.Shell.In == null) return "No input";
        using var reader = new StreamReader(LocalTerminal.Shell.In);
        string data = await reader.ReadToEndAsync();
        return $"Processed: {data.Trim().ToUpper()}";
    }

    // Async binary processing
    public async Task ProcessBinaryData()
    {
        if (LocalTerminal.Shell.In == null) return;
        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = await LocalTerminal.Shell.In.ReadAsync(buffer)) > 0)
        {
            await LocalTerminal.Out.WriteAsync(buffer.AsMemory(0, bytesRead));
        }
    }
}
```

**Client usage with `nameof()`:**

```csharp
LocalTerminal.Shell = new HttpTool(httpClient);
string methodName = nameof(MyCommands.ProcessData);
string result = CMD($"{methodName} --input test"); // "TEST"
```

**Key points:**
- `CalqCommandExecutor` uses as-is naming — `nameof()` works directly for method names
- `LocalTerminal.Shell.In` provides the HTTP request body as input stream
- `LocalTerminal.Out` is set to the HTTP response body by `LocalTerminalFilter`
- Void methods and `Task` methods return `EmptyResult` — use `RUN` or `LocalTerminal.Out` to write to the response
- `string` returns are sent as `text/plain`, `Stream` as `application/octet-stream`, other objects as JSON

See also: [How to Register CalqCmdController](#how-to-register-calqcmdcontroller)

---

### 3. Input/Output (I/O) & Stream Management

*How to handle streaming in ASP.NET Core controllers.*

#### How to Stream Data via HTTP

Command targets can return `Stream` or `Task<Stream>` for streaming responses. Use `CMDStreamAsync` in custom controllers.

**Streaming from command targets:**

```csharp
public class MyCommands
{
    public async Task<Stream> StreamResults()
    {
        if (LocalTerminal.Shell.In == null) return new MemoryStream();
        using var reader = new StreamReader(LocalTerminal.Shell.In);
        var data = await reader.ReadToEndAsync();
        var resultBytes = Encoding.UTF8.GetBytes($"Streaming result: {data.Trim()}");
        return new MemoryStream(resultBytes);
    }
}
```

**Streaming from custom controllers:**

```csharp
[ApiController, UseBashShell]
public class DataController : ControllerBase
{
    [HttpGet]
    [Produces("text/plain")]
    public async Task<Stream> StreamData() => await CMDStreamAsync("process-large-dataset");

    [HttpPost]
    [Produces("application/json")]
    public async Task<Stream> ProcessUpload(IFormFile file)
    {
        LocalTerminal.Shell = new CommandLine(); // Override controller-level attribute
        return await CMDStreamAsync("analyze-file", file.OpenReadStream());
    }
}
```

See also: [How to Use Built-in Shell Attributes](#how-to-use-built-in-shell-attributes)

---

### 4. Data Processing & Serialization

*How CalqCommandExecutor handles naming conventions.*

#### How to Preserve Parameter Naming Conventions

The default `CalqCommandExecutor` uses `AsIsClassMemberStringifier` which preserves original method and parameter names without transformation. This differs from [Calq CLI](https://github.com/calq-framework/cli)'s default kebab-case conversion.

```csharp
public class MyCommands
{
    public string ProcessData(string input) => input.ToUpper();
    public string process_snake_case(string data) => data.ToLower();
}

// Client usage — names are preserved as-is
LocalTerminal.Shell = new HttpTool(httpClient);
string result = CMD("ProcessData --input test");           // PascalCase works
result = CMD("process_snake_case --data TEST");            // snake_case works
// CMD("process-data --input test");                       // kebab-case does NOT work
```

**Key points:**
- `CalqCommandExecutor` preserves original C# naming — use `nameof()` for type-safe method references
- This is intentional: HTTP APIs typically use PascalCase or snake_case, not kebab-case
- To change this behavior, provide a custom `ICalqCommandExecutor`

See also: [How to Implement Custom Command Executors](#how-to-implement-custom-command-executors)

---

### 5. State & Context Management

*How LocalTerminal is configured per HTTP request.*

#### How to Apply Context via Shell Attributes

Built-in action filter attributes automatically configure `LocalTerminal.Shell` per request. No additional registration required.

#### How to Use Built-in Shell Attributes

```csharp
using CalqFramework.Cmd.AspNetCore.Attributes;

// Apply to entire controller
[ApiController, UseBashShell]
public class DataController : ControllerBase
{
    [HttpGet]
    public async Task<Stream> StreamData() => await CMDStreamAsync("process-data");

    // Override controller-level attribute for a specific action
    [HttpPost, UseCommandLineShell]
    public async Task<Stream> ProcessStream([FromBody] Stream data) =>
        await CMDStreamAsync("transform-data", data);
}
```

**Available built-in attributes:**
- `[UseBashShell]` — sets `LocalTerminal.Shell` to `Bash`
- `[UseCommandLineShell]` — sets `LocalTerminal.Shell` to `CommandLine`
- `[UseLocalToolShell]` — sets `LocalTerminal.Shell` to `LocalTool` (auto-adapts between local and distributed)
- `[UsePythonToolShell(pythonTool)]` — sets `LocalTerminal.Shell` to a `PythonTool` instance (can only be applied programmatically, not as a declarative attribute)

**Key points:**
- `LocalTerminalFilter` (registered by `AddCalqCmdController`) sets defaults for every request: `LocalTerminal.Out` = response body, `Shell` = `CommandLine`, `TerminalLogger` = `NullTerminalLogger`
- Shell attributes override the filter's default shell for the decorated controller or action
- You can also set `LocalTerminal.Shell` programmatically inside action methods

See also: [How to Create Custom Action Filters](#how-to-create-custom-action-filters)

#### How to Create Custom Action Filters

Create custom `ActionFilterAttribute` subclasses for specific shell configurations.

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

public class UseMyCustomShellAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LocalTerminal.Shell = new ShellTool(new Bash(), "sudo");
    }
}

[ApiController, UseMyCustomShell]
public class AdminController : ControllerBase
{
    [HttpGet("update")]
    public async Task<string> Update() => await CMDAsync("apt update");
}
```

See also: [How to Use Built-in Shell Attributes](#how-to-use-built-in-shell-attributes)

---

### 6. Error Handling & Resource Management

*How to handle errors in distributed HTTP scenarios.*

#### How to Handle Distributed Errors

CalqCmdController automatically caches errors with unique error codes. Clients retrieve error details via `ReadErrorMessageAsync`.

```csharp
using static CalqFramework.Cmd.Terminal;

var script = new ShellScript(LocalTerminal.Shell, "FailingCommand");
using var worker = await script.StartAsync(disposeOnCompletion: false);
using var reader = new StreamReader(worker.StandardOutput);
try {
    string output = await reader.ReadToEndAsync();
} catch (ShellWorkerException ex) {
    var errorCode = ex.ErrorCode; // unique error code from HTTP reset
    string fullErrorMessage = await worker.ReadErrorMessageAsync();
    // ReadErrorMessageAsync calls CalqCmdController/ReadErrorMessage to retrieve cached error
    Console.WriteLine($"Error {errorCode}: {fullErrorMessage}");
}
```

**Key points:**
- Errors are cached in `IDistributedCache` with configurable expiration (default: 1 hour)
- Error codes are derived from the exception's hash code
- `ReadErrorMessageAsync` on `HttpToolWorker` automatically calls the `ReadErrorMessage` endpoint
- Uses HTTP/2 RST_STREAM for error signaling

See also: [How to Register CalqCmdController](#how-to-register-calqcmdcontroller)

---

### 7. Extensibility & Customization

*How to customize command execution and help.*

#### How to Implement Custom Command Executors

Provide a custom `ICalqCommandExecutor` to change how commands are parsed and executed.

```csharp
using CalqFramework.Cmd.AspNetCore;

public class JsonRpcCommandExecutor(object target) : ICalqCommandExecutor
{
    private readonly object _target = target;

    public object? Execute(string[] args, TextWriter output)
    {
        var methodName = args[0];
        var method = _target.GetType().GetMethod(methodName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        var parameters = args.Length > 1
            ? JsonSerializer.Deserialize<object[]>(args[1])
            : Array.Empty<object>();

        return method?.Invoke(_target, parameters);
    }
}

// Register with custom executor
builder.Services.AddCalqCmdController(myCommandTarget, options =>
{
    options.CommandExecutor = new JsonRpcCommandExecutor(myCommandTarget);
});
```

**Key points:**
- `ICalqCommandExecutor.Execute` receives split args and an optional `TextWriter` for interface output
- The default `CalqCommandExecutor` uses CalqFramework.Cli with `AsIsClassMemberStringifier`
- Custom executors enable JSON-RPC, gRPC, or any other command execution strategy

See also: [How to Register CalqCmdController](#how-to-register-calqcmdcontroller)

#### How to Access Help for Command Targets

Help is automatically available via the `--help` flag, working exactly like a CLI tool but over HTTP.

```csharp
// Client usage
LocalTerminal.Shell = new HttpTool(httpClient);
string help = CMD("--help");
// Returns:
// Subcommands
//   ProcessData
//   Add

string commandHelp = CMD($"{nameof(MyCommands.Add)} --help");
// Returns:
// Parameters
//   -a  (Requires: int32)
//   -b  (Requires: int32)
```

**Via HTTP directly:**

```http
GET http://localhost:5000/CalqCmd?cmd=--help
GET http://localhost:5000/CalqCmd?cmd=Add --help
```

**Key points:**
- Help output is generated by `CalqCommandExecutor` using CalqFramework.Cli
- Add XML documentation comments and enable `<GenerateDocumentationFile>` for richer descriptions
- Help is written to the `interfaceOut` TextWriter, which streams to the HTTP response

See also: [How to Define Command Targets](#how-to-define-command-targets)

---
---

## Usage - Calq CMD ASP.NET Core with Python

### 1. Application Setup & Initialization

*How to register PythonTool services for ASP.NET Core dependency injection.*

#### How to Register PythonTool with Dependency Injection

Use `AddPythonTool` to register `PythonToolServer` and `PythonTool` for DI. Start the server during application startup.

```csharp
using CalqFramework.Cmd.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Register PythonTool with script path
builder.Services.AddPythonTool("path/to/your/script.py");

// Register CalqCmdController
builder.Services.AddCalqCmdController(new MyCommands());

var app = builder.Build();

// Start the Python server during application startup
await app.Services.StartPythonToolServerAsync();

app.MapControllers();
app.Run();
```

**Using a factory for complex configuration:**

```csharp
builder.Services.AddPythonTool(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var scriptPath = config["PythonScript:Path"];
    return new PythonToolServer(scriptPath);
});
```

**Key points:**
- `PythonToolServer` is registered as singleton (manages the Python process lifecycle)
- `PythonTool` is registered as transient (depends on `PythonToolServer`)
- `StartPythonToolServerAsync()` must be called before `PythonTool` can be resolved from DI
- The server generates SSL certificates and finds an available port automatically

See also: [How to Use PythonTool in Controllers](#how-to-use-pythontool-in-controllers)

---

### 2. Execution & Task Orchestration

*How to use PythonTool in ASP.NET Core controllers.*

#### How to Use PythonTool in Controllers

Use the `[UsePythonToolShell]` attribute or inject `PythonTool` via DI to execute Python functions from controllers.

**Using PythonTool in a controller:**

```csharp
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

[ApiController]
[Route("api/[controller]")]
public class PythonController : ControllerBase
{
    private readonly PythonTool _pythonTool;

    public PythonController(PythonTool pythonTool)
    {
        _pythonTool = pythonTool;
    }

    [HttpGet("add")]
    public string Add(int x, int y)
    {
        LocalTerminal.Shell = _pythonTool;
        return CMD($"add {x} {y}");
    }

    [HttpPost("process")]
    public async Task<string> Process()
    {
        LocalTerminal.Shell = _pythonTool;
        return await CMDAsync("upper --msg hello");
    }
}
```

**Key points:**
- Inject `PythonTool` via DI constructor and set `LocalTerminal.Shell` programmatically
- `[UsePythonToolShell]` can only be applied programmatically (it requires a `PythonTool` instance in its constructor, not as a declarative attribute)
- Python Fire argument syntax applies: positional args and `--named` args

See also: [How to Register PythonTool with Dependency Injection](#how-to-register-pythontool-with-dependency-injection)

---

### 3. Input/Output (I/O) & Stream Management

*How to stream data between HTTP clients and Python.*

#### How to Stream Between HTTP and Python

Combine ASP.NET Core request/response streams with PythonTool for end-to-end streaming.

```csharp
[ApiController]
[Route("api/[controller]")]
public class StreamController : ControllerBase
{
    private readonly PythonTool _pythonTool;

    public StreamController(PythonTool pythonTool)
    {
        _pythonTool = pythonTool;
    }

    [HttpPost("process-stream")]
    public async Task<Stream> ProcessStream()
    {
        // Request body → Python stdin, Python stdout → response body
        LocalTerminal.Shell = _pythonTool;
        return await CMDStreamAsync("test", Request.Body);
    }

    [HttpPost("process-binary")]
    public async Task ProcessBinary()
    {
        LocalTerminal.Shell = _pythonTool;
        await RUNAsync("test_binary", Request.Body);
    }
}
```

**Key points:**
- `Request.Body` provides the HTTP request body as a stream for Python's `sys.stdin`
- `CMDStreamAsync` returns a stream that can be directly returned from controller actions
- Binary data flows through Python and back without text encoding corruption

See also: [How to Process Binary Data with Python](#how-to-process-binary-data-with-python)

---

### 4. Data Processing & Serialization

*Covered by [Calq CMD — How to Deserialize Output to Strongly-Typed Objects](#how-to-deserialize-output-to-strongly-typed-objects) and [Calq CMD ASP.NET Core — How to Preserve Parameter Naming Conventions](#how-to-preserve-parameter-naming-conventions). Both apply identically when using PythonTool in ASP.NET Core.*

---

### 5. State & Context Management

*Covered by [Calq CMD ASP.NET Core — How to Apply Context via Shell Attributes](#how-to-apply-context-via-shell-attributes). Use `[UsePythonToolShell]` or set `LocalTerminal.Shell` programmatically.*

---

### 6. Error Handling & Resource Management

*Covered by [Calq CMD with Python — How to Handle Python Execution Failures](#how-to-handle-python-execution-failures) and [Calq CMD ASP.NET Core — How to Handle Distributed Errors](#how-to-handle-distributed-errors). Both apply when using PythonTool in ASP.NET Core.*

---

### 7. Extensibility & Customization

*Covered by [Calq CMD ASP.NET Core — How to Implement Custom Command Executors](#how-to-implement-custom-command-executors) and [Calq CMD — How to Create Custom Shell Wrappers with ShellTool](#how-to-create-custom-shell-wrappers-with-shelltool). Both apply when using PythonTool in ASP.NET Core.*

## Demo Examples
[Cloud-Native Data Processor Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CloudNative.DataProcessor)  

[Cloud-Native Python Data Processor Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CloudNativePython.DataProcessor)  

[Kubectl Wrapper CLI Tool Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CliTool.KubectlWrapper) (uses [Calq CLI](https://github.com/calq-framework/cli))  

## Quick Start

### Calq CMD
[QuickStart Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CalqCmd.QuickStart)  

```bash
git clone --branch latest https://github.com/calq-framework/cmd docs/cmd
dotnet new console -n QuickStart
cd QuickStart
cp ../docs/cmd/Examples/Example.CalqCmd.QuickStart/Program.cs ./Program.cs
dotnet add package CalqFramework.Cmd
dotnet run
```

### Calq CMD with Python
[QuickStart Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CalqCmdPython.QuickStart)  

```bash
git clone --branch latest https://github.com/calq-framework/cmd docs/cmd
dotnet new console -n QuickStart
cd QuickStart
cp ../docs/cmd/Examples/Example.CalqCmdPython.QuickStart/Program.cs ./Program.cs
cp ../docs/cmd/Examples/Example.CalqCmdPython.QuickStart/tool.py ./tool.py
dotnet add package CalqFramework.Cmd
dotnet run
```

### Calq CMD ASP.NET Core
[QuickStart Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CalqCmdAspNetCore.QuickStart)  

```bash
git clone --branch latest https://github.com/calq-framework/cmd docs/cmd
dotnet new web -n QuickStart
cd QuickStart
cp ../docs/cmd/Examples/Example.CalqCmdAspNetCore.QuickStart/Program.cs ./Program.cs
dotnet add package CalqFramework.Cmd.AspNetCore
dotnet run
```

### Calq CMD ASP.NET Core with Python
[QuickStart Example](https://github.com/calq-framework/cmd/tree/main/Examples/Example.CalqCmdAspNetCorePython.QuickStart)  

```bash
git clone --branch latest https://github.com/calq-framework/cmd docs/cmd
dotnet new web -n QuickStart
cd QuickStart
cp ../docs/cmd/Examples/Example.CalqCmdAspNetCorePython.QuickStart/Program.cs ./Program.cs
cp ../docs/cmd/Examples/Example.CalqCmdAspNetCorePython.QuickStart/tool.py ./tool.py
dotnet add package CalqFramework.Cmd.AspNetCore
dotnet run
```

## License  
Calq CMD is dual-licensed under GNU AGPLv3 and the Calq Commercial License.
