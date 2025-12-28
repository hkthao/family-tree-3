using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using backend.Application.Prompts.DTOs;
using AutoMapper; // Added

namespace backend.Application.Prompts.Queries.ExportPrompts;

public class ExportPromptsQueryHandler : IRequestHandler<ExportPromptsQuery, Result<List<PromptDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ExportPromptsQueryHandler> _logger;
    private readonly IMapper _mapper; // Added

    public ExportPromptsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ExportPromptsQueryHandler> logger, IMapper mapper) // Added IMapper
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper; // Assigned
    }

    public async Task<Result<List<PromptDto>>> Handle(ExportPromptsQuery request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin mới có thể xuất prompts
        if (!_authorizationService.IsAdmin())
        {
            _logger.LogWarning("Người dùng không có quyền cố gắng xuất prompts.");
            return Result<List<PromptDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var prompts = await _context.Prompts
            .AsNoTracking()
            .ProjectTo<PromptDto>(_mapper.ConfigurationProvider) // Use AutoMapper
            .ToListAsync(cancellationToken);

        return Result<List<PromptDto>>.Success(prompts);
    }
}
