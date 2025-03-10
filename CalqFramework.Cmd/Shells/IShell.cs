
namespace CalqFramework.Cmd.Shells {
    public interface IShell {

        Task ExecuteAsync(string workingDirectory, string script, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default);

        string GetInternalPath(string hostPath);
    }
}