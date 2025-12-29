using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using backend.Application.Prompts.DTOs;

namespace backend.Application.Prompts.Commands.ImportPrompts;

public class ImportPromptsCommandHandler : IRequestHandler<ImportPromptsCommand, Result<List<PromptDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ImportPromptsCommandHandler> _logger;
    private readonly IMapper _mapper;

    public ImportPromptsCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ImportPromptsCommandHandler> logger, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<PromptDto>>> Handle(ImportPromptsCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin mới có thể nhập prompts
        if (!_authorizationService.IsAdmin())
        {
            _logger.LogWarning("Người dùng không có quyền cố gắng nhập prompts.");
            return Result<List<PromptDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Handle empty import list explicitly
        if (request.Prompts == null || !request.Prompts.Any())
        {
            return Result<List<PromptDto>>.Success(new List<PromptDto>());
        }

        var importedPrompts = new List<Prompt>();

        foreach (var importPromptItemDto in request.Prompts)
        {
            // Kiểm tra xem prompt đã tồn tại theo Code chưa
            var existingPrompt = await _context.Prompts
                .FirstOrDefaultAsync(p => p.Code == importPromptItemDto.Code, cancellationToken);

            if (existingPrompt != null)
            {
                _logger.LogInformation("Prompt với Code '{PromptCode}' đã tồn tại. Bỏ qua nhập.", importPromptItemDto.Code);
                continue; 
            }

            var newPrompt = _mapper.Map<Prompt>(importPromptItemDto); // Use AutoMapper
            _context.Prompts.Add(newPrompt);
            importedPrompts.Add(newPrompt);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var importedPromptDtos = _mapper.Map<List<PromptDto>>(importedPrompts); // Use AutoMapper

        return Result<List<PromptDto>>.Success(importedPromptDtos);
    }
}
