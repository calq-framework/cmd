namespace CalqFramework.Cmd;

/// <summary>
/// Exception thrown when shell script execution fails.
/// Contains error codes and messages from failed command execution.
/// </summary>

public class ShellScriptException(long? errorCode, string? message, Exception? innerException) : Exception(message, innerException) {

    public ShellScriptException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    /// <summary>
    /// Exit code returned by the failed shell command, or null if not available.
    /// </summary>
    public long? ErrorCode { get; } = errorCode;
}
