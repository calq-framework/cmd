using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Configuration options for CalqCmdController
/// </summary>
public class CalqCmdControllerOptions {
    /// <summary>
    ///     Route prefix for the CalqCmdController. If null or empty, uses default "CalqCmd"
    /// </summary>
    public string? RoutePrefix { get; set; }

    /// <summary>
    ///     HTTP client timeout for LocalHttpTool connections. Default is 30 seconds.
    /// </summary>
    public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Named HTTP client configuration for CalqFramework.Cmd.LocalHttpTool
    /// </summary>
    public string HttpClientName { get; set; } = "CalqFramework.Cmd.LocalHttpTool";

    /// <summary>
    ///     Custom command executor. If null, uses CalqCommandExecutor (CalqFramework.Cli) by default.
    /// </summary>
    public ICalqCommandExecutor? CommandExecutor { get; set; }

    /// <summary>
    ///     Default shell for LocalTerminal. Defaults to CommandLine shell.
    /// </summary>
    public IShell DefaultShell { get; set; } = new CommandLine();

    /// <summary>
    ///     Default terminal logger for LocalTerminal. Defaults to NullTerminalLogger.
    /// </summary>
    public ITerminalLogger DefaultTerminalLogger { get; set; } = new NullTerminalLogger();
}
