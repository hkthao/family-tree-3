using backend.Application.Faces.Common.Interfaces;
using backend.Application.Common.Interfaces;

namespace backend.Application.Faces.Commands.DetectFaces;
public class DetectFacesCommandHandler : IRequestHandler<DetectFacesCommand, List<FaceDetectionResultDto>>
{
    private readonly IFaceDetectionService _faceDetectionService;
    private readonly IApplicationDbContext _context;

    public DetectFacesCommandHandler(IFaceDetectionService faceDetectionService, IApplicationDbContext context)
    {
        _faceDetectionService = faceDetectionService;
        _context = context;
    }

    public async Task<List<FaceDetectionResultDto>> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        var detectedFaces = await _faceDetectionService.DetectFacesAsync(request.ImageBytes, request.ContentType, request.ReturnCrop);

        return detectedFaces;
    }
}