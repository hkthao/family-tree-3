namespace backend.Application.Common.Models.GraphvizPdfConverter;

public class GraphvizPdfConverterResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? OutputFilePath { get; set; }
    public string? ErrorMessage { get; set; }
}
