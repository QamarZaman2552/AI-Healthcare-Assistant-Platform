using System.Net;
using AIHealthcareAssistant.API.Middleware;
using AIHealthcareAssistant.Application.Common.Response;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AIHealthcareAssistant.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ArgumentException_ReturnsBadRequest()
    {
        var context = BuildContext();
        var exception = new ArgumentException("test message");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        await AssertResponseBody(context, "test message");
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ReturnsNotFound()
    {
        var context = BuildContext();
        var exception = new KeyNotFoundException("not found");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        await AssertResponseBody(context, "not found");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var context = BuildContext();
        var exception = new UnauthorizedAccessException("unauthorized");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        await AssertResponseBody(context, "unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationException_ReturnsConflict()
    {
        var context = BuildContext();
        var exception = new InvalidOperationException("conflict");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        await AssertResponseBody(context, "conflict");
    }

    [Fact]
    public async Task InvokeAsync_TimeoutException_ReturnsRequestTimeout()
    {
        var context = BuildContext();
        var exception = new TimeoutException();

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.RequestTimeout, context.Response.StatusCode);
        await AssertResponseBody(context, "The request timed out.");
    }

    [Fact]
    public async Task InvokeAsync_HttpRequestException_ReturnsServiceUnavailable()
    {
        var context = BuildContext();
        var exception = new HttpRequestException("unavailable");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, context.Response.StatusCode);
        await AssertResponseBody(context, "The requested service is currently unavailable.");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
    {
        var context = BuildContext();
        var exception = new Exception("unexpected");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        await AssertResponseBody(context, "An unexpected error occurred.");
    }

    [Fact]
    public async Task InvokeAsync_SetsContentTypeToJson()
    {
        var context = BuildContext();
        var exception = new ArgumentException("msg");

        await ExceptionHandlingMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    private static DefaultHttpContext BuildContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task AssertResponseBody(DefaultHttpContext context, string expectedMessage)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains(expectedMessage, body);
    }
}
