using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.GenerateFamilyKb
{
    // Enum to specify the type of AI Text Record to generate
    public enum KbRecordType
    {
        Member,
        Family,
        Event,
        Story
    }

    /// <summary>
    /// Command để tạo dữ liệu cho Knowledge Base (KB) của gia đình.
    /// </summary>
    public record GenerateFamilyKbCommand : IRequest<Result<string>>
    {
        public GenerateFamilyKbCommand(string familyId, string recordId, KbRecordType recordType)
        {
            FamilyId = familyId;
            RecordId = recordId;
            RecordType = recordType;
        }

        public string FamilyId { get; init; } = null!;
        public string RecordId { get; init; } = null!; // ID của entity đã kích hoạt event
        public KbRecordType RecordType { get; init; } // Loại bản ghi (Member, Family, Event, Story)
    }
}