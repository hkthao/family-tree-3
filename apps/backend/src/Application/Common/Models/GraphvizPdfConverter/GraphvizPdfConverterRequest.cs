namespace backend.Application.Common.Models.GraphvizPdfConverter;

public class GraphvizPdfConverterRequest
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public string DotFilename { get; set; } = string.Empty;
    public string PageSize { get; set; } = "A0";
    public string Direction { get; set; } = "LR";
}