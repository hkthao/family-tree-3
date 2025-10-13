namespace backend.Application.AI.Commands.EmbedChunks;

public class EmbedChunksCommandValidator : AbstractValidator<EmbedChunksCommand>
{
    public EmbedChunksCommandValidator()
    {
        RuleFor(v => v.Chunks)
            .NotNull().WithMessage("Chunks cannot be null.")
            .NotEmpty().WithMessage("Chunks cannot be empty.");

        RuleFor(v => v.ProviderName)
            .NotNull().WithMessage("ProviderName cannot be null.")
            .NotEmpty().WithMessage("ProviderName cannot be empty.");
    }
}
