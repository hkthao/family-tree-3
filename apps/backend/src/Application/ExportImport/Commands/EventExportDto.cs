using backend.Domain.Enums;

namespace backend.Application.ExportImport.Commands;

public class EventExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    public CalendarType CalendarType { get; set; }
    public DateTime? SolarDate { get; set; }
    public LunarDateDto? LunarDate { get; set; } // Use LunarDateDto for export/import
    public RepeatRule RepeatRule { get; set; }

    public EventType Type { get; set; }
    public string? Color { get; set; }
    public List<Guid> RelatedMembers { get; set; } = [];
}
