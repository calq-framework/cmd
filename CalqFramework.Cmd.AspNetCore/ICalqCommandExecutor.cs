namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Defines a contract for executing commands against a target object.
/// Implementations can use different command execution strategies (CLI parsing, JSON-RPC, direct method invocation, etc.).
/// </summary>
public interface ICalqCommandExecutor
{
    /// <summary>
    /// Executes a command against the specified target object.
    /// </summary>
    /// <param name="target">The target object containing the methods to execute.</param>
    /// <param name="args">Command arguments. Interpretation depends on the implementation.</param>
    /// <returns>The result of the command execution. May be a Task, Stream, string, or other type.</returns>
    object? Execute(object target, string[] args);
}
