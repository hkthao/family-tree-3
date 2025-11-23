using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace backend.Application.Families.Queries.ExportPdf;

// DTO for sending HTML content to the Puppeteer service
public class PdfGenerationRequestDto
{
    public string html { get; set; } = string.Empty;
}

public class GetFamilyPdfExportQueryHandler(
    IApplicationDbContext context,
    IAuthorizationService authorizationService,
    ICurrentUser currentUser,
    HttpClient httpClient) // Inject HttpClient
    : IRequestHandler<GetFamilyPdfExportQuery, Result<ExportedPdfFile>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly HttpClient _httpClient = httpClient; // Store HttpClient

    public async Task<Result<ExportedPdfFile>> Handle(GetFamilyPdfExportQuery request, CancellationToken cancellationToken)
    {
        // 1. Authorization
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<ExportedPdfFile>.Failure($"Family with ID {request.FamilyId} not found.", ErrorSources.NotFound);
        }

        var succeeded = _authorizationService.CanAccessFamily(request.FamilyId);
        if (!succeeded)
        {
            return Result<ExportedPdfFile>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 2. Prepare data for Puppeteer service
        var pdfRequestPayload = new PdfGenerationRequestDto
        {
            html = request.HtmlContent
        };

        // 3. Call Puppeteer PDF service
        var jsonContent = JsonSerializer.Serialize(pdfRequestPayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            // The service name 'puppeteer-service' will be used as the hostname in Docker Compose
            response = await _httpClient.PostAsync("http://puppeteer-service:3000/convert-to-pdf", httpContent, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<ExportedPdfFile>.Failure($"Failed to connect to PDF generation service: {ex.Message}", ErrorSources.ExternalServiceError);
        }
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<ExportedPdfFile>.Failure($"PDF generation service returned error: {response.StatusCode} - {errorContent}", ErrorSources.ExternalServiceError);
        }

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();

        var fileName = $"FamilyTree_{family.Name.Replace(" ", "_")}.pdf";
        return Result<ExportedPdfFile>.Success(new ExportedPdfFile(pdfBytes, fileName, "application/pdf"));
    }
}
