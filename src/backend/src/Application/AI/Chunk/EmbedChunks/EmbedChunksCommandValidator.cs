namespace backend.Application.AI.Chunk.EmbedChunks;

/// <summary>
/// Trình xác thực cho EmbedChunksCommand.
/// </summary>
public class EmbedChunksCommandValidator : AbstractValidator<EmbedChunksCommand>
{
    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp EmbedChunksCommandValidator và định nghĩa các quy tắc xác thực.
    /// </summary>
    public EmbedChunksCommandValidator()
    {
        RuleFor(v => v.Chunks)
            .NotNull().WithMessage("Chunks cannot be null.")
            .NotEmpty().WithMessage("Chunks cannot be empty.");
    }
}
