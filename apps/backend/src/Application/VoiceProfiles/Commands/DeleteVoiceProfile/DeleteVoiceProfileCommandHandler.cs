using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.VoiceProfiles.Commands.DeleteVoiceProfile;

public class DeleteVoiceProfileCommandHandler : IRequestHandler<DeleteVoiceProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteVoiceProfileCommandHandler> _logger;

    public DeleteVoiceProfileCommandHandler(IApplicationDbContext context, ILogger<DeleteVoiceProfileCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteVoiceProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteVoiceProfileCommand for Id: {VoiceProfileId}", request.Id);

        var entity = await _context.VoiceProfiles
            .Where(vp => vp.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("VoiceProfile with Id {VoiceProfileId} not found.", request.Id);
            return Result.NotFound($"VoiceProfile with Id {request.Id} not found.");
        }

        _context.VoiceProfiles.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("VoiceProfile with Id {VoiceProfileId} successfully deleted.", request.Id);
        return Result.Success();
    }
}
