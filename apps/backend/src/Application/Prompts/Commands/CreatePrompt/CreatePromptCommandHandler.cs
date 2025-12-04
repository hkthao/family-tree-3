using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;

namespace backend.Application.Prompts.Commands.CreatePrompt;

public class CreatePromptCommandHandler : IRequestHandler<CreatePromptCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreatePromptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreatePromptCommand request, CancellationToken cancellationToken)
    {
        var entity = new Prompt
        {
            Code = request.Code,
            Title = request.Title,
            Content = request.Content,
            Description = request.Description
        };

        _context.Prompts.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
