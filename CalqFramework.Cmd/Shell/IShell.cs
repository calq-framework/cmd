namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        IShellWorkerErrorHandler ErrorHandler { get; }
        Stream? In { get; }
        IShellScriptPostprocessor Postprocessor { get; }

        IShellWorker CreateShellWorker(ShellScript shellScript);
        IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream);
        string MapToInternalPath(string hostPath);
    }
}