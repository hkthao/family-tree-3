using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net.Http; // Added for HttpClient
using System.Text.Json; // Added for JsonDocument
using System.Net.Http.Headers; // Added for MediaTypeHeaderValue
using System.Net.Http.Json; // Added for ReadFromJsonAsync
using Microsoft.Extensions.Options; // Added for IOptions
using backend.Application.Common.Models.AppSetting; // Added for N8nSettings and ImageProcessingServiceSettings

namespace backend.Application.Memories.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandHandler : IRequestHandler<AnalyzePhotoCommand, Result<PhotoAnalysisResultDto>>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context; // To check if memberId exists
    private readonly IAuthorizationService _authorizationService;
    private readonly HttpClient _httpClient; // Added HttpClient
    private readonly N8nSettings _n8nSettings; // Replaced IConfiguration with N8nSettings
    private readonly ImageProcessingServiceSettings _imageProcessingServiceSettings; // Added ImageProcessingServiceSettings

    public AnalyzePhotoCommandHandler(IMapper mapper, IApplicationDbContext context, IAuthorizationService authorizationService, HttpClient httpClient, IOptions<N8nSettings> n8nSettings, IOptions<ImageProcessingServiceSettings> imageProcessingServiceSettings)
    {
        _mapper = mapper;
        _context = context;
        _authorizationService = authorizationService;
        _httpClient = httpClient;
        _n8nSettings = n8nSettings.Value; // Assign the value
        _imageProcessingServiceSettings = imageProcessingServiceSettings.Value; // Assign the value
    }

    public async Task<Result<PhotoAnalysisResultDto>> Handle(AnalyzePhotoCommand request, CancellationToken cancellationToken)
    {
        // Check if memberId exists if provided
        if (request.MemberId.HasValue)
        {
            var member = await _context.Members.FindAsync(new object[] { request.MemberId.Value }, cancellationToken);
            if (member == null)
            {
                return Result<PhotoAnalysisResultDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
            }
            // Authorization check (can access family of the member)
            if (!_authorizationService.CanAccessFamily(member.FamilyId))
            {
                return Result<PhotoAnalysisResultDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }
        }

        var imageProcessingServiceUrl = _imageProcessingServiceSettings.BaseUrl;
        if (string.IsNullOrEmpty(imageProcessingServiceUrl))
        {
            return Result<PhotoAnalysisResultDto>.Failure("Image Processing Service base URL is not configured.");
        }

        // 1. Call image-processing-service for face detection
        var detectFacesEndpoint = $"{imageProcessingServiceUrl}/detect-faces/";
        DetectFacesResponseDto detectFacesResponse;

        using (var content = new MultipartFormDataContent())
        using (var stream = request.File.OpenReadStream())
        {
            content.Add(new StreamContent(stream), "file", request.File.FileName);
            
            HttpResponseMessage detectFacesHttpResponse;
            try
            {
                detectFacesHttpResponse = await _httpClient.PostAsync(detectFacesEndpoint, content, cancellationToken);
                detectFacesHttpResponse.EnsureSuccessStatusCode(); // Throws on HTTP error codes
            }
            catch (HttpRequestException ex)
            {
                return Result<PhotoAnalysisResultDto>.Failure($"Lỗi khi gọi dịch vụ xử lý ảnh để phát hiện khuôn mặt: {ex.Message}", ErrorSources.ExternalServiceError);
            }

            try
            {
                detectFacesResponse = (await detectFacesHttpResponse.Content.ReadFromJsonAsync<DetectFacesResponseDto>(cancellationToken))!;
            }
            catch (JsonException)
            {
                return Result<PhotoAnalysisResultDto>.Failure("Dữ liệu phản hồi phát hiện khuôn mặt không hợp lệ.", ErrorSources.ExternalServiceError);
            }
            
            if (detectFacesResponse == null || !detectFacesResponse.FaceLocations.Any())
            {
                return Result<PhotoAnalysisResultDto>.Failure("Không tìm thấy khuôn mặt nào trong ảnh.");
            }
        }

        // For now, let's process the first detected face
        var firstFace = detectFacesResponse.FaceLocations.First();

        // 2. Call image-processing-service for cropping and basic emotion analysis
        var cropAndAnalyzeFaceEndpoint = $"{imageProcessingServiceUrl}/crop-and-analyze-face/";
        CropAndAnalyzeFaceResponseDto cropAndAnalyzeResponse;

        using (var content = new MultipartFormDataContent())
        using (var stream = request.File.OpenReadStream())
        {
            content.Add(new StreamContent(stream), "file", request.File.FileName);
            content.Add(new StringContent(JsonSerializer.Serialize(firstFace)), "face_location_json");
            if (request.MemberId.HasValue)
            {
                content.Add(new StringContent(request.MemberId.Value.ToString()), "member_id");
            }

            HttpResponseMessage cropAndAnalyzeHttpResponse;
            try
            {
                cropAndAnalyzeHttpResponse = await _httpClient.PostAsync(cropAndAnalyzeFaceEndpoint, content, cancellationToken);
                cropAndAnalyzeHttpResponse.EnsureSuccessStatusCode(); // Throws on HTTP error codes
            }
            catch (HttpRequestException ex)
            {
                return Result<PhotoAnalysisResultDto>.Failure($"Lỗi khi gọi dịch vụ xử lý ảnh để cắt và phân tích khuôn mặt: {ex.Message}", ErrorSources.ExternalServiceError);
            }

            try
            {
                cropAndAnalyzeResponse = (await cropAndAnalyzeHttpResponse.Content.ReadFromJsonAsync<CropAndAnalyzeFaceResponseDto>(cancellationToken))!;
            }
            catch (JsonException)
            {
                return Result<PhotoAnalysisResultDto>.Failure("Dữ liệu phản hồi cắt và phân tích khuôn mặt không hợp lệ.", ErrorSources.ExternalServiceError);
            }

            if (cropAndAnalyzeResponse == null || string.IsNullOrEmpty(cropAndAnalyzeResponse.CroppedFaceBase64))
            {
                return Result<PhotoAnalysisResultDto>.Failure("Không thể cắt và phân tích khuôn mặt.");
            }
        }
        
        // 3. Call n8n WebHook for advanced analysis and suggestions
        var n8nPhotoAnalysisWebhook = _n8nSettings.PhotoAnalysisWebhook;
        if (string.IsNullOrEmpty(n8nPhotoAnalysisWebhook))
        {
            return Result<PhotoAnalysisResultDto>.Failure("N8n configuration for photo analysis is missing.");
        }

        var n8nRequestPayload = new N8nPhotoAnalysisRequestDto
        {
            CroppedFaceBase64 = cropAndAnalyzeResponse.CroppedFaceBase64,
            Faces = JsonSerializer.SerializeToDocument(detectFacesResponse.FaceLocations),
            MemberId = request.MemberId,
            Emotion = cropAndAnalyzeResponse.Emotion,
            Confidence = cropAndAnalyzeResponse.Confidence,
            OriginalFileName = request.File.FileName,
            FirstFaceLocation = firstFace
        };

        HttpResponseMessage n8nHttpResponse;
        try
        {
            n8nHttpResponse = await _httpClient.PostAsJsonAsync(n8nPhotoAnalysisWebhook, n8nRequestPayload, cancellationToken);
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
                        // Return a generic message for JSON parsing failures from N8n
                        return Result<PhotoAnalysisResultDto>.Failure("Phân tích ảnh từ n8n trả về phản hồi rỗng hoặc không hợp lệ.", ErrorSources.ExternalServiceError);
                    }        

        if (photoAnalysisResult == null)
        {
            return Result<PhotoAnalysisResultDto>.Failure("Phân tích ảnh từ n8n trả về phản hồi rỗng hoặc không hợp lệ.");
        }

        // Add CreatedAt to the result if it's not already set by n8n
        if (photoAnalysisResult.CreatedAt == default)
        {
            photoAnalysisResult.CreatedAt = DateTime.UtcNow;
        }

        return Result<PhotoAnalysisResultDto>.Success(photoAnalysisResult);
    }
}
