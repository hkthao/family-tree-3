using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.VoiceGenerations.Queries;
using backend.Domain.Entities;

namespace backend.Application.VoiceGenerations.Commands.GenerateVoice;

/// <summary>
/// Command để tạo audio từ một hồ sơ giọng nói và văn bản.
/// </summary>
public record GenerateVoiceCommand : IRequest<Result<VoiceGenerationDto>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói sẽ được sử dụng.
    /// </summary>
    public Guid VoiceProfileId { get; init; }

    /// <summary>
    /// Văn bản cần chuyển đổi thành giọng nói.
    /// </summary>
    public string Text { get; init; } = null!;
}

/// <summary>
/// Validator cho GenerateVoiceCommand.
/// </summary>
public class GenerateVoiceCommandValidator : AbstractValidator<GenerateVoiceCommand>
{
    private readonly IApplicationDbContext _context;

    public GenerateVoiceCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.VoiceProfileId)
            .NotEmpty().WithMessage("Voice Profile ID không được để trống.")
            .MustAsync(BeExistingVoiceProfile).WithMessage("Voice Profile không tồn tại.");

        RuleFor(v => v.Text)
            .NotEmpty().WithMessage("Văn bản không được để trống.")
            .MaximumLength(4000).WithMessage("Văn bản không được vượt quá 4000 ký tự.");
    }

    private async Task<bool> BeExistingVoiceProfile(Guid voiceProfileId, CancellationToken cancellationToken)
    {
        return await _context.VoiceProfiles.AnyAsync(vp => vp.Id == voiceProfileId, cancellationToken);
    }
}

/// <summary>
/// Handler để xử lý GenerateVoiceCommand.
/// </summary>
public class GenerateVoiceCommandHandler : IRequestHandler<GenerateVoiceCommand, Result<VoiceGenerationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GenerateVoiceCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

        // TODO: Call Python service to generate audio.
        // For now, we'll simulate the audio generation with a placeholder URL and duration.
        // The actual implementation will involve an HttpClient call to the Python service.
        var generatedAudioUrl = $"https://example.com/generated_audio/{Guid.NewGuid()}.wav";
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
