namespace backend.Application.Common.Models.GraphvizPdfConverter;

public class ConvertDotToPdfRequest
{
    public string DotContent { get; set; } = string.Empty;
    public string PageSize { get; set; } = "A0";
    public string Direction { get; set; } = "LR";
}