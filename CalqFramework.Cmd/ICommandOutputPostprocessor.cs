
namespace CalqFramework.Cmd {
    public interface ICommandOutputPostprocessor {
        string ProcessValue(string value);
    }
}