using CalqFramework.Cli;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Default command executor implementation using CalqFramework.Cli for command-line style parsing.
/// </summary>
public class CliCommandExecutor : ICalqCommandExecutor
{
    private readonly CommandLineInterface _cli = new();

    /// <summary>
    /// Executes a command using CLI-style argument parsing via CalqFramework.Cli.
    /// </summary>
    /// <param name="target">The target object containing the methods to execute.</param>
    /// <param name="args">Command-line arguments (e.g., ["method-name", "--param", "value"]).</param>
    /// <returns>The result of the command execution.</returns>
    public object? Execute(object target, string[] args)
    {
        return _cli.Execute(target, args);
    }
}
