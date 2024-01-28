namespace CalqFramework.Shell;

// TODO extend TextWriter (requires statefull shells)
// stateful bash could be done maybe with subshells ( script ) > file ?
public interface IShell
{
    string CMD(string script);
}
