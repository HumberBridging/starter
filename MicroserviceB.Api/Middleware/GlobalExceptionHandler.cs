using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceB.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync( HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The W3C Trace ID of this request: the key to find it in Application Insights
        var traceId = Activity.Current?.TraceId.ToString()
                      ?? httpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            // Never send exception.Message to the client: it can leak internals (Module 7)
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        // Give the caller the trace ID instead of the stack trace
        problemDetails.Extensions.Add("traceId", traceId);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
