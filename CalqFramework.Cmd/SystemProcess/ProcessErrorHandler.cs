namespace CalqFramework.Cmd.SystemProcess {
    public class ProcessErrorHandler : IProcessErrorHandler {
        public void AssertSuccess(int code, string message) {
            // stderr might contain diagnostics/info instead of error message so don't throw just because not empty
            if (code != 0) {
                throw new ProcessExecutionException(code, message);
            }
        }
    }
}
