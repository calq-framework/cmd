namespace CalqFramework.Shell;

// TODO extend TextWriter (requires statefull shells)
// stateful bash could be done maybe with subshells ( script ) > file ?
public interface IShell {
    string CurrentDirectory { get; }
    string CMD(string script, TextReader? inputReader = null);
    void RUN(string script, TextReader? inputReader = null);
    void CD(string path);
    string GetLocalPath(string path);
}
