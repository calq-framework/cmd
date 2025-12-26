[![NuGet Version](https://img.shields.io/nuget/v/CalqFramework.Cmd?color=508cf0)](https://www.nuget.org/packages/CalqFramework.Cmd)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CalqFramework.Cmd?color=508cf0)](https://www.nuget.org/packages/CalqFramework.Cmd)

# Calq CMD  
Calq CMD introduces distributed, shell-style scripting to C#, turning complex systems into simple scripts. Build cross-platform tools, streaming data pipelines, parallel batch workloads, HPC processes, and AI-powered systems with unprecedented simplicity.

Supports Bash on Windows via WSL and Cygwin/MinGW/MSYS2. Python interoperability provided via high-performance asynchronous HTTP/2 server.

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

## Shell-style scripting in C#  
Calq CMD introduces a set of static APIs that allow writing in a style that mimics Unix shell scripts.
```csharp
string echo = CMD("echo Hello World");
RUN($"echo {echo}"); // prints "Hello World"
```
```csharp
string echo = await CMDAsync("echo Hello World");
await RUNAsync($"echo {echo}"); // prints "Hello World"
```

### CMD/RUN

*   **CMD** by default doesn't read from any stream and returns output as a string, with the last newline trimmed by `LocalTerminal.Shell.Postprocessor`.
    ```csharp
    string result = CMD("echo Hello World");
    ```

*   **CMDStream** is just like CMD but returns streams for real-time processing scenarios.
    ```csharp
    using var stream = CMDStream("tail -f logfile");
    ```

*   **CMDV** is just like CMD but returns `ShellScript` instances for pipeline chaining with the `|` operator. `ShellScripts` have implicit conversion to string that triggers processing and returns output.
    ```csharp
    string output = CMDV("echo data") | CMDV("grep pattern");
    ```

*   **RUN** by default reads from `LocalTerminal.Shell.In` and writes into `LocalTerminal.Out`. RUN operations are automatically logged via `LocalTerminal.TerminalLogger`.
    ```csharp
    RUN("read -r input; echo $input");
    ```
See [Terminal.cs](https://github.com/calq-framework/cmd/blob/main/CalqFramework.Cmd/Terminal.cs) for all available overloads.

### Working with ShellScript Instances
For advanced scenarios, you can work directly with `ShellScript` instances for more control:
```csharp
// Create script instances
var script = new ShellScript(LocalTerminal.Shell, "echo Hello World");

// Evaluate vs Run - choose based on your needs
string result = script.Evaluate();           // Returns output as string
script.Run(Console.OpenStandardOutput());    // Streams output to provided stream

// Async versions for better performance
string result = await script.EvaluateAsync();
await script.RunAsync(outputStream);

// Custom input handling
using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("input data"));
string result = script.Evaluate(inputStream);
await script.RunAsync(inputStream, outputStream);

// Advanced control with workers
using var worker = await script.StartAsync();
using var reader = new StreamReader(worker.StandardOutput);
string line = await reader.ReadLineAsync();
```

## Key Features

### Parallel Pipeline Execution
Pipelines are internally run asynchronously, and each pipeline step is run in parallel.  
The following returns "Hello World" after 1 second.
```csharp
string output = CMDV("echo Hello World") | CMDV("sleep 1; cat") | CMDV("sleep 1; cat") | CMDV("sleep 1; cat");
```

### Thread/Task Isolation with AsyncLocal
`LocalTerminal` settings are stored in `AsyncLocal<T>` so threads and tasks can be used like subshells.
```csharp
CD("/tmp");  
Task.Run(() => {  
    CD("/var");  
    Console.WriteLine(PWD); // prints "/var"
});  
Console.WriteLine(PWD); // prints "/tmp"
```

### Unified Process and HTTP Interface
Calq CMD provides unified interfaces that wrap `Process` and `HttpClient`, simplifying direct stream operations.
```csharp
ShellScript cmd = CMDV("tail -F /var/log/messages") | CMDV("grep -i 'error'");
using var worker = await cmd.StartAsync();
using var reader = new StreamReader(worker.StandardOutput);
try {
    var line = await reader.ReadLineAsync();
} catch (ShellWorkerException ex) {
    var errorCode = ex.ErrorCode; // returns exit code
    var errorMessage = await worker.ReadErrorMessageAsync(); // reads STDERR
}
```

### Automatic Resource Management
Workers support automatic disposal when output stream reading reaches end-of-stream or the output stream is disposed:
```csharp
var worker = await cmd.StartAsync();

// Disables automatic disposal
using var worker = await cmd.StartAsync(disposeOnCompletion: false);
```

### Automatic Process Cleanup
All spawned processes are automatically terminated when the application exits, preventing orphaned processes:
```csharp
// Processes are automatically tracked and cleaned up
CMD("long-running-command");
// If application exits unexpectedly, child processes are automatically killed
```

### Working Directory and Path Mapping
  
**LocalTerminal.WorkingDirectory** - the host's absolute path of the current working directory.  
**PWD** - `LocalTerminal.WorkingDirectory` mapped to the shell's internal path.  

Shell implementations automatically handle path translation between host and internal formats.
When using Bash on Windows via WSL, PWD is automatically mapped to the WSL path of the current working directory.  
On Linux, these are effectively the same.

```csharp
// Working directory examples
Console.WriteLine(LocalTerminal.WorkingDirectory); // e.g. "C:\Users"
Console.WriteLine(PWD);                            // e.g. "/mnt/c/Users"

// Programmatic path mapping
LocalTerminal.Shell.MapToInternalPath("C:\\temp");  // "/mnt/c/temp"
LocalTerminal.Shell.MapToHostPath("/mnt/c/temp");   // "C:\temp"
```

### Automatic Output Processing
Command output is automatically cleaned up by shell-specific postprocessors:
```csharp
// CMD automatically trims trailing newlines
string result = CMD("echo 'hello world'"); // "hello world" (no \n)
// Custom postprocessing available via IShellScriptPostprocessor
```

### Exception Handling
Failed commands throw `ShellScriptException` with contextual information:
```csharp
try {
    CMD("nonexistent-command");
} catch (ShellScriptException ex) {
    Console.WriteLine($"Exit Code: {ex.ErrorCode}");
    Console.WriteLine($"Details: {ex.Message}");
    // Message contains: command text, error details, and stderr output
}
```

When working with workers directly, catch `ShellWorkerException` for lower-level control:
```csharp
var script = new ShellScript(LocalTerminal.Shell, "failing-command");
try {
    using var worker = await script.StartAsync();
    using var reader = new StreamReader(worker.StandardOutput);
    string output = await reader.ReadToEndAsync();
} catch (ShellWorkerException ex) {
    Console.WriteLine($"Worker failed with code: {ex.ErrorCode}");
    string errorDetails = await worker.ReadErrorMessageAsync();
    Console.WriteLine($"Error output: {errorDetails}");
}
```

## Shell Configuration & Tools
Currently available built-in shells: `CommandLine`, `Bash`, `PythonTool`, `HttpTool`, and `ShellTool`.

### LocalTerminal
`LocalTerminal` settings are stored in an `AsyncLocal<T>` so that each logical context (thread or async task) keeps its own configuration.  
By default, `LocalTerminal.Shell.In` is set to `null` and `LocalTerminal.Out` is set to `Console.OpenStandardOutput()`.
```csharp
LocalTerminal.Shell = new CommandLine() {
    In =  Console.OpenStandardInput(); // default is `null`
}
LocalTerminal.Out = Console.OpenStandardOutput(); // default
```

### HttpTool
Can be used to operate on HTTP servers that comply with `HttpToolWorker` to simplify the development of distributed systems dealing with parallel batch workloads.   
Support for easy development of such servers through [Calq CLI](https://github.com/calq-framework/cli) is under consideration.

### ShellTool
ShellTool can be used to create custom shells that prepend commands at runtime.
```csharp
LocalTerminal.Shell = new ShellTool(new Bash(), "sudo");
RUN("apt update");
LocalTerminal.Shell = new ShellTool(new Bash(), "git");
RUN("commit -m 'automated message'"); 
```

## Integrations

### Python
Python scripts compatible with [Python Fire](https://github.com/google/python-fire) can be run identically via PythonTool shell.
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
```bash
python tool.py add 9 1 # prints "10"  
python tool.py upper --msg world  # prints "WORLD"  
```  
```csharp
var pts = new PythonToolServer("tool.py");
using var worker = await pts.StartAsync();
LocalTerminal.Shell = new PythonTool(pts);
RUN("add 9 1"); // prints "10"
RUN("upper --msg world"); // prints "WORLD"
```

#### PythonTool Input/Output Streams
Python HTTP server monkey-patches sys.stdin and consumes the entire stream before executing Python scripts via Python Fire. Python scripts requiring real-time input streaming must be executed directly via `python` using `Bash` or `CommandLine` shells.  
Conversely, output is unbuffered, streamed directly to C# over a raw HTTP/2 connection using asynchronous generators.
```python
# async-tool.py
import asyncio
import sys
import io

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
```csharp
var pts = new PythonToolServer("async-tool.py");
using var worker = await pts.StartAsync();
LocalTerminal.Shell = new PythonTool(pts) {
    In = new MemoryStream(Encoding.ASCII.GetBytes(" one\n two\n three\n"));
};
RUN("test") // prints each line every second
```

### ASP.NET Core Integration
Calq CMD integrates seamlessly with ASP.NET Core applications, enabling cloud-native web APIs that leverage shell-style scripting for data processing and streaming operations.

```csharp
public class UseMyPythonToolShellAttribute : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context) {
        var tool = context.HttpContext.RequestServices.GetRequiredService<MyPythonTool>();
        LocalTerminal.Shell = tool; // Sets Shell for this request scope
    }
}

[ApiController, UseMyPythonToolShell]
public class DataController : ControllerBase {
    [HttpGet("stream-data")]
    [Produces("text/plain")]
    public async Task<Stream> StreamData() => await CMDStreamAsync("process_large_dataset");

    [HttpPost("process-upload")]
    [Produces("application/json")]
    public async Task<Stream> ProcessUpload(IFormFile file) => await CMDStreamAsync("analyze_file", file.OpenReadStream());

    [HttpPost("process-stream")]
    [Produces("application/json")]
    public async Task<Stream> ProcessStream([FromBody] Stream data) => await CMDStreamAsync("transform_data", data);
}
```

## Quick Start  
```csharp
using static CalqFramework.Cmd.Terminal;  

class QuickStart {  
    static async Task Main() {  
        Console.WriteLine("PWD: " + PWD);  
        RUN("echo Hello Calq CMD!");  
        string version = CMD("curl --version");  
        Console.WriteLine(version);  
    }  
}  
```  

## License  
Calq CMD is dual-licensed under GNU AGPLv3 and a commercial license.  