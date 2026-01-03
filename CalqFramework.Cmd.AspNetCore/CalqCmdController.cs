using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Concurrent;
using System.Text;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ASP.NET Core controller that provides streaming endpoints with hardcoded responses
/// </summary>
[ApiController]
[Route("[controller]")]
public class CalqCmdController : ControllerBase
{
    // Cache for storing exception messages by error code
    private static readonly ConcurrentDictionary<string, string> ExceptionCache = new();

    /// <summary>
    /// Main endpoint that executes commands and returns streaming responses
    /// </summary>
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> ExecuteScript()
    {
        try
        {
            // Read the request body
            string requestBody;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            // Get the script command from headers
            if (!Request.Headers.TryGetValue("script", out var scriptValues))
            {
                return BadRequest("Missing 'script' header");
            }

            string script = scriptValues.FirstOrDefault() ?? "";
            
            // Return hardcoded streaming response
            return await ReturnHardcodedStream(requestBody, script);
        }
        catch (Exception ex)
        {
            // Handle exceptions
            return await HandleException(ex);
        }
    }

    /// <summary>
    /// Endpoint for retrieving cached error messages by error code
    /// </summary>
    [HttpGet("read_error_message")]
    [HttpPost("read_error_message")]
    public IActionResult ReadErrorMessage()
    {
        if (!Request.Headers.TryGetValue("error_code", out var errorCodeValues))
        {
            return BadRequest("Missing 'error_code' header");
        }

        string errorCode = errorCodeValues.FirstOrDefault() ?? "";
        
        if (ExceptionCache.TryGetValue(errorCode, out var errorMessage))
        {
            return Ok(errorMessage);
        }
        
        return NotFound($"Error code '{errorCode}' not found");
    }

    /// <summary>
    /// Returns a hardcoded streaming response
    /// </summary>
    private Task<IActionResult> ReturnHardcodedStream(string input, string script)
    {
        Response.ContentType = "text/plain";
        
        return Task.FromResult<IActionResult>(new CallbackResult("text/plain", async (stream, cancellationToken) =>
        {
            // Hardcoded stream response - you can customize this logic
            var hardcodedResponse = $"Hardcoded response for script: {script}\nInput received: {input}\nThis is a streaming response.\n";
            
            // Split into chunks to simulate streaming
            var chunks = hardcodedResponse.Split('\n');
            
            foreach (var chunk in chunks)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                // Write the chunk to the stream
                var bytes = Encoding.UTF8.GetBytes(chunk + "\n");
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
        }));
    }

    /// <summary>
    /// Handles exceptions by caching them with hash codes and optionally resetting the stream
    /// </summary>
    private Task<IActionResult> HandleException(Exception ex)
    {
        // Generate stack trace
        string stackTrace = ex.ToString();
        
        // Generate error code using string hash code, mapped to the original range (256 to 0xFFFFFFFF)
        int hashCode = stackTrace.GetHashCode();
        uint normalizedHash = (uint)Math.Abs(hashCode);
        uint errorCode = 256 + (normalizedHash % (0xFFFFFFFF - 256));
        
        // Cache the exception
        ExceptionCache[errorCode.ToString()] = stackTrace;
        
        // Try to reset the HTTP/2 stream with the error code
        var resetFeature = HttpContext.Features.Get<IHttpResetFeature>();
        if (resetFeature != null)
        {
            // Reset the stream with the error code
            resetFeature.Reset((int)errorCode);
            // Return empty result since the stream is being reset
            return Task.FromResult<IActionResult>(new EmptyResult());
        }
        
        // Fallback: return error response with the error code
        Response.StatusCode = 500;
        return Task.FromResult<IActionResult>(new ObjectResult(new { error_code = errorCode, message = ex.Message }));
    }

    /// <summary>
    /// Caches an exception message with the given error code (used by CallbackResult)
    /// </summary>
    internal static void CacheException(string errorCode, string stackTrace)
    {
        ExceptionCache[errorCode] = stackTrace;
    }
}

/// <summary>
/// Custom ActionResult for streaming responses
/// Allows writing directly to the response stream with proper content type handling
/// </summary>
public class CallbackResult : IActionResult
{
    private readonly string _contentType;
    private readonly Func<Stream, CancellationToken, Task> _callback;

    public CallbackResult(string contentType, Func<Stream, CancellationToken, Task> callback)
    {
        _contentType = contentType;
        _callback = callback;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = _contentType;
        
        try
        {
            await _callback(response.Body, context.HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            // Reset stream on any exception with a hash of the stack trace
            string stackTrace = ex.ToString();
            int hashCode = stackTrace.GetHashCode();
            uint normalizedHash = (uint)Math.Abs(hashCode);
            
            // Map hash to configurable integer range
            uint errorCode = 256 + (normalizedHash % (0xFFFFFFFF - 256));
            
            // Cache the exception
            CalqCmdController.CacheException(errorCode.ToString(), stackTrace);
            
            // Try to reset the HTTP/2 stream with the error code
            var resetFeature = context.HttpContext.Features.Get<IHttpResetFeature>();
            if (resetFeature != null)
            {
                // TODO HttpClient throws on stream read if it already received RST_STREAM even before the output leading to the RESET has been read so consider fixing this wait time
                await Task.Delay(1000, context.HttpContext.RequestAborted);
                resetFeature.Reset((int)errorCode);
                return;
            }
            
            // Fallback: write error information to the stream if reset is not available
            try
            {
                var errorBytes = Encoding.UTF8.GetBytes($"\nERROR: {ex.Message}\n");
                await response.Body.WriteAsync(errorBytes, context.HttpContext.RequestAborted);
            }
            catch
            {
                // Ignore if we can't write the error
            }
        }
    }
}