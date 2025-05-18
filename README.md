[![NuGet Version](https://img.shields.io/nuget/v/CalqFramework.Cmd?color=508cf0)](https://www.nuget.org/packages/CalqFramework.Cmd)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CalqFramework.Cmd?color=508cf0)](https://www.nuget.org/packages/CalqFramework.Cmd)

# Calq CMD  
Calq CMD is an inter-process development framework that simplifies building cross-platform, multi-language tools by enabling shell-style C# scripting.  
It integrates with Bash on Windows (WSL, Cygwin/MinGW/MSYS2) and provides Python interoperability via an asynchronous HTTP/2 server for low-latency, high-throughput scripting.

## Shell-style scripting in C#  
Write concise shell pipelines with C#:  
```csharp
string output = CMD("git status");
// CMDV returns ShellScript which implicitly converts to string
string files  = CMDV("ls -1") | CMDV("grep \".cs\"");
```  
Subshell isolation is as simple as `Task.Run()` - settings in the child task don’t leak out:  
```csharp
var original = LocalTerminal.Shell;  
Task.Run(() => {  
    LocalTerminal.Shell = new Bash();  
    RUN("echo inside");  
});  
// still uses the original shell here:  
RUN("echo outside");  
```  

## Usage  

### Basic shells  
```csharp
LocalTerminal.Shell = new CommandLine();  
RUN("echo Hello from CMD");  

LocalTerminal.Shell = new Bash();  
string whoami = CMD("whoami");  
Console.WriteLine(whoami);  
```  

### Async  
```csharp
string uname = await CMDAsync("uname -a");  
Console.WriteLine(uname);  
```  

### Pipelines with CMDV  
```csharp
var script = CMDV("git ls-files") | CMDV("grep \".csproj\"");  
Console.WriteLine(script.Evaluate());  
```  

### Prepend commands with ShellTool  
```csharp
LocalTerminal.Shell = new ShellTool(new CommandLine(), "sudo");
// runs "sudo apt update" on Linux and fails on Windows as Windows doesn't have sudo (nor apt)
RUN("apt update");
LocalTerminal.Shell = new ShellTool(new CommandLine(), "git");
RUN("commit -m 'automated message'"); 
```  

### LocalTerminal  
`LocalTerminal` settings (shell, output, logger) are stored in an `AsyncLocal<T>` so that each logical context (thread or async task) keeps its own configuration.  
```csharp
CD("/tmp");  
Task.Run(() => {  
    CD("/var");  
    Console.WriteLine(PWD);           // /var  
});  
Console.WriteLine(PWD);               // /tmp  
```  

#### Working Directory and WSL  
- **`LocalTerminal.WorkingDirectory`**: the host’s absolute path of the current working directory.  
- **`PWD`**: `LocalTerminal.WorkingDirectory` mapped to the shell’s internal path.
When using Bash on Windows via WSL, PWD is automatically mapped to the WSL path of the current working directory.  
On Linux, these are effectively the same.
```csharp
Console.WriteLine(PWD);                             // e.g. /mnt/c/Users/You  
Console.WriteLine(LocalTerminal.WorkingDirectory);  // C:\Users\You  
CD("projects");  
Console.WriteLine(PWD);                             // /mnt/c/Users/You/projects  
Console.WriteLine(LocalTerminal.WorkingDirectory);  // C:\Users\You\projects  
```  

## Python  
`PythonTool` lets you call methods in a Python Fire–based script over HTTP/2, with raw streaming for minimal overhead.  
```python
# tool.py
import fire  

def foo(x: int):  
    return x * 2  

def bar(msg: str = "hello"):  
    return msg.upper()  

if __name__ == "__main__":  
    fire.Fire()  
```  
From the shell:  
```bash
python tool.py foo 5    # prints "10"  
python tool.py bar --msg world  # prints "WORLD"  
```  
With C# and `PythonTool`:  
```csharp
var pythonServer = new PythonServer("tool.py")
LocalTerminal.Shell = new PythonTool(pythonServer);  
string doubled = CMD("foo 5");  
Console.WriteLine(doubled);    // "10"  
```  
#### Input/Output
Input redirection is done automatically by monkey-patching sys.stdin before the script module is loaded. This means the entire input must be consumed before the Python script begins execution, introducing only a negligible delay that doesn’t affect overall server throughput. However, this approach currently does not support real-time input streaming.     
Conversely, output is unbuffered, streamed directly to C# over a raw HTTP/2 connection using asynchronous generators.
```python
# async-tool.py
﻿import fire
import asyncio
import sys
import io

async def delayed_echo_gen():
    for line in sys.stdin:
        await asyncio.sleep(1)
        yield line

def test():
    return delayed_echo_gen()

if __name__ == '__main__':
    value = fire.Fire()
    async def main():
        async for line in value:
            print(line)
    asyncio.run(main())
```
Calling it like this will output each line every second in console.
```csharp
var pythonServer = new PythonToolServer("async-tool.py");
await pythonServer.StartAsync();
LocalTerminal.Shell = new PythonTool(pythonServer) {
    In = new MemoryStream(Encoding.ASCII.GetBytes(" one\n two\n three\n"));
};
RUN("test")
```

### Quick Start  
```csharp
using static CalqFramework.Cmd.Terminal;  

class QuickStart {  
    static async Task Main() {  
        Console.WriteLine("CWD: " + PWD);  
        RUN("echo Hello Calq CMD!");  
        string date = CMD("date");  
        Console.WriteLine(date);  
    }  
}  
```  

## License  
Calq CMD is dual-licensed under GNU AGPLv3 and a commercial license.  
