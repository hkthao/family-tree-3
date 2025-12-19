using backend.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Web.UnitTests;

public class ApiKeyAuthenticationFilterTests
{
    private const string TestApiKeyHeaderName = "X-Test-Api-Key";
    private const string ValidApiKeyValue = "test-api-key-123";

    private ApiKeyAuthenticationFilter CreateFilter(ApiKeySettings? settings = default)
    {
        var mockLogger = new Mock<ILogger<ApiKeyAuthenticationFilter>>();
        var options = Options.Create(settings ?? new ApiKeySettings
        {
            HeaderName = TestApiKeyHeaderName,
            ApiKeyValue = ValidApiKeyValue
        });
        return new ApiKeyAuthenticationFilter(mockLogger.Object, options);
    }

    private ActionExecutingContext CreateContext(
        string? apiKeyHeaderValue = null,
        bool isAnonymous = false,
        bool hasIpAddress = true)
    {
        var httpContext = new DefaultHttpContext();
        if (hasIpAddress)
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        }

        if (apiKeyHeaderValue != null)
        {
            httpContext.Request.Headers[TestApiKeyHeaderName] = apiKeyHeaderValue;
        }

        var actionDescriptor = new ActionDescriptor();
        if (isAnonymous)
        {
            actionDescriptor.EndpointMetadata = new List<object> { new AllowAnonymousAttribute() };
        }
        else
        {
            actionDescriptor.EndpointMetadata = new List<object>();
        }


        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            actionDescriptor
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            Mock.Of<Controller>()
        );
    }

    private async Task<ActionExecutedContext> CallFilter(ApiKeyAuthenticationFilter filter, ActionExecutingContext context)
    {
        var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>());
        var nextDelegate = new ActionExecutionDelegate(() => Task.FromResult(executedContext));
        await filter.OnActionExecutionAsync(context, nextDelegate);
        return executedContext;
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidApiKey_ContinuesExecution()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(apiKeyHeaderValue: ValidApiKeyValue);

        // Act
        await CallFilter(filter, context);

        // Assert
        Assert.Null(context.Result); // Should not set a result, allowing execution to continue
    }

    [Fact]
    public async Task OnActionExecutionAsync_MissingApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(apiKeyHeaderValue: null); // Missing API Key

        // Act
        await CallFilter(filter, context);

        // Assert
        Assert.NotNull(context.Result);
        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(apiKeyHeaderValue: "wrong-api-key"); // Invalid API Key

        // Act
        await CallFilter(filter, context);

        // Assert
        Assert.NotNull(context.Result);
        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_AllowAnonymous_ContinuesExecutionWithoutApiKey()
    {
        // Arrange
        var filter = CreateFilter();
        var context = CreateContext(apiKeyHeaderValue: null, isAnonymous: true); // AllowAnonymous, missing API Key

        // Act
        await CallFilter(filter, context);

        // Assert
        Assert.Null(context.Result); // Should not set a result, allowing execution to continue
    }
}
