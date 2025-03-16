using System.Diagnostics;

namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessErrorHandler {
        void AssertSuccess(ProcessRunInfo processRunInfo, IProcessRunConfiguration processRunConfiguration, Process process, string error);
    }
}