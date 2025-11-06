using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Moq;
using McpServer.Services.Ai;
using McpServer.Services.Ai.Tools;
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

            // Remove the existing IAiProvider registration
            var aiProviderDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IAiProvider));
            if (aiProviderDescriptor != null)
            {
                services.Remove(aiProviderDescriptor);
            }

            // Mock IAiProvider to simulate a tool call
            var mockAiProvider = new Mock<IAiProvider>();
            mockAiProvider.Setup(p => p.GenerateToolUseResponseStreamAsync(
                It.Is<string>(prompt => prompt.Contains("show me detail family with id")),
                It.IsAny<List<AiToolDefinition>>(),
                It.IsAny<List<AiToolResult>>()))
                .Returns(() =>
                {
                    var toolCalls = new List<AiToolCall>
                    {
                        new AiToolCall("call_123", "get_family_details", "{\"id\": \"1a955fff-ce01-422f-8bb3-02ab14e8ec47\"}")
                    };
                    return GetAiResponsePartAsyncEnumerable(new AiToolCallResponsePart(toolCalls));
                });

            // Add the mocked IAiProvider
            services.AddScoped<IAiProvider>(sp => mockAiProvider.Object);

            // Remove the existing FamilyTreeBackendService registration
            var familyTreeBackendServiceDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(FamilyTreeBackendService));
            if (familyTreeBackendServiceDescriptor != null)
            {
                services.Remove(familyTreeBackendServiceDescriptor);
            }

            // Mock FamilyTreeBackendService
            var mockFamilyTreeBackendService = new Mock<FamilyTreeBackendService>();
            mockFamilyTreeBackendService.Setup(s => s.GetFamilyByIdAsync(
                It.Is<Guid>(id => id == Guid.Parse("1a955fff-ce01-422f-8bb3-02ab14e8ec47")),
                It.IsAny<string>()))
                .ReturnsAsync(new FamilyDto { Name = "Gia đình ABC", History = "Lịch sử gia đình ABC" });
            
            services.AddScoped<FamilyTreeBackendService>(sp => mockFamilyTreeBackendService.Object);
        });

        builder.UseEnvironment("Development");
    }

    private static async IAsyncEnumerable<AiResponsePart> GetAiResponsePartAsyncEnumerable(params AiResponsePart[] parts)
    {
        foreach (var part in parts)
        {
            await Task.Yield(); // To make it truly asynchronous
            yield return part;
        }
    }
}
