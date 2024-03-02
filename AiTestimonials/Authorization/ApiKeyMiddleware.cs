using System.Net;

namespace AiTestimonials.Authorization;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyValidation _apiKeyValidation;

    private readonly List<string> _utilityEndpoints =
    [
        "/health/ready",
        "/health/live"
    ];

    public ApiKeyMiddleware(RequestDelegate next, IApiKeyValidation apiKeyValidation)
    {
        _next = next;
        _apiKeyValidation = apiKeyValidation;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        if (_utilityEndpoints.Contains(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(context.Request.Headers[AuthConstants.ApiKeyHeaderName]))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        string? userApiKey = context.Request.Headers[AuthConstants.ApiKeyHeaderName];
        if (!_apiKeyValidation.IsValidApiKey(userApiKey!))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        await _next(context);
    }
}
