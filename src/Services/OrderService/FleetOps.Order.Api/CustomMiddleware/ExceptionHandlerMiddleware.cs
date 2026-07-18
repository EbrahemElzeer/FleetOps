using Microsoft.AspNetCore.Mvc;

namespace FleetOps.Order.Api.CustomMiddleware;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);

            if (httpContext.Response.StatusCode == StatusCodes.Status404NotFound && !httpContext.Response.HasStarted)
                await WriteNotFoundResponseAsync(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext,exception);
              
              
        }
    }

    private async Task HandleExceptionAsync( HttpContext httpContext, Exception exception)       
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        if (httpContext.Response.HasStarted)
            throw exception;

        httpContext.Response.Clear();
        httpContext.Response.StatusCode =StatusCodes.Status500InternalServerError;
           
        httpContext.Response.ContentType ="application/problem+json";
            

        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected server error occurred.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
           

        await httpContext.Response.WriteAsJsonAsync(problemDetails);
          
    }

    private static async Task WriteNotFoundResponseAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType ="application/problem+json";
           

        var problemDetails = new ProblemDetails
        {
            Title = "Endpoint not found.",
            Status = StatusCodes.Status404NotFound,
            Detail =$"The endpoint '{httpContext.Request.Path}' was not found.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =httpContext.TraceIdentifier;
            

        await httpContext.Response.WriteAsJsonAsync(problemDetails);
            
    }
}