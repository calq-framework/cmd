namespace CalqFramework.Cmd;
public class ShellWorkerException : Exception {
    public long? ErrorCode { get; }

    public ShellWorkerException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public ShellWorkerException(long? errorCode, string? message, Exception? innerException) : base(message, innerException) {
        ErrorCode = errorCode;
    }
}
