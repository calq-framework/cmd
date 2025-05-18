namespace CalqFramework.Cmd;

public class ShellWorkerException : Exception {

    public ShellWorkerException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public ShellWorkerException(long? errorCode, string? message, Exception? innerException) : base(message, innerException) {
        ErrorCode = errorCode;
    }

    public long? ErrorCode { get; }
}