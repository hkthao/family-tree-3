using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyMedias.Commands.StartImageRestoration;

public record StartImageRestorationCommand(string ImageUrl) : IRequest<Result<StartImageRestorationResponseDto>>;

public class StartImageRestorationCommandHandler(IImageRestorationService imageRestorationService) : IRequestHandler<StartImageRestorationCommand, Result<StartImageRestorationResponseDto>>
{
    private readonly IImageRestorationService _imageRestorationService = imageRestorationService;

    public async Task<Result<StartImageRestorationResponseDto>> Handle(StartImageRestorationCommand request, CancellationToken cancellationToken)
    {
        var result = await _imageRestorationService.StartRestorationAsync(request.ImageUrl);
        return result;
    }
}
