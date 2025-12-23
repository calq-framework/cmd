using System.Collections.Concurrent;
using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {

    public class AutoTerminatingProcess : Process {
        private static readonly ConcurrentDictionary<Process, byte> s_allProcesses = new();
        private bool _disposed;

        static AutoTerminatingProcess() {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        public AutoTerminatingProcess() {
            _ = s_allProcesses.TryAdd(this, 0);
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (!HasExited) { // TODO add HasStarted condition
                    Kill(true); // killing already killed process shouldn't throw
                }
                _ = s_allProcesses.TryRemove(this, out _);

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private static void OnProcessExit(object? sender, EventArgs e) {
            foreach (Process process in s_allProcesses.Keys) {
                try {
                    if (!process.HasExited) {
                        process.Kill(true);
                    }
                } catch { }
            }
        }
    }
}
