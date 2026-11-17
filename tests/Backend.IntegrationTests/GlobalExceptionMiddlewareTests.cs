using System.Text.Json;
using Backend.API.Middleware;
using Backend.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.IntegrationTests;

public class GlobalExceptionMiddlewareTests
{
    public static TheoryData<Exception, int, string> Cases => new()
    {
        { new NotFoundException("Thing", 1), 404, "Thing '1' was not found." },
        { new DomainException("Rule broken."), 400, "Rule broken." },
        { new InvalidOperationException("secret detail"), 500, "An unexpected error occurred." },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task MapsExceptionToStandardResponse(Exception exception, int expectedStatus, string expectedMessage)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.False(body.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedMessage, body.RootElement.GetProperty("message").GetString());
    }
}