using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using backend.Application.Common.Interfaces.Services.GraphvizPdfConverter;
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

    public async Task<byte[]> ConvertDotToPdfAsync(ConvertDotToPdfRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Graphviz PDF Converter API to convert DOT content to PDF via /render-and-download.");

        using var formData = new MultipartFormDataContent();

        // Generate a new job_id for the request
        var jobId = Guid.NewGuid().ToString("N");

        // Add form fields
        formData.Add(new StringContent(jobId), "job_id");
        formData.Add(new StringContent(request.PageSize), "page_size");
        formData.Add(new StringContent(request.Direction), "direction");

        // Add dotContent as a file
        var dotFileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(request.DotContent));
        dotFileContent.Headers.Add("Content-Disposition", "form-data; name=\"dot_file\"; filename=\"graph.dot\"");
        formData.Add(dotFileContent, "dot_file");

        var response = await _httpClient.PostAsync("/render-and-download", formData, cancellationToken);

        response.EnsureSuccessStatusCode();

        var pdfBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        _logger.LogInformation("Successfully converted DOT content to PDF (job_id: {JobId}).", jobId);
        return pdfBytes;
    }
}
