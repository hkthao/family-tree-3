using backend.Application.Common.Models.GraphvizPdfConverter; // Referencing the DTOs from Application

namespace backend.Application.Common.Interfaces.Services.GraphvizPdfConverter;

public interface IGraphvizPdfConverterClient
{
    Task<GraphvizPdfConverterResponse> RenderPdfFilenameAsync(GraphvizPdfConverterRequest request, CancellationToken cancellationToken = default);
    Task<byte[]> ConvertDotToPdfAsync(ConvertDotToPdfRequest request, CancellationToken cancellationToken = default);
}
