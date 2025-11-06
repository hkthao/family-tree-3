using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using McpServer.Services.Ai;
using McpServer.Services.Ai.AITools;
using System.Collections.Generic;
using System.Linq;
using McpServer.Services.Integrations;
using McpServer.Models;
using McpServer.Common.Models;

namespace McpServer.E2ETests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });

        builder.UseEnvironment("Development");
    }
}
