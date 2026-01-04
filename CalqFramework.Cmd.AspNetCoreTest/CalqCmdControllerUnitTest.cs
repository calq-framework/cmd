using CalqFramework.Cmd.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class CalqCmdControllerUnitTest
{
    private CalqCmdController CreateController()
    {
        var controller = new CalqCmdController();
        
        // Set up HttpContext
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        
        return controller;
    }

    [Fact]
    public async Task ExecuteScript_WithValidRequest_ReturnsStreamingResponse()
    {
        // Arrange
        var controller = CreateController();
        var requestBody = "test input data";
        var scriptHeader = "test_script.py";
        
        // Set up request body
        var bodyBytes = Encoding.UTF8.GetBytes(requestBody);
        controller.Request.Body = new MemoryStream(bodyBytes);
        controller.Request.Headers["script"] = scriptHeader;

        // Act
        var result = await controller.ExecuteScript();

        // Assert
        Assert.IsType<CallbackResult>(result);
        
        // Test the callback result by executing it
        var callbackResult = (CallbackResult)result;
        var actionContext = new ActionContext(controller.HttpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        
        using var responseStream = new MemoryStream();
        controller.Response.Body = responseStream;
        
        await callbackResult.ExecuteResultAsync(actionContext);
        
        var responseBody = Encoding.UTF8.GetString(responseStream.ToArray());
        Assert.Contains("Hardcoded response for script: test_script.py", responseBody);
        Assert.Contains("Input received: test input data", responseBody);
        Assert.Contains("This is a streaming response.", responseBody);
    }

    [Fact]
    public async Task ExecuteScript_WithMissingScriptHeader_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        var requestBody = "test input data";
        
        // Set up request body without script header
        var bodyBytes = Encoding.UTF8.GetBytes(requestBody);
        controller.Request.Body = new MemoryStream(bodyBytes);

        // Act
        var result = await controller.ExecuteScript();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Missing 'script' header", badRequestResult.Value);
    }

    [Fact]
    public async Task ReadErrorMessage_WithValidErrorCode_ReturnsErrorMessage()
    {
        // Arrange
        var controller = CreateController();
        
        // Trigger an exception to cache it
        var requestBody = "test input";
        var bodyBytes = Encoding.UTF8.GetBytes(requestBody);
        controller.Request.Body = new MemoryStream(bodyBytes);
        controller.Request.Headers["script"] = "test_exception_trigger";
        
        // Act - trigger the exception which will cache it
        var exceptionResult = await controller.ExecuteScript();
        
        // Assert - verify we got the expected exception response
        var objectResult = Assert.IsType<ObjectResult>(exceptionResult);
        Assert.Equal(500, controller.Response.StatusCode);
        
        // Extract the error code from the exception response
        var resultValue = objectResult.Value;
        Assert.NotNull(resultValue);
        var errorCodeProperty = resultValue.GetType().GetProperty("error_code");
        Assert.NotNull(errorCodeProperty);
        var errorCode = errorCodeProperty.GetValue(resultValue);
        Assert.NotNull(errorCode);
        
        // Now test reading the cached error message
        var readController = CreateController();
        readController.Request.Headers["error_code"] = errorCode.ToString();
        var readResult = readController.ReadErrorMessage();
        
        // Assert - verify we can retrieve the cached stack trace
        var okResult = Assert.IsType<OkObjectResult>(readResult);
        var cachedStackTrace = okResult.Value?.ToString();
        
        Assert.NotNull(cachedStackTrace);
        Assert.Contains("Test exception for error caching", cachedStackTrace);
        Assert.Contains("InvalidOperationException", cachedStackTrace);
        Assert.Contains("CalqCmdController.ExecuteScript", cachedStackTrace);
        
        // The cached value should be the full stack trace, much longer than just the exception message
        Assert.True(cachedStackTrace.Length > 100, 
            $"Expected full stack trace to be longer than 100 chars, got {cachedStackTrace.Length}");
    }

    [Fact]
    public void ReadErrorMessage_WithInvalidErrorCode_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        controller.Request.Headers["error_code"] = "nonexistent";

        // Act
        var result = controller.ReadErrorMessage();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Error code 'nonexistent' not found", notFoundResult.Value?.ToString());
    }

    [Fact]
    public void ReadErrorMessage_WithMissingErrorCodeHeader_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.ReadErrorMessage();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Missing 'error_code' header", badRequestResult.Value);
    }
}