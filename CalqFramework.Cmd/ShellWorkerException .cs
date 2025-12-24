namespace CalqFramework.Cmd;

/// <summary>
/// Exception thrown by shell workers when command execution fails.
/// Provides access to exit codes and error messages for debugging.
/// </summary>

public class ShellWorkerException(long? errorCode, string? message, Exception? innerException) : Exception(message, innerException) {

    public ShellWorkerException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public long? ErrorCode { get; } = errorCode;
}
