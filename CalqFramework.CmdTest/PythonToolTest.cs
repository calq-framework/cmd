using System.Text;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class PythonToolTest {

    [Fact]
    public async Task PythonTool_ReadInput_EchosCorrectly() {
        string input = "hello world\nhello world\n";
        var writer = new MemoryStream();
        LocalTerminal.Out = writer;

        var pythonServer = new PythonToolServer("./test_tool.py");
        await pythonServer.StartAsync();
        LocalTerminal.Shell = new PythonTool(pythonServer) {
            In = GetStream(input)
        };

        RUN("test");

        string output = ReadString(writer);
        Assert.Equal(input, output);
    }

    private static Stream GetStream(string input) {
        byte[] byteArray = Encoding.ASCII.GetBytes(input);
        MemoryStream stream = new(byteArray);
        return stream;
    }

    private static string ReadString(Stream writer) {
        writer.Position = 0;
        using StreamReader reader = new(writer);
        return reader.ReadToEnd();
    }
}
