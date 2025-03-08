
namespace CalqFramework.Cmd.Shells {
    public interface IShell {
        string CurrentDirectory { get; set; }
        TextReader In { get; init; }
        TextWriter Out { get; init; }

        Task ExecuteAsync(string script, CancellationToken cancellationToken = default);
        Task ExecuteAsync(string script, TextReader inputReader, CancellationToken cancellationToken = default);
        Task ExecuteAsync(string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default);
        Task ExecuteAsync(string script, TextWriter outputWriter, CancellationToken cancellationToken = default);

        string GetLocalPath(string path);
    }
}