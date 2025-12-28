namespace CalqFramework.Cmd;

/// <summary>
/// Exception thrown by shell workers when command execution fails.
/// Provides access to exit codes and error messages for debugging.
/// </summary>

public class ShellWorkerException(long? errorCode, string? message, Exception? innerException) : Exception(message, innerException) {

    public ShellWorkerException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    /// <summary>
    /// Exit code returned by the failed worker operation, or null if not available.
    /// </summary>
    public long? ErrorCode { get; } = errorCode;
}
