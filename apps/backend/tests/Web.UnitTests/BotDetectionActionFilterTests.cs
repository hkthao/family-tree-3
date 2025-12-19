using System.Net.Mime;
using backend.Web.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Web.UnitTests;

public class BotDetectionActionFilterTests
{
    private BotDetectionActionFilter CreateFilter(BotDetectionSettings? settings = default)
    {
        var mockLogger = new Mock<ILogger<BotDetectionActionFilter>>();
        var options = Options.Create(settings ?? new BotDetectionSettings());
        return new BotDetectionActionFilter(mockLogger.Object, options);
    }

    private ActionExecutingContext CreateContext(
        string? userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36",
        string? accept = MediaTypeNames.Application.Json,
        string? contentType = null,
        string httpMethod = "GET",
        bool hasIpAddress = true)
    {
        var httpContext = new DefaultHttpContext();
        if (hasIpAddress)
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        }
        httpContext.Request.Method = httpMethod;

        if (userAgent != null)
        {
            httpContext.Request.Headers.UserAgent = userAgent;
        }
        if (accept != null)
        {
            httpContext.Request.Headers.Accept = accept;
        }
        if (contentType != null)
        {
            httpContext.Request.Headers.ContentType = contentType;
        }

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(), // Corrected type for actionArguments
            Mock.Of<Controller>()
        );
    }

    [Theory]
    [InlineData("bot")]
    [InlineData("spider")]
    [InlineData("python-requests")]
    public void OnActionExecuting_BlacklistedUserAgent_ContinuesExecution(string blacklistedAgent)
    {
        // Arrange
        var settings = new BotDetectionSettings { BlacklistedUserAgents = new[] { "bot", "spider", "python-requests" } };
        var filter = CreateFilter(settings);
        var context = CreateContext(userAgent: blacklistedAgent);

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result); // Should not set a result, as User-Agent is only logged now
    }

    [Fact]
    public void OnActionExecuting_ValidUserAgent_ContinuesExecution()
    {
        // Arrange
        var settings = new BotDetectionSettings { BlacklistedUserAgents = new[] { "bot", "spider" } };
        var filter = CreateFilter(settings);
        var context = CreateContext(userAgent: "Mozilla/5.0 (Windows NT 10.0)");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result); // Should not set a result, allowing execution to continue
    }

    [Fact]
    public void OnActionExecuting_EmptyUserAgent_ContinuesExecution()
    {
        // Arrange
        var settings = new BotDetectionSettings { BlockEmptyUserAgent = true }; // BlockEmptyUserAgent is ignored for blocking now
        var filter = CreateFilter(settings);
        var context = CreateContext(userAgent: "");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result); // Should not set a result, as empty User-Agent is no longer blocked
    }

    [Fact]
    public void OnActionExecuting_MissingAcceptHeader_Returns403()
    {
        // Arrange
        var filter = CreateFilter(); // Default settings allow "application/json", "*/*"
        var context = CreateContext(accept: "");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.NotNull(context.Result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("*/*")]
    [InlineData("application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/webp,*/*;q=0.8")] // Complex but valid
    public void OnActionExecuting_ValidAcceptHeader_ContinuesExecution(string validAcceptHeader)
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(accept: validAcceptHeader);

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_InvalidAcceptHeader_Returns403()
    {
        // Arrange
        var settings = new BotDetectionSettings { AllowedAcceptHeaders = new[] { "application/json" } };
        var filter = CreateFilter(settings);
        var context = CreateContext(accept: "image/jpeg");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.NotNull(context.Result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }

    [Fact]
    public void OnActionExecuting_PostRequest_MissingContentType_Returns403()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(httpMethod: "POST", contentType: "");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.NotNull(context.Result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }

    [Fact]
    public void OnActionExecuting_PostRequest_InvalidContentType_Returns403()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(httpMethod: "POST", contentType: "text/plain");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.NotNull(context.Result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }

    [Fact]
    public void OnActionExecuting_PostRequest_ValidContentType_ContinuesExecution()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(httpMethod: "POST", contentType: MediaTypeNames.Application.Json);

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }
}
