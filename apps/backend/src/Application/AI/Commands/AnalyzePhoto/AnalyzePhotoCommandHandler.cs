using System.Net.Http.Headers; // For MediaTypeHeaderValue
using System.Net.Http.Json; // Added for ReadFromJsonAsync
using System.Text.Json; // Added for JsonDocument
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting; // Added for N8nSettings
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options; // Added for IOptions

namespace backend.Application.AI.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandHandler : IRequestHandler<AnalyzePhotoCommand, Result<PhotoAnalysisResultDto>>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context; // To check if memberId exists
    private readonly IAuthorizationService _authorizationService;
    private readonly HttpClient _httpClient; // Keep HttpClient for n8n webhook
    private readonly N8nSettings _n8nSettings; // Replaced IConfiguration with N8nSettings

    public AnalyzePhotoCommandHandler(IMapper mapper, IApplicationDbContext context, IAuthorizationService authorizationService, HttpClient httpClient, IOptions<N8nSettings> n8nSettings)
    {
        _mapper = mapper;
        _context = context;
        _authorizationService = authorizationService;
        _httpClient = httpClient;
        _n8nSettings = n8nSettings.Value; // Assign the value
    }

    public async Task<Result<PhotoAnalysisResultDto>> Handle(AnalyzePhotoCommand request, CancellationToken cancellationToken)
    {
        // Check if memberId exists if provided in Input.MemberInfo
        if (request.Input.MemberInfo?.Id != null)
        {
            if (!Guid.TryParse(request.Input.MemberInfo.Id, out var memberIdGuid))
            {
                return Result<PhotoAnalysisResultDto>.Failure($"Invalid Member ID format: {request.Input.MemberInfo.Id}", ErrorSources.BadRequest);
            }

            var member = await _context.Members.FindAsync(new object[] { memberIdGuid }, cancellationToken);
            if (member == null)
            {
                return Result<PhotoAnalysisResultDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Input.MemberInfo.Id} not found."), ErrorSources.NotFound);
            }
            // Authorization check (can access family of the member)
            if (!_authorizationService.CanAccessFamily(member.FamilyId))
            {
                return Result<PhotoAnalysisResultDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }
        }

        // 3. Call n8n WebHook for advanced analysis and suggestions
        var n8nPhotoAnalysisWebhook = _n8nSettings.PhotoAnalysisWebhook;
        if (string.IsNullOrEmpty(n8nPhotoAnalysisWebhook))
        {
            return Result<PhotoAnalysisResultDto>.Failure("N8n configuration for photo analysis is missing.");
        }

        // Use the AiPhotoAnalysisInputDto directly as payload for n8n webhook
        // n8n will handle the image processing and AI analysis
        var n8nRequestPayload = request.Input;

        HttpResponseMessage n8nHttpResponse;
        try
        {
            // Ensure content type is application/json
            var jsonContent = JsonContent.Create(n8nRequestPayload, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            n8nHttpResponse = await _httpClient.PostAsync(n8nPhotoAnalysisWebhook, jsonContent, cancellationToken);
            n8nHttpResponse.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            return Result<PhotoAnalysisResultDto>.Failure($"Lỗi khi gọi webhook n8n để phân tích ảnh: {ex.Message}", ErrorSources.ExternalServiceError);
        }

        PhotoAnalysisResultDto? photoAnalysisResult;
        try
        {
            photoAnalysisResult = await n8nHttpResponse.Content.ReadFromJsonAsync<PhotoAnalysisResultDto>(cancellationToken);
        }
        catch (JsonException)
        {
            return Result<PhotoAnalysisResultDto>.Failure("Phân tích ảnh từ n8n trả về phản hồi không hợp lệ.", ErrorSources.ExternalServiceError);
        }

        if (photoAnalysisResult == null)
        {
            return Result<PhotoAnalysisResultDto>.Failure("Phân tích ảnh từ n8n trả về phản hồi rỗng.");
        }

        // Add CreatedAt to the result if it's not already set by n8n
        if (photoAnalysisResult.CreatedAt == default)
        {
            photoAnalysisResult.CreatedAt = DateTime.UtcNow;
        }

        return Result<PhotoAnalysisResultDto>.Success(photoAnalysisResult);
    }
}
