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
Pipelines are internally run asynchronously, and each pipeline step is run in parallel.  
The following returns "Hello World" after 1 second.
```csharp
string output = CMDV("echo Hello World") | CMDV("sleep 1; cat") | CMDV("sleep 1; cat") | CMDV("sleep 1; cat");
```
`LocalTerminal` settings are stored in `AsyncLocal<T>` so threads and tasks can be used like subshells.
```csharp
CD("/tmp");  
Task.Run(() => {  
    CD("/var");  
    Console.WriteLine(PWD); // prints "/var"
});  
Console.WriteLine(PWD); // prints "/tmp"
```
Calq CMD provides unified interfaces that wrap `Process` and `HttpClient`, simplifying direct stream operations.
```csharp
ShellScript cmd = CMDV("tail -F /var/log/messages") | CMDV("grep -i 'error'");
using var worker = await cmd.StartAsync();
using var reader = new StreamReader(worker.StandardOutput);
try {
    var line = reader.ReadLine()
} catch (ShellWorkerException ex) {
    var errorCode = ex.ErrorCode; // returns exit code
    var errorMessage = await worker.ReadErrorMessageAsync(); // reads STDERR
}
```

## Usage
Currently available built-in shells: `CommandLine`, `Bash`, `PythonTool`, `HttpTool`, and `ShellTool`.

### CMD/RUN
CMD by default doesn't read from any stream and returns a string with the last newline trimmed by `LocalTerminal.Shell.ShellScriptPostprocessor`.
```csharp
CMD("echo Hello World");
```
RUN by default reads from `LocalTerminal.Shell.In` and writes into `LocalTerminal.Out`.
```csharp
RUN("read -r input; echo $input");
```
See [Terminal.cs](https://github.com/calq-framework/cmd/blob/main/CalqFramework.Cmd/Terminal.cs) for all available overloads.

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

### Working Directory and WSL
**LocalTerminal.WorkingDirectory** - the host’s absolute path of the current working directory.  
**PWD** - `LocalTerminal.WorkingDirectory` mapped to the shell’s internal path.  
When using Bash on Windows via WSL, PWD is automatically mapped to the WSL path of the current working directory.  
On Linux, these are effectively the same.
```csharp
Console.WriteLine(LocalTerminal.WorkingDirectory); // e.g. "C:\Users"
Console.WriteLine(PWD);                            // e.g. "/mnt/c/Users"
```

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

### Quick Start  
```csharp
using static CalqFramework.Cmd.Terminal;  

class QuickStart {  
    static async Task Main() {  
        Console.WriteLine("PWD: " + PWD);  
        RUN("echo Hello Calq CMD!");  
        string date = CMD("date");  
        Console.WriteLine(date);  
    }  
}  
```  

## License  
Calq CMD is dual-licensed under GNU AGPLv3 and a commercial license.  
