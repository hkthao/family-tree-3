using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using backend.Application.Common.Interfaces.Services.GraphvizPdfConverter;
using backend.Infrastructure.ExternalApiSettings; // Need a settings class for base URL
using backend.Application.Common.Models.GraphvizPdfConverter; // DTOs moved to Application

namespace backend.Infrastructure.Services.GraphvizPdfConverter;

public class GraphvizPdfConverterClient : IGraphvizPdfConverterClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GraphvizPdfConverterClient> _logger;

    public GraphvizPdfConverterClient(HttpClient httpClient, ILogger<GraphvizPdfConverterClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GraphvizPdfConverterResponse> RenderPdfFilenameAsync(GraphvizPdfConverterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Graphviz PDF Converter API to render PDF for job {JobId}", request.JobId);

        var response = await _httpClient.PostAsJsonAsync("/render-pdf-filename", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphvizPdfConverterResponse>(cancellationToken: cancellationToken);

        if (result == null)
        {
            _logger.LogError("Received null response from Graphviz PDF Converter API for job {JobId}", request.JobId);
            throw new Exception("Received null response from Graphviz PDF Converter API.");
        }

        _logger.LogInformation("Successfully rendered PDF for job {JobId}. Output file: {OutputFilePath}", request.JobId, result.OutputFilePath);
        return result;
    }
}
