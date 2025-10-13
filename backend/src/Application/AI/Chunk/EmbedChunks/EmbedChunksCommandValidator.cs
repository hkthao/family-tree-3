namespace backend.Application.AI.Chunk.EmbedChunks;

public class EmbedChunksCommandValidator : AbstractValidator<EmbedChunksCommand>
{
    public EmbedChunksCommandValidator()
    {
        RuleFor(v => v.Chunks)
            .NotNull().WithMessage("Chunks cannot be null.")
            .NotEmpty().WithMessage("Chunks cannot be empty.");
    }
}
