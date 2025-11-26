using backend.Application.Common.Models;

namespace backend.Application.Families.ExportImport;

public class ImportFamilyCommand : IRequest<Result<Guid>>
{
    public Guid? FamilyId { get; set; }
    public FamilyExportDto FamilyData { get; set; } = null!;
    public bool ClearExistingData { get; set; } = true; // New option to clear existing data
}
