namespace CalqFramework.Cmd;

public class ShellWorkerException(long? errorCode, string? message, Exception? innerException) : Exception(message, innerException) {

    public ShellWorkerException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public long? ErrorCode { get; } = errorCode;
}
