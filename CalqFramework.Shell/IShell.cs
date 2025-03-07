
namespace CalqFramework.Shell {
    public interface IShell {
        string CurrentDirectory { get; }
        TextReader In { get; init; }
        TextWriter Out { get; init; }

        void CD(string path);
        string CMD(string script, TimeSpan? cancellationDelay = null);
        string CMD(string script, TextReader inputReader, TimeSpan? cancellationDelay = null);
        string GetLocalPath(string path);
        void RUN(string script, TimeSpan? cancellationDelay = null);
        void RUN(string script, TextReader inputReader, TimeSpan? cancellationDelay = null);
    }
}