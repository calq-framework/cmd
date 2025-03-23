namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessErrorHandler {
        void AssertSuccess(int code, string message);
    }
}