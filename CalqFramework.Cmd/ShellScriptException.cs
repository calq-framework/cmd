namespace CalqFramework.Cmd;
public class ShellScriptException : Exception {
    public long? ErrorCode { get; }

    public ShellScriptException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public ShellScriptException(long? errorCode, string? message, Exception? innerException) : base(message, innerException) {
        ErrorCode = errorCode;
    }
}
