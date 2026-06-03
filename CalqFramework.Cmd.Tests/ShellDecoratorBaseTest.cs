using CalqFramework.Cmd.Shell.Durable;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.Tests;

public class ShellDecoratorBaseTest {
    [Fact]
    public void ShellDecoratorBase_DelegatesExceptionFactory() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(
            Path.Combine(
                Path.GetTempPath(),
                "calq-test-" + Guid.NewGuid()
                    .ToString("N")[..8]));
        using DurableShell decorator = new(inner, store, "test");

        Assert.Same(inner.ExceptionFactory, decorator.ExceptionFactory);
    }

    [Fact]
    public void ShellDecoratorBase_DelegatesPostprocessor() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(
            Path.Combine(
                Path.GetTempPath(),
                "calq-test-" + Guid.NewGuid()
                    .ToString("N")[..8]));
        using DurableShell decorator = new(inner, store, "test");

        Assert.Same(inner.Postprocessor, decorator.Postprocessor);
    }

    [Fact]
    public void ShellDecoratorBase_DelegatesIn() {
        var inputStream = new MemoryStream();
        CommandLine inner = new() {
            In = inputStream
        };
        FileSystemDurabilityStore store = new(
            Path.Combine(
                Path.GetTempPath(),
                "calq-test-" + Guid.NewGuid()
                    .ToString("N")[..8]));
        using DurableShell decorator = new(inner, store, "test");

        Assert.Same(inputStream, decorator.In);
    }

    [Fact]
    public void ShellDecoratorBase_DelegatesMapToHostPath() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(
            Path.Combine(
                Path.GetTempPath(),
                "calq-test-" + Guid.NewGuid()
                    .ToString("N")[..8]));
        using DurableShell decorator = new(inner, store, "test");

        string testPath = @"C:\temp\test";
        Assert.Equal(inner.MapToHostPath(testPath), decorator.MapToHostPath(testPath));
    }

    [Fact]
    public void ShellDecoratorBase_DelegatesMapToInternalPath() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(
            Path.Combine(
                Path.GetTempPath(),
                "calq-test-" + Guid.NewGuid()
                    .ToString("N")[..8]));
        using DurableShell decorator = new(inner, store, "test");

        string testPath = @"C:\temp\test";
        Assert.Equal(inner.MapToInternalPath(testPath), decorator.MapToInternalPath(testPath));
    }
}
