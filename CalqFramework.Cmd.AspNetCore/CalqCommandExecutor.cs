using System.IO;
using CalqFramework.Cli;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Default command executor implementation using CalqFramework.Cli for command-line style parsing.
/// </summary>
public class CalqCommandExecutor : ICalqCommandExecutor {
    /// <summary>
    ///     Executes a command using CLI-style argument parsing via CalqFramework.Cli.
    /// </summary>
    /// <param name="target">The target object containing the methods to execute.</param>
    /// <param name="args">Command-line arguments (e.g., ["method-name", "--param", "value"]).</param>
    /// <returns>The result of the command execution.</returns>
    public object? Execute(object target, string[] args) => Execute(target, args, null);

    /// <summary>
    ///     Executes a command using CLI-style argument parsing via CalqFramework.Cli with output capture.
    /// </summary>
    /// <param name="target">The target object containing the methods to execute.</param>
    /// <param name="args">Command-line arguments (e.g., ["method-name", "--param", "value"]).</param>
    /// <param name="output">Optional TextWriter to capture output. If null, uses Console.Out.</param>
    /// <returns>The result of the command execution.</returns>
    public object? Execute(object target, string[] args, TextWriter? output) {
        CommandLineInterface cli = new() {
            Out = output ?? Console.Out
        };
        return cli.Execute(target, args);
    }
}
