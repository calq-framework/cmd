using System.Collections.Concurrent;
using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public class AutoTerminateProcess : Process {
        private static ConcurrentDictionary<Process, byte> _allProcesses = new();
        private bool _disposed;

        static AutoTerminateProcess() {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        public AutoTerminateProcess() {
            _ = _allProcesses.TryAdd(this, 0);
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (!HasExited) { // TODO add HasStarted condition
                    Kill(true); // killing already killed process shouldn't throw
                }
                _ = _allProcesses.TryRemove(this, out _);

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private static void OnProcessExit(object? sender, EventArgs e) {
            foreach (var process in _allProcesses.Keys) {
                try {
                    if (!process.HasExited) {
                        process.Kill(true);
                    }
                } catch { }
            }
        }
    }
}
