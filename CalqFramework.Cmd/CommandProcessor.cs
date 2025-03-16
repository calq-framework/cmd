namespace CalqFramework.Cmd {
    public class CommandProcessor : ICommandProcessor {
        public string ProcessValue(string value) {
            return value.TrimEnd();
        }
    }
}
