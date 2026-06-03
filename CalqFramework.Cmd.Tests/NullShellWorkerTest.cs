using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.Tests;

public class NullShellWorkerTest {
    [Fact]
    public void NullShellWorker_ServesPreExistingStream() {
        byte[] data = "cached output"u8.ToArray();
        var stream = new MemoryStream(data);
        ShellScript shellScript = new(new CommandLine(), "test");
        NullShellWorker worker = new(shellScript, stream, false);

        byte[] buffer = new byte[data.Length];
        int read = worker.StandardOutput.Read(buffer);

        Assert.Equal(data.Length, read);
        Assert.Equal(data, buffer);
        worker.Dispose();
    }

    [Fact]
    public async Task NullShellWorker_StartAsync_IsNoOp() {
        var stream = new MemoryStream("data"u8.ToArray());
        ShellScript shellScript = new(new CommandLine(), "test");
        NullShellWorker worker = new(shellScript, stream, false);

        await worker.StartAsync();

        // Should not throw, should be instant
        Assert.NotNull(worker.StandardOutput);
        worker.Dispose();
    }

    [Fact]
    public async Task NullShellWorker_EnsurePipeIsCompleted_IsNoOp() {
        var stream = new MemoryStream("data"u8.ToArray());
        ShellScript shellScript = new(new CommandLine(), "test");
        NullShellWorker worker = new(shellScript, stream, false);

        await worker.EnsurePipeIsCompletedAsync();

        // Should not throw
        worker.Dispose();
    }

    [Fact]
    public async Task NullShellWorker_ReadErrorMessage_ReturnsEmpty() {
        var stream = new MemoryStream("data"u8.ToArray());
        ShellScript shellScript = new(new CommandLine(), "test");
        NullShellWorker worker = new(shellScript, stream, false);

        string error = await worker.ReadErrorMessageAsync();

        Assert.Equal(string.Empty, error);
        worker.Dispose();
    }

    [Fact]
    public void NullShellWorker_PipedWorker_IsNull() {
        var stream = new MemoryStream("data"u8.ToArray());
        ShellScript shellScript = new(new CommandLine(), "test");
        NullShellWorker worker = new(shellScript, stream, false);

        Assert.Null(worker.PipedWorker);
        worker.Dispose();
    }

    [Fact]
    public void NullShellWorker_ReadsToEof_ReturnsZero() {
        var stream = new MemoryStream("hi"u8.ToArray());
        ShellScript shellScript = new(new CommandLine(), "test");
        NullShellWorker worker = new(shellScript, stream, false);

        byte[] buffer = new byte[100];
        int read1 = worker.StandardOutput.Read(buffer);
        int read2 = worker.StandardOutput.Read(buffer);

        Assert.Equal(2, read1);
        Assert.Equal(0, read2);
        worker.Dispose();
    }
}
