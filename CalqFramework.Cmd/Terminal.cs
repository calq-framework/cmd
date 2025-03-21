﻿using CalqFramework.Cmd.SystemProcess;
using CalqFramework.Cmd.TerminalComponents.Contexts;

namespace CalqFramework.Cmd;
public static class Terminal {
    public static LocalTerminalConfigurationContext LocalTerminal { get; } = new LocalTerminalConfigurationContext();

    public static void CD(string path) {
        LocalTerminal.ProcessRunConfiguration.WorkingDirectory = Path.GetFullPath(Path.Combine(LocalTerminal.ProcessRunConfiguration.WorkingDirectory, path));
    }

    public static string CMD(string script, TimeSpan? timeout = null) {
        return CMD(script, TextReader.Null, timeout);
    }

    public static string CMD(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor }.Output;
    }

    public static Task<string> CMDAsync(string script, TimeSpan? timeout = null) {
        return CMDAsync(script, TextReader.Null, timeout);
    }

    public static Task<string> CMDAsync(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor }.GetOutputAsync();
    }

    public static ShellCommand CMDV(string script, TimeSpan? timeout = null) {
        return CMDV(script, TextReader.Null, timeout);
    }

    public static ShellCommand CMDV(string script, TextReader inputReader, TimeSpan? timeout = null) {
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        return new ShellCommand(LocalTerminal.Shell, script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }) { ShellCommandPostprocessor = LocalTerminal.ShellCommandPostprocessor };
    }
    public static void RUN(string script, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Run(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration), cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Run(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { Out = outputWriter }, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextReader inputReader, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Run(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }, cancellationTokenSource.Token);
    }

    public static void RUN(string script, TextReader inputReader, TextWriter outputWriter, TimeSpan? timeout = null) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        var cancellationTokenSource = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan);
        LocalTerminal.Shell.Run(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader, Out = outputWriter }, cancellationTokenSource.Token);
    }

    public static async Task RUNAsync(string script, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.RunAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration), cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.RunAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader }, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.RunAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { Out = outputWriter }, cancellationToken);
    }

    public static async Task RUNAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
        LocalTerminal.TerminalLogger.Log(script, LocalTerminal.ProcessRunConfiguration);
        await LocalTerminal.Shell.RunAsync(script, new ProcessRunConfiguration(LocalTerminal.ProcessRunConfiguration) { In = inputReader, Out = outputWriter }, cancellationToken);
    }
}
