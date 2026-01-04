using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Concurrent;
using System.Text;
using CalqFramework.Cli;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ASP.NET Core controller that executes CLI commands using CalqFramework.Cli
/// </summary>
[ApiController]
[Route("[controller]")]
public class CalqCmdController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, string> ExceptionCache = new();
    
    private readonly object _cliTarget;

    public CalqCmdController(object cliTarget)
    {
        _cliTarget = cliTarget;
    }

    [HttpPost]
    [Route("")]
    public async Task<Stream> ExecuteScript()
    {
        try
        {
            if (!Request.Headers.TryGetValue("Script", out var scriptValues))
            {
                return CreateErrorStream("Missing 'Script' header");
            }

            string script = scriptValues.FirstOrDefault() ?? "";
            
            LocalTerminal.Shell = new CommandLine() { In = Request.Body };
            
            var args = script.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cli = new CommandLineInterface();
            var result = cli.Execute(_cliTarget, args);
            
            if (result is Task task)
            {
                await task;
                
                result = task switch
                {
                    Task<string> stringTask => stringTask.Result,
                    Task<Stream> streamTask => streamTask.Result,
                    Task<int> intTask => intTask.Result,
                    Task<object> objectTask => objectTask.Result,
                    _ when task.GetType().IsGenericType => 
                        task.GetType().GetProperty("Result")?.GetValue(task),
                    _ => ResultVoid.Value
                };
            }
            
            return result switch
            {
                Stream stream => stream,
                ResultVoid => CreateEmptyStream(),
                string str => CreateStringStream(str),
                null => CreateEmptyStream(),
                _ => CreateObjectStream(result)
            };
        }
        catch (Exception ex)
        {
            return await HandleExceptionStreamAsync(ex);
        }
    }

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

    private async Task<Stream> HandleExceptionStreamAsync(Exception ex)
    {
        string stackTrace = ex.ToString();
        
        int hashCode = stackTrace.GetHashCode();
        uint normalizedHash = (uint)Math.Abs(hashCode);
        uint errorCode = 256 + (normalizedHash % (0xFFFFFFFF - 256));
        
        ExceptionCache[errorCode.ToString()] = stackTrace;
        
        var resetFeature = HttpContext.Features.Get<IHttpResetFeature>();
        if (resetFeature != null)
        {
            // TODO HttpClient throws on stream read if it already received RST_STREAM even before the output leading to the RESET has been read so consider fixing this wait time
            await Task.Delay(1000);
            
            resetFeature.Reset((int)errorCode);
            return Stream.Null;
        }
        
        Response.StatusCode = 500;
        var errorResponse = System.Text.Json.JsonSerializer.Serialize(new { error_code = errorCode, message = ex.Message });
        return CreateStringStream(errorResponse);
    }

    private Stream CreateStringStream(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    private Stream CreateObjectStream(object obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return CreateStringStream(json);
    }

    private Stream CreateEmptyStream()
    {
        return new MemoryStream();
    }

    private Stream CreateErrorStream(string message)
    {
        Response.StatusCode = 400;
        return CreateStringStream(message);
    }
}