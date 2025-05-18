namespace CalqFramework.Cmd;

public class ShellScriptException(long? errorCode, string? message, Exception? innerException) : Exception(message, innerException) {

    public ShellScriptException(long? errorCode, string? message) : this(errorCode, message, null) {
    }

    public long? ErrorCode { get; } = errorCode;
}
