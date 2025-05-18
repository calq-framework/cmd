using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using System.Text;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class PythonToolTest {

    [Fact]
    public async Task PythonTool_ReadInput_EchosCorrectly() {
        var input = "hello world\nhello world\n";
        var writer = new MemoryStream();
        LocalTerminal.Out = writer;

        var pythonServer = new PythonToolServer("./test_tool.py");
        await pythonServer.StartAsync();
        LocalTerminal.Shell = new PythonTool(pythonServer) {
            In = GetStream(input)
        };

        RUN("test");

        var output = ReadString(writer);
        Assert.Equal(input, output);
    }

    private Stream GetStream(string input) {
        byte[] byteArray = Encoding.ASCII.GetBytes(input);
        MemoryStream stream = new MemoryStream(byteArray);
        return stream;
    }

    private string ReadString(Stream writer) {
        writer.Position = 0;
        using StreamReader reader = new StreamReader(writer);
        return reader.ReadToEnd();
    }
}