namespace CalqFramework.Cmd;

/// <summary>
/// Exception thrown when shell script execution fails.
/// Contains error codes and messages from failed command execution.
/// </summary>

public class ShellScriptException(long? errorCode, string? message, Exception? innerException) : Exception(message, innerException) {

    public ShellScriptException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public long? ErrorCode { get; } = errorCode;
}
