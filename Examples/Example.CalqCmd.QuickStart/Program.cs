using static CalqFramework.Cmd.Terminal;

// Execute a shell command and get the output as a string
string echo = CMD("echo Hello World");
RUN($"echo {echo}"); // prints "Hello World"

// Pipeline chaining — each step runs in parallel
string output = CMDV("echo Hello World") | CMDV("cat");
Console.WriteLine(output); // "Hello World"
