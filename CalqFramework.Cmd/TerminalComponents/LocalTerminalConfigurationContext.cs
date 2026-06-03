using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shell.Durable;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.TerminalComponents;

/// <summary>
///     Manages terminal configuration using AsyncLocal storage for thread/task isolation.
///     Each logical context maintains its own Shell, output stream, and logger settings.
/// </summary>
public class LocalTerminalConfigurationContext {
    private readonly AsyncLocal<Stream> _localOut = new();
    private readonly AsyncLocal<IShell> _localShell = new();
    private readonly AsyncLocal<ITerminalLogger> _localTerminalLogger = new();

    /// <summary>
    ///     Output stream for terminal operations. Defaults to Console.OpenStandardOutput().
    /// </summary>
    public Stream Out {
        get {
            _localOut.Value ??= Console.OpenStandardOutput();
            return _localOut.Value!;
        }
        set => _localOut.Value = value;
    }

    /// <summary>
    ///     Shell implementation for command execution. Auto-wraps in DurableShell unless already a decorator.
    ///     When DurabilityContext carries a store (server context), uses that store.
    ///     Otherwise defaults to filesystem DurableShell (CLI context).
    /// </summary>
    public IShell Shell {
        get {
            if (_localShell.Value == null) {
                if (DurabilityContext.Current.Value is { } ctx) {
                    _localShell.Value = new DurableShell(new CommandLine(), ctx.Store, ctx.WorkflowId, ctx.SequencePath);
                } else {
                    _localShell.Value = new DurableShell(new CommandLine());
                }
            }

            return _localShell.Value!;
        }
        set {
            if (value is ShellDecoratorBase) {
                _localShell.Value = value;
            } else if (DurabilityContext.Current.Value is { } ctx) {
                _localShell.Value = new DurableShell(value, ctx.Store, ctx.WorkflowId, ctx.SequencePath);
            } else {
                _localShell.Value = new DurableShell(value);
            }
        }
    }

    /// <summary>
    ///     Logger for terminal operations. Defaults to TerminalLogger that formats commands as "RUN: command".
    ///     Used to log commands executed via RUN operations for debugging and monitoring.
    /// </summary>
    public ITerminalLogger TerminalLogger {
        get {
            _localTerminalLogger.Value ??= new TerminalLogger();
            return _localTerminalLogger.Value!;
        }
        set => _localTerminalLogger.Value = value;
    }

    /// <summary>
    ///     Host's absolute path of the current working directory.
    ///     Mapped to shell's internal path format via PWD property.
    /// </summary>
    public static string WorkingDirectory {
        get => ShellScript.LocalWorkingDirectory.Value!;
        set => ShellScript.LocalWorkingDirectory.Value = value;
    }

    /// <summary>
    ///     Sets the shell without auto-wrapping in DurableShell.
    ///     Used by framework internals and explicit durability opt-out.
    /// </summary>
    public void SetRawShell(IShell shell) => _localShell.Value = shell;
}
