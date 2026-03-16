using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     ASP.NET Core controller that executes commands using a configurable command executor
/// </summary>
[ApiController]
[Route("[controller]")]
public class CalqCmdController : ControllerBase {
    private readonly CalqCmdCacheOptions _cacheOptions;
    private readonly ICalqCommandExecutor _calqCommandExecutor;
    private readonly IDistributedCache _distributedCache;
    private readonly ILocalToolFactory _localToolFactory;

    public CalqCmdController(ICalqCommandExecutor calqCommandExecutor, ILocalToolFactory localToolFactory, IDistributedCache distributedCache, IOptions<CalqCmdCacheOptions> cacheOptions) {
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
            LocalTerminal.Shell = new CommandLine {
                In = Request.Body
            };
            LocalTerminal.Out = responseStream; // Void methods return ValueTuple and might write directly to the response body via RUN or LocalTerminal.Out

            string[] args = cmdValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            StreamWriter interfaceOut = new(responseStream);
            object? result = _calqCommandExecutor.Execute(args, interfaceOut);

            if (result is Task task) {
                await task;
                // Task methods return Task<VoidTaskResult> so task.GetType().IsGenericType is always true and hence unreliable
                result = task.GetType()
                    .GetProperty("Result")
                    ?.GetValue(task) ?? default(ValueTuple);
            }

            // For other result types, don't flush to avoid committing headers prematurely
            if (result is ValueTuple || result?.GetType()
                    .Name == "VoidTaskResult") {
                await interfaceOut.FlushAsync();
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

    [HttpPost("ReadErrorMessage")]
    [HttpGet("ReadErrorMessage")]
    public async Task<IActionResult> ReadErrorMessage([FromQuery] string? errorCode = null) {
        // Try query string first (GET), then header (POST)
        string? errorCodeValue = errorCode;
        if (string.IsNullOrEmpty(errorCodeValue)) {
            if (!Request.Headers.TryGetValue("error_code", out StringValues errorCodeValues)) {
                return BadRequest("Missing 'error_code' header");
            }

            errorCodeValue = errorCodeValues.FirstOrDefault() ?? "";
        }

        string cacheKey = GetErrorCacheKey(errorCodeValue);

        byte[]? bytes = await _distributedCache.GetAsync(cacheKey);
        if (bytes != null) {
            string errorMessage = Encoding.UTF8.GetString(bytes);
            return Ok(errorMessage);
        }

        return NotFound($"Error code '{errorCodeValue}' not found");
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

        throw new NotSupportedException("HTTP reset feature is not supported by the current server. Error details have been cached with error code: " + errorCode, ex);
    }

    private string GetErrorCacheKey(string errorCode) => $"{_cacheOptions.ErrorCacheKeyPrefix}{errorCode}";
}
