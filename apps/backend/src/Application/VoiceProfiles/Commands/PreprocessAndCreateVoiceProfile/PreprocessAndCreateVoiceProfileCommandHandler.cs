using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.DTOs;
using backend.Application.VoiceProfiles.Queries;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using backend.Application.VoiceProfiles.Commands.CreateVoiceProfile;

namespace backend.Application.VoiceProfiles.Commands.PreprocessAndCreateVoiceProfile;

public class PreprocessAndCreateVoiceProfileCommandHandler : IRequestHandler<PreprocessAndCreateVoiceProfileCommand, Result<VoiceProfileDto>>
{
    private readonly IMediator _mediator;
    private readonly IVoiceAIService _voiceAIService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PreprocessAndCreateVoiceProfileCommandHandler> _logger;

    public PreprocessAndCreateVoiceProfileCommandHandler(
        IMediator mediator,
        IVoiceAIService voiceAIService,
        IFileStorageService fileStorageService,
        IHttpClientFactory httpClientFactory,
        ILogger<PreprocessAndCreateVoiceProfileCommandHandler> logger)
    {
        _mediator = mediator;
        _voiceAIService = voiceAIService;
        _fileStorageService = fileStorageService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<VoiceProfileDto>> Handle(PreprocessAndCreateVoiceProfileCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling PreprocessAndCreateVoiceProfileCommand for MemberId: {MemberId}", command.MemberId);

        // 1. Call Python Voice AI Service for preprocessing
        var preprocessRequest = new VoicePreprocessRequest
        {
            AudioUrls = command.RawAudioUrls
        };

        var preprocessResult = await _voiceAIService.PreprocessVoiceAsync(preprocessRequest);

        if (!preprocessResult.IsSuccess)
        {
            _logger.LogError("Preprocessing failed: {Error}", preprocessResult.Error ?? preprocessResult.ValidationErrors?.FirstOrDefault().Value?.FirstOrDefault());
            return Result<VoiceProfileDto>.Failure(preprocessResult.Error ?? "Preprocessing failed due to validation errors.");
        }

        var preprocessedAudioUrl = preprocessResult.Value!.ProcessedAudioUrl;
        var durationSeconds = preprocessResult.Value!.Duration;
        var qualityReport = preprocessResult.Value!.QualityReport;

        _logger.LogInformation("Preprocessing successful. Preprocessed Audio URL: {Url}, Duration: {Duration}s, Quality: {OverallQuality}", preprocessedAudioUrl, durationSeconds, qualityReport.OverallQuality);

        // 2. Download the preprocessed audio from the Python service's temporary URL
        Stream audioStream;
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            audioStream = await httpClient.GetStreamAsync(preprocessedAudioUrl, cancellationToken);
            _logger.LogInformation("Downloaded preprocessed audio from {Url}", preprocessedAudioUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to download preprocessed audio from {Url}", preprocessedAudioUrl);
            return Result<VoiceProfileDto>.Failure($"Failed to download preprocessed audio: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while downloading preprocessed audio from {Url}", preprocessedAudioUrl);
            return Result<VoiceProfileDto>.Failure($"An unexpected error occurred during audio download: {ex.Message}");
        }

        // 3. Upload the downloaded audio to the C# backend's permanent storage
        // Assuming a folder structure like "family-voices/{memberId}/"
        var folder = $"family-voices/{command.MemberId}";
        var fileName = $"{Guid.NewGuid()}.wav";

        Result<FileStorageResultDto> uploadResult;
        await using (audioStream)
        {
            uploadResult = await _fileStorageService.UploadFileAsync(audioStream, fileName, folder, cancellationToken);
        }

        if (!uploadResult.IsSuccess)
        {
            _logger.LogError("Failed to upload preprocessed audio to permanent storage: {Error}", uploadResult.Error ?? uploadResult.ValidationErrors?.FirstOrDefault().Value?.FirstOrDefault());
            return Result<VoiceProfileDto>.Failure(uploadResult.Error ?? "Failed to upload audio due to validation errors.");
        }

        var permanentAudioUrl = uploadResult.Value!.FileUrl;
        _logger.LogInformation("Uploaded preprocessed audio to permanent storage. URL: {Url}", permanentAudioUrl);

        // 4. Create a new VoiceProfile using the CreateVoiceProfileCommand
        var createVoiceProfileCommand = new CreateVoiceProfileCommand
        {
            MemberId = command.MemberId,
            Label = command.Label,
            AudioUrl = permanentAudioUrl,
            DurationSeconds = durationSeconds,
            QualityScore = qualityReport.QualityScore,
            OverallQuality = qualityReport.OverallQuality,
            QualityMessages = JsonSerializer.Serialize(qualityReport.Messages),
            Language = command.Language,
            Consent = command.Consent
        };

        return await _mediator.Send(createVoiceProfileCommand, cancellationToken);
    }
}
