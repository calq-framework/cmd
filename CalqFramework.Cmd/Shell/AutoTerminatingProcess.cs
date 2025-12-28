using System.Collections.Concurrent;
using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Process wrapper that automatically terminates child processes when the application exits.
    /// Prevents orphaned processes by tracking all instances and cleaning them up on shutdown.
    /// </summary>
    public class AutoTerminatingProcess : Process {
        private static readonly ConcurrentDictionary<Process, byte> s_allProcesses = new();
        private bool _disposed;

        static AutoTerminatingProcess() {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        /// <summary>
        /// Initializes a new AutoTerminatingProcess and registers it for automatic cleanup.
        /// </summary>
        public AutoTerminatingProcess() {
            _ = s_allProcesses.TryAdd(this, 0);
        }

        /// <summary>
        /// Disposes the process and ensures it's terminated if still running.
        /// Removes the process from the tracking collection.
        /// </summary>
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

        /// <summary>
        /// Event handler that terminates all tracked processes when the application exits.
        /// Ensures no orphaned processes remain after application shutdown.
        /// </summary>
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
