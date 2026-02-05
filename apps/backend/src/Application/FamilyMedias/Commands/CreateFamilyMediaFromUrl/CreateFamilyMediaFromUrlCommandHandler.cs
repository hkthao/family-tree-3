using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Domain.Entities;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMediaFromUrl;

public class CreateFamilyMediaFromUrlCommandHandler : IRequestHandler<CreateFamilyMediaFromUrlCommand, Result<FamilyMediaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public CreateFamilyMediaFromUrlCommandHandler(
        IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<FamilyMediaDto>> Handle(CreateFamilyMediaFromUrlCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<FamilyMediaDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Validate URL again for good measure, although validator should catch most
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            // If URL is empty/null, and MediaType is also null, this should have been caught by validator.
            // If MediaType is provided, we can proceed.
            if (request.MediaType == null)
            {
                return Result<FamilyMediaDto>.Failure("Media Type or a valid URL is required.", ErrorSources.Validation);
            }
        }
        else if (!Uri.TryCreate(request.Url, UriKind.Absolute, out Uri? uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            return Result<FamilyMediaDto>.Failure("URL must be a valid absolute URL.", ErrorSources.Validation);
        }

        var familyMedia = new FamilyMedia
        {
            FamilyId = request.FamilyId,
            FileName = request.FileName,
            FilePath = request.Url, // Store the URL directly
            MediaType = request.MediaType ?? request.Url.InferMediaTypeFromUrl(), // Infer from URL using the new extension method
            FileSize = 0, // File size is 0 for URL-based media
            Description = request.Description,

        };

        _context.FamilyMedia.Add(familyMedia);
        await _context.SaveChangesAsync(cancellationToken);

        var familyMediaDto = _mapper.Map<FamilyMediaDto>(familyMedia);
        return Result<FamilyMediaDto>.Success(familyMediaDto);
    }
}
