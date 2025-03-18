namespace CalqFramework.Cmd {
    public class CommandOutputPostprocessor : ICommandOutputPostprocessor {
        public string ProcessValue(string value) {
            return value.TrimEnd();
        }
    }
}
