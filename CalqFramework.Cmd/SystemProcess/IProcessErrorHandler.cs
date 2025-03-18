using System.Diagnostics;

namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessErrorHandler {
        void AssertSuccess(ProcessExecutionInfo processExecutionInfo, IProcessRunConfiguration processRunConfiguration, Process process, string error);
    }
}