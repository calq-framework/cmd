namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Defines a contract for executing commands against a target object.
///     Implementations can use different command execution strategies (CLI parsing, JSON-RPC, direct method invocation,
///     etc.).
/// </summary>
public interface ICalqCommandExecutor {
    /// <summary>
    ///     Executes a command with output capture.
    /// </summary>
    /// <param name="args">Command arguments. Interpretation depends on the implementation.</param>
    /// <param name="output">Optional TextWriter to capture output from void methods.</param>
    /// <returns>The result of the command execution. May be a Task, Stream, string, or other type.</returns>
    object? Execute(string[] args, System.IO.TextWriter output);
}
