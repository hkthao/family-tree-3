using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.Import;

public record ImportFamilyCommand : IRequest<Result<Guid>>
{
    public Guid? FamilyId { get; set; }
    public FamilyImportDto FamilyData { get; set; } = new();
    public bool ClearExistingData { get; set; } = false;
}
