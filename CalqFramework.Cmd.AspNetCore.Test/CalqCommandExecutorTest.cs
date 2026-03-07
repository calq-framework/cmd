using System.IO;
using System.Text;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class CalqCommandExecutorTest {
    private readonly CalqCommandExecutor _executor = new();

    [Fact]
    public void Execute_WithPascalCaseMethod_ExecutesSuccessfully() {
        // Arrange
        var target = new TestCommands();
        string[] args = ["ProcessData", "--input", "test"];

        // Act
        var result = _executor.Execute(target, args);

        // Assert
        Assert.Equal("Processed: test", result);
    }

    [Fact]
    public void Execute_WithKebabCaseMethod_ThrowsException() {
        // Arrange
        var target = new TestCommands();
        string[] args = ["process-data", "--input", "test"]; // kebab-case should not work

        // Act & Assert
        Assert.Throws<Cli.CliException>(() => _executor.Execute(target, args));
    }

    [Fact]
    public void Execute_WithNameofOperator_ExecutesSuccessfully() {
        // Arrange
        var target = new TestCommands();
        string methodName = nameof(TestCommands.ProcessData);
        string[] args = [methodName, "--input", "nameof"];

        // Act
        var result = _executor.Execute(target, args);

        // Assert
        Assert.Equal("Processed: nameof", result);
    }

    [Fact]
    public void Execute_WithMultipleParameters_ExecutesSuccessfully() {
        // Arrange
        var target = new TestCommands();
        string[] args = ["Add", "--a", "5", "--b", "3"];

        // Act
        var result = _executor.Execute(target, args);

        // Assert
        Assert.Equal(8, result);
    }

    private class TestCommands {
        public string ProcessData(string input) => $"Processed: {input}";

        public int Add(int a, int b) => a + b;
    }
}
