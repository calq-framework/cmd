﻿using System.Text;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class TerminalTest {

    [Fact]
    public void Bash_ReadInput_EchosCorrectly() {
        var writer = new MemoryStream();
        string input = "hello world\n";

        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash() {
            In = GetStream(input)
        };

        RUN("sleep 1; read -r input; echo $input");
        string output = ReadString(writer);

        Assert.Equal(input, output);
    }

    [Fact]
    public void CMD_WithLongOutput_ReturnsCorrectly() {
        string expectedOutput = "";
        for (int i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new MemoryStream();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        string output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        string writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void CMD_WithValidCommand_ReturnsNonEmpty() {
        LocalTerminal.Shell = new CommandLine();

        string output = CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void CommandOutput_AfterOutput_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string input = "hello world";

        ShellScript command = CMDV($"echo {input}");
        string output1 = command.Evaluate();
        string output2 = command.Evaluate();

        Assert.Equal(input, output1);
        Assert.Equal(input, output2);
    }

    [Fact]
    public void CommandPiping_AfterMultiplePipes_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string echoText = "hello world";

        string output = CMDV($"echo {echoText}") | CMDV("cat") | CMDV("cat") | CMDV("cat");
        Assert.Equal(echoText, output);
    }

    [Fact]
    public void CommandPiping_WithEchoAndCut_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string echoText = "hello, world";
        ShellScript echoCommand = CMDV($"echo {echoText}");

        if (string.Compare(echoText, echoCommand) != 0) {
            Assert.True(false);
        }
        if (echoText != echoCommand) {
            Assert.True(false);
        }

        string output = echoCommand | CMDV("cut -d',' -f1");
        Assert.Equal("hello", output);
    }

    [Fact]
    public void CommandPiping_WithError_ThrowsException() {
        LocalTerminal.Shell = new Bash();
        string echoText = "hello world";

        Assert.Throws<ShellScriptException>(() => {
            string output = CMDV($"echo {echoText}") | CMDV("cat; exit 1;") | CMDV("cat");
        });
    }

    [Fact]
    public async Task CommandStart_AfterGarbageCollection_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string input = "hello world";

        ShellScript command = CMDV($"sleep 2; echo {input}");
        using Cmd.Shell.IShellWorker proc = await command.StartAsync();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        using var reader = new StreamReader(proc.StandardOutput);
        string? output = reader.ReadLine();

        Assert.Equal(input, output);
    }

    [Fact]
    public async Task HttpTool_EchoContentBody_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        ShellScript pythonScript = CMDV(@"python <<EOF
from http.server import BaseHTTPRequestHandler, HTTPServer

class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        try:
            content_length = int(self.headers.get('Content-Length', 0))
            post_body = self.rfile.read(content_length).decode('utf-8')

            print(""Received POST body:"")
            print(post_body)

            self.send_response(200)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(post_body.encode())
        except Exception as e:
            print(""Error reading POST body:"", e)
            self.send_response(500)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(f""Error: {e}"".encode())

HTTPServer(('', 8001), Handler).serve_forever()
EOF
    ");
        using Cmd.Shell.IShellWorker serverWorker = await pythonScript.StartAsync();

        var httpClient = new HttpClient {
            BaseAddress = new Uri("http://127.0.0.1:8001")
        };
        string input = "hello world";
        LocalTerminal.Shell = new HttpTool(httpClient) {
            In = GetStream(input)
        };

        string echo = CMD("", LocalTerminal.Shell.In);

        Assert.Equal(input, echo);
    }

    [Fact]
    public async Task HttpTool_EvalPython_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        ShellScript pythonScript = CMDV(@"python <<EOF
from http.server import BaseHTTPRequestHandler, HTTPServer

class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        try:
            script_param = self.headers.get('Script')
            if script_param:
                result = str(eval(script_param))
                self.send_response(200)
                self.send_header('Content-Type', 'text/plain')
                self.end_headers()
                self.wfile.write(result.encode())
            else:
                self.send_response(400)
                self.send_header('Content-Type', 'text/plain')
                self.end_headers()
                self.wfile.write(b'Missing Script header')
        except Exception as e:
            print(""Error during eval:"", e)
            self.send_response(500)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(f""Error: {e}"".encode())

HTTPServer(('', 8000), Handler).serve_forever()
EOF
");
        using Cmd.Shell.IShellWorker serverWorker = await pythonScript.StartAsync();

        var httpClient = new HttpClient {
            BaseAddress = new Uri("http://127.0.0.1:8000")
        };
        LocalTerminal.Shell = new HttpTool(httpClient);

        string echo = CMD("sum([8, 16, 32])");

        Assert.Equal((8 + 16 + 32).ToString(), echo);
    }

    [Fact]
    public async Task HttpTool_MidStreamError_Throws() {
        LocalTerminal.Shell = new Bash();
        CMD(@"openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes -subj ""/CN=localhost""");
        ShellScript pythonScript = CMDV(@"python <<EOF
import ssl
import socket
import h2.connection
import h2.events
import h2.config
import time

def run_h2_tls_server():
    context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
    context.load_cert_chain(certfile='cert.pem', keyfile='key.pem')
    context.set_alpn_protocols(['h2'])

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(('0.0.0.0', 8443))
    sock.listen(5)
    print(""Listening on https://localhost:8443 (HTTP/2 TLS)"")

    while True:
        conn_tcp, addr = sock.accept()
        conn = context.wrap_socket(conn_tcp, server_side=True)

        negotiated = conn.selected_alpn_protocol()
        if negotiated != 'h2':
            print(f""Client did not negotiate h2 (got {negotiated})"")
            conn.close()
            continue

        config = h2.config.H2Configuration(client_side=False, header_encoding='utf-8')
        h2_conn = h2.connection.H2Connection(config=config)
        h2_conn.initiate_connection()
        conn.sendall(h2_conn.data_to_send())

        stream_data = {}

        while True:
            data = conn.recv(65535)
            if not data:
                break

            events = h2_conn.receive_data(data)
            for event in events:
                if isinstance(event, h2.events.RequestReceived):
                    stream_id = event.stream_id
                    stream_data[stream_id] = b""""
                    print(f""Headers received on stream {stream_id}"")

                elif isinstance(event, h2.events.DataReceived):
                    h2_conn.acknowledge_received_data(event.flow_controlled_length, event.stream_id)
                    stream_data[event.stream_id] += event.data
                    conn.sendall(h2_conn.data_to_send())

                elif isinstance(event, h2.events.StreamEnded):
                    stream_id = event.stream_id
                    body = stream_data[stream_id]
                    print(f""Full request body on stream {stream_id}: {body!r}"")

                    h2_conn.send_headers(stream_id, [
                        (':status', '200'),
                        ('content-type', 'text/plain'),
                    ])
                    conn.sendall(h2_conn.data_to_send())

                    chunks = [body[i:i+6] for i in range(0, len(body), 6)]

                    for i, chunk in enumerate(chunks):
                        print(f""Sending chunk: {chunk}"")
                        h2_conn.send_data(stream_id, chunk, end_stream=False)
                        conn.sendall(h2_conn.data_to_send())

                        if i == 2:
                            # Reset after second chunk
                            time.sleep(1) # TODO HttpClient throws on stream read if it already received RST_STREAM even before the output leading to the RESET has been read so consider fixing this wait time
                            h2_conn.reset_stream(stream_id, error_code=128)
                            conn.sendall(h2_conn.data_to_send())
                            print(f""RST_STREAM sent on stream {stream_id}"")
                            break

                    # End stream properly
                    h2_conn.send_data(stream_id, b"""", end_stream=True)
                    conn.sendall(h2_conn.data_to_send())
                    print(""Stream ended cleanly."")

        conn.close()

run_h2_tls_server()
EOF");
        using Cmd.Shell.IShellWorker serverWorker = await pythonScript.StartAsync();

        var handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://localhost:8443")
        };

        string inputBeforeReset = "hello\nworld\n";
        string input = inputBeforeReset + "should not be streamed back\n";
        LocalTerminal.Shell = new HttpTool(httpClient) {
            In = GetStream(input)
        };

        ShellScript echo = CMDV("");
        using Cmd.Shell.IShellWorker requestWorker = await echo.StartAsync();
        var reader = new StreamReader(requestWorker.StandardOutput);
        string output = "";
        Assert.Throws<ShellWorkerException>(() => {
            output += reader.ReadLine() + '\n'; // ok
            output += reader.ReadLine() + '\n'; // ok
            output += reader.ReadLine() + '\n'; // throws
        });

        Assert.Equal(inputBeforeReset, output);
    }

    [Fact]
    public void RUN_WithLongOutput_WritesCorrectly() {
        string expectedOutput = "";
        for (int i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new MemoryStream();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        string writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, writerOutput);
    }

    [Fact]
    public async Task Shell_WhenChangedInTask_RevertsToOriginal() {
        LocalTerminal.Shell = new CommandLine();
        Cmd.Shell.IShell cmd = LocalTerminal.Shell;
        await Task.Run(() => {
            LocalTerminal.Shell = new Bash();
            Assert.True(LocalTerminal.Shell is Bash);
        });
        Assert.Equal(cmd, LocalTerminal.Shell);
    }

    [Fact]
    public async Task WorkingDirectory_WhenChangedInTask_RevertsToOriginal() {
        string currentDirectory = PWD;
        await Task.Run(() => {
            CD("changed");
            Assert.NotEqual(currentDirectory, PWD);
        });
        Assert.Equal(currentDirectory, PWD);
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
