namespace CalqFramework.Cmd.Shell {
    public interface IShellWorkerErrorHandler {
        void AssertSuccess(string script, int exitCode, string errorMessasge, string? output);
    }
}