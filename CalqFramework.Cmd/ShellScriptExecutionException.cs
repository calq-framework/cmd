namespace CalqFramework.Cmd;
public class ShellScriptExecutionException : Exception {
    public long ErrorCode { get; }

    public ShellScriptExecutionException(long errorCode, string? message) : this(errorCode, message, null) {
    }

    public ShellScriptExecutionException(long errorCode, string? message, Exception? innerException) : base(message, innerException) {
        ErrorCode = errorCode;
    }
}
