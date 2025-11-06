
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text;

namespace McpServer.E2ETests;

public class AiControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AiControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Status_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/ai/status");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task QueryAi_ShowFamilyDetails_ReturnsExpectedResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImVlOUx4b24yV0xaMUNvN2g3aFMyTCJ9.eyJodHRwczovL2ZhbWlseXRyZWUuY29tL3JvbGVzIjpbIkFkbWluIl0sImh0dHBzOi8vZmFtaWx5dHJlZS5jb20vZW1haWwiOiJ0aGFvLmhrOTBAZ21haWwuY29tIiwiaHR0cHM6Ly9mYW1pbHl0cmVlLmNvbS9uYW1lIjoidGhhby5oazkwQGdtYWlsLmNvbSIsImlzcyI6Imh0dHBzOi8vZGV2LWc3NnRxMDBnaWN3ZHprM3oudXMuYXV0aDAuY29tLyIsInN1YiI6ImF1dGgwfDY4ZTM4YTVhOTY5MTA3ZWJhYTkxMjU3NyIsImF1ZCI6WyJodHRwOi8vbG9jYWxob3N0OjUwMDAiLCJodHRwczovL2Rldi1nNzZ0cTAwZ2ljd2R6azN6LnVzLmF1dGgwLmNvbS91c2VyaW5mbyJdLCJpYXQiOjE3NjIzOTcwNjcsImV4cCI6MTc2MjQ4MzQ2Nywic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCIsImF6cCI6InY0alNlNVFSNFVqNmRkb0JCTUhOdGFETkh3djhVelFOIn0.SnOY4oxPYhI3vqxzFFaDT4kDdqirNl2AJHKOFpgOPoz5NohPTiJnWSgtCztiWrAnU93TlnWtBlqQ0cneJR-CCV_tToTeM2CICRXTxtWoq9rEbuxYl1_9rWHyOu3oQGFrVL9E3cLk5tYI83HjdfbiUqo5FTjFsP_jx4UoqJp1L5SGpfeDEtV57suF8Mr0HcQ_yZbAAn568xb7nztQZelQKbxMLr3T4o6uYDTU0PxnTkMFVrc2W3HGofp-Qa6WB2aO3sxxHiGoVmdQV8bx2WAiqjA10nu3H7Vn2ffADUPqD4uG7h6WEzIgZYPu0B6Kg3Zb_PPf04MfuM3Dl9Q8azS7GQ");

        var requestBody = new AiQueryRequest { Prompt = "show me detail family with id 1a955fff-ce01-422f-8bb3-02ab14e8ec47" };

        // Act
        var response = await client.PostAsJsonAsync("/api/ai/query", requestBody);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseStream = await response.Content.ReadAsStreamAsync();
        using var reader = new System.IO.StreamReader(responseStream);
        var fullResponse = new StringBuilder();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrEmpty(line))
            {
                fullResponse.Append(line);
            }
        }

        // This assertion will need to be refined based on the actual expected output
        // from the AI service when it processes the tool call.
        // For now, we'll check for a generic success message or an indication of tool execution.
        Assert.Contains("Thông tin gia đình", fullResponse.ToString());
    }
}

// This class needs to be defined in the test project to be used in the test.
public class AiQueryRequest
{
    public string Prompt { get; set; } = string.Empty;
}
