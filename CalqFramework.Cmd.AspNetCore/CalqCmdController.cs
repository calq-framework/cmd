using System.Text;
using System.Text.Json;
using CalqFramework.Cli;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     ASP.NET Core controller that executes commands using a configurable command executor
/// </summary>
[ApiController]
[Route("[controller]")]
public class CalqCmdController : ControllerBase {
    private readonly CalqCmdCacheOptions _cacheOptions;
    private readonly object _commandTarget;
    private readonly ICalqCommandExecutor _calqCommandExecutor;
    private readonly IDistributedCache _distributedCache;
    private readonly ILocalToolFactory _localToolFactory;

    public CalqCmdController(
        object commandTarget,
        ICalqCommandExecutor calqCommandExecutor,
        ILocalToolFactory localToolFactory,
        IDistributedCache distributedCache,
        IOptions<CalqCmdCacheOptions> cacheOptions) {
        _commandTarget = commandTarget;
        _calqCommandExecutor = calqCommandExecutor;
        _localToolFactory = localToolFactory;
        _distributedCache = distributedCache;
        _cacheOptions = cacheOptions.Value;

        // Set LocalHttpToolFactory as the default for LocalTool when used in ASP.NET Core context
        LocalTool.Factory = localToolFactory;
    }

    [HttpPost]
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> ExecuteScript([FromQuery] string? cmd = null) {
        try {
            // Try query string first (GET), then header (POST)
            string? cmdValue = cmd;
            if (string.IsNullOrEmpty(cmdValue)) {
                if (!Request.Headers.TryGetValue("cmd", out StringValues cmdValues)) {
                    return BadRequest("Missing 'cmd' query parameter or 'cmd' header");
                }
                cmdValue = cmdValues.FirstOrDefault() ?? "";
            }

            Stream responseStream = Response.BodyWriter.AsStream();
            LocalTerminal.Shell = new CommandLine { In = Request.Body };
            LocalTerminal.Out = responseStream; // Void methods return ResultVoid and might write directly to the response body via RUN or LocalTerminal.Out

            string[] args = cmdValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            StreamWriter outputWriter = new(responseStream);
            object? result = _calqCommandExecutor.Execute(_commandTarget, args, outputWriter);

            if (result is Task task) {
                await task;
                result = task.GetType().IsGenericType 
                    ? ((dynamic)task).Result 
                    : ResultVoid.Value;
            }

            // For other result types, don't flush to avoid committing headers prematurely
            if (result is ResultVoid) {
                await outputWriter.FlushAsync();
                return new EmptyResult();
            }

            return result switch {
                Stream stream => File(stream, "application/octet-stream"),
                string str => Content(str, "text/plain"),
                null => NoContent(),
                _ => Ok(result)
            };
        } catch (Exception ex) {
            return await HandleExceptionAsync(ex);
        }
    }

    [HttpGet("read_error_message")]
    [HttpPost("read_error_message")]
    public async Task<IActionResult> ReadErrorMessage() {
        if (!Request.Headers.TryGetValue("error_code", out StringValues errorCodeValues)) {
            return BadRequest("Missing 'error_code' header");
        }

        string errorCode = errorCodeValues.FirstOrDefault() ?? "";
        string cacheKey = GetErrorCacheKey(errorCode);

        byte[]? bytes = await _distributedCache.GetAsync(cacheKey);
        if (bytes != null) {
            string errorMessage = Encoding.UTF8.GetString(bytes);
            return Ok(errorMessage);
        }

        return NotFound($"Error code '{errorCode}' not found");
    }

    private async Task<IActionResult> HandleExceptionAsync(Exception ex) {
        string stackTrace = ex.ToString();

        int hashCode = stackTrace.GetHashCode();
        uint normalizedHash = (uint)Math.Abs(hashCode);
        uint errorCode = 256 + normalizedHash % (0xFFFFFFFF - 256);

        string errorCodeStr = errorCode.ToString();
        string cacheKey = GetErrorCacheKey(errorCodeStr);

        DistributedCacheEntryOptions cacheOptions = new() {
            AbsoluteExpirationRelativeToNow = _cacheOptions.ErrorCacheExpiration
        };

        byte[] bytes = Encoding.UTF8.GetBytes(stackTrace);
        await _distributedCache.SetAsync(cacheKey, bytes, cacheOptions);

        IHttpResetFeature? resetFeature = HttpContext.Features.Get<IHttpResetFeature>();
        if (resetFeature != null) {
            // TODO HttpClient throws on stream read if it already received RST_STREAM even before the output leading to the RESET has been read so consider fixing this wait time
            await Task.Delay(1000);

            resetFeature.Reset((int)errorCode);
            return new EmptyResult();
        }

        return StatusCode(500, new { error_code = errorCode, message = ex.Message });
    }

    private string GetErrorCacheKey(string errorCode) => $"{_cacheOptions.ErrorCacheKeyPrefix}{errorCode}";
}
