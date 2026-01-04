using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Voice.DTOs; // Added for VoiceGenerateRequest and VoiceGenerateResponse
using backend.Application.VoiceProfiles.Queries;
using backend.Domain.Entities;

namespace backend.Application.VoiceProfiles.Commands.GenerateVoice;

/// <summary>
/// Handler để xử lý GenerateVoiceCommand.
/// </summary>
public class GenerateVoiceCommandHandler : IRequestHandler<GenerateVoiceCommand, Result<VoiceGenerationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IVoiceAIService _voiceAIService; // Injected IVoiceAIService

    public GenerateVoiceCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IVoiceAIService voiceAIService) // Constructor updated
    {
        _context = context;
        _mapper = mapper;
        _voiceAIService = voiceAIService; // Assign injected service
    }

    public async Task<Result<VoiceGenerationDto>> Handle(GenerateVoiceCommand request, CancellationToken cancellationToken)
    {
        var voiceProfile = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.VoiceProfileId)
            .FirstOrDefaultAsync(cancellationToken);

        if (voiceProfile == null)
        {
            return Result<VoiceGenerationDto>.Failure("Voice Profile not found.");
        }

        // Rule 1: Không generate voice nếu consent = false
        if (!voiceProfile.Consent)
        {
            return Result<VoiceGenerationDto>.Failure("Không thể tạo giọng nói. Thành viên chưa đồng ý sử dụng hồ sơ giọng nói này.");
        }

        // Call Python Voice AI Service to generate audio
        var generateRequest = new VoiceGenerateRequest
        {
            SpeakerWavUrl = voiceProfile.AudioUrl, // Use the stored audio URL from the voice profile
            Text = request.Text,
            Language = voiceProfile.Language // Use the language from the voice profile
        };

        var generateResult = await _voiceAIService.GenerateVoiceAsync(generateRequest);

        if (!generateResult.IsSuccess)
        {
            return Result<VoiceGenerationDto>.Failure($"Failed to generate voice: {generateResult.Error}");
        }

        var generatedAudioUrl = generateResult.Value!.AudioUrl;
        // The Python service does not return duration for generated audio, so we'll estimate it.
        // This estimation might need refinement or could be retrieved from a different service if available.
        var generatedDuration = request.Text.Length * 0.1; // Placeholder: 0.1 seconds per character

        var entity = new VoiceGeneration(
            request.VoiceProfileId,
            request.Text,
            generatedAudioUrl,
            generatedDuration
        );

        _context.VoiceGenerations.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<VoiceGenerationDto>.Success(_mapper.Map<VoiceGenerationDto>(entity));
    }
}
