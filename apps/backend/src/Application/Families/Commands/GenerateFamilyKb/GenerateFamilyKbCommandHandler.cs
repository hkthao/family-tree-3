using backend.Application.AI.DTOs.Embeddings;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using backend.Domain.Entities;
using backend.Domain.Enums; // For Domain Entities

namespace backend.Application.Families.Commands.GenerateFamilyKb
{
    /// <summary>
    /// Xử lý command GenerateFamilyKbCommand.
    /// </summary>
    public class GenerateFamilyKbCommandHandler : IRequestHandler<GenerateFamilyKbCommand, Result<string>>
    {
        private readonly ILogger<GenerateFamilyKbCommandHandler> _logger;
        private readonly IN8nService _n8nService;
        private readonly IApplicationDbContext _context; // Injected IApplicationDbContext

        public GenerateFamilyKbCommandHandler(
            ILogger<GenerateFamilyKbCommandHandler> logger,
            IN8nService n8nService,
            IApplicationDbContext context) // Inject IApplicationDbContext
        {
            _logger = logger;
            _n8nService = n8nService;
            _context = context;
        }

        public async Task<Result<string>> Handle(GenerateFamilyKbCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating Family KB record for FamilyId: {FamilyId}, RecordId: {RecordId}, Type: {RecordType}",
                request.FamilyId, request.RecordId, request.RecordType);

            BaseEmbeddingsDto? embeddingsDto = null;
            switch (request.RecordType)
            {
                case KbRecordType.Member:
                    embeddingsDto = await GenerateMemberEmbeddingsDto(request.FamilyId, request.RecordId, cancellationToken);
                    break;
                case KbRecordType.Family:
                    embeddingsDto = await GenerateFamilyEmbeddingsDto(request.FamilyId, request.RecordId, cancellationToken);
                    break;
                case KbRecordType.Event:
                    embeddingsDto = await GenerateEventEmbeddingsDto(request.FamilyId, request.RecordId, cancellationToken);
                    break;
                case KbRecordType.Story:
                    embeddingsDto = await GenerateStoryEmbeddingsDto(request.FamilyId, request.RecordId, cancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unsupported KB Record Type: {RecordType}", request.RecordType);
                    return Result<string>.Failure($"Unsupported KB Record Type: {request.RecordType}");
            }

            if (embeddingsDto == null)
            {
                return Result<string>.Failure($"Could not generate embeddings DTO for {request.RecordType} with ID {request.RecordId}");
            }

            return await _n8nService.CallEmbeddingsWebhookAsync(embeddingsDto, cancellationToken);
        }

        private string GetVietnameseRelationshipType(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.Father => "cha",
                RelationshipType.Mother => "mẹ",
                RelationshipType.Husband => "chồng",
                RelationshipType.Wife => "vợ",
                _ => type.ToString()
            };
        }

        private async Task<MemberEmbeddingsDto?> GenerateMemberEmbeddingsDto(string familyId, string memberId, CancellationToken cancellationToken)
        {
            var memberGuid = Guid.Parse(memberId);
            var familyGuid = Guid.Parse(familyId);

            var member = await _context.Members
                                       .Include(m => m.SourceRelationships).ThenInclude(r => r.TargetMember) // Correct: Load the 'other' member in source relationships
                                       .Include(m => m.TargetRelationships).ThenInclude(r => r.SourceMember) // Correct: Load the 'other' member in target relationships
                                       .Where(m => m.Id == memberGuid && m.FamilyId == familyGuid)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(cancellationToken);

            if (member == null)
            {
                _logger.LogWarning("Member with ID {MemberId} not found in family {FamilyId}.", memberId, familyId);
                return null;
            }

            var relationships = new List<Relationship>();
            relationships.AddRange(member.SourceRelationships.Where(r => r.TargetMember != null && r.SourceMember != null)); // Filter out nulls
            relationships.AddRange(member.TargetRelationships.Where(r => r.SourceMember != null && r.TargetMember != null)); // Filter out nulls

            var textBuilder = new StringBuilder();
            textBuilder.AppendLine($"Tên: {member.FullName}");
            textBuilder.AppendLine($"Ngày sinh: {member.DateOfBirth?.ToShortDateString()}");
            textBuilder.AppendLine($"Ngày mất: {member.DateOfDeath?.ToShortDateString()}");
            textBuilder.AppendLine($"Giới tính: {member.Gender}");

            var relationshipText = new StringBuilder();
            foreach (var rel in relationships)
            {
                if (rel.SourceMemberId == member.Id)
                {
                    if (rel.TargetMember != null)
                    {
                        relationshipText.Append($"{GetVietnameseRelationshipType(rel.Type)} của {rel.TargetMember.FullName}; ");
                    }
                    else
                    {
                        relationshipText.Append($"{GetVietnameseRelationshipType(rel.Type)} của Unknown; ");
                    }
                }
                else if (rel.TargetMemberId == member.Id)
                {
                    if (rel.SourceMember != null)
                    {
                        relationshipText.Append($"{GetVietnameseRelationshipType(rel.Type)} với {rel.SourceMember.FullName}; ");
                    }
                    else
                    {
                        relationshipText.Append($"{GetVietnameseRelationshipType(rel.Type)} với Unknown; ");
                    }
                }
            }
            textBuilder.AppendLine($"Quan hệ: {relationshipText.ToString().TrimEnd(' ', ';')}");
            textBuilder.AppendLine($"Tiểu sử tóm tắt: {member.Biography}");
            // Placeholder for "Sự kiện quan trọng" and "Câu chuyện nổi bật" - requires fetching event/story data
            textBuilder.AppendLine("Sự kiện quan trọng: []"); // TODO: Implement fetching significant events
            textBuilder.AppendLine("Câu chuyện nổi bật: []"); // TODO: Implement fetching notable stories

            return new MemberEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = memberId,
                Text = textBuilder.ToString(),
                Metadata = new MemberMetadataDto
                {
                    FullName = member.FullName,
                    Gender = member.Gender??string.Empty,
                    BirthDate = member.DateOfBirth?.ToShortDateString() ?? "",
                    DeathDate = member.DateOfDeath?.ToShortDateString() ?? "",
                    Bio = member.Biography ?? "",
                    Relationships = relationships.Select(r =>
                    {
                        string relatedName = "Unknown";
                        if (r.SourceMemberId == member.Id && r.TargetMember != null)
                        {
                            relatedName = r.TargetMember.FullName;
                        }
                        else if (r.TargetMemberId == member.Id && r.SourceMember != null)
                        {
                            relatedName = r.SourceMember.FullName;
                        }
                        return $"{GetVietnameseRelationshipType(r.Type)} - {relatedName}";
                    }).ToList()
                }
            };
        }

        private async Task<FamilyEmbeddingsDto?> GenerateFamilyEmbeddingsDto(string familyId, string recordId, CancellationToken cancellationToken)
        {
            var familyGuid = Guid.Parse(familyId);

            var family = await _context.Families
                                       .Where(f => f.Id == familyGuid)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(cancellationToken);

            if (family == null)
            {
                _logger.LogWarning("Family with ID {FamilyId} not found.", familyId);
                return null;
            }

            var members = await _context.Members
                                        .Where(m => m.FamilyId == familyGuid)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken);

            var events = await _context.Events
                                       .Where(e => e.FamilyId == familyGuid)
                                       .AsNoTracking()
                                       .ToListAsync(cancellationToken);

            var stories = await _context.MemberStories
                                        .Include(s => s.Member) // Include Member to access FamilyId
                                        .Where(s => s.Member.FamilyId == familyGuid)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken);


            var textBuilder = new StringBuilder();
            textBuilder.AppendLine($"Tên gia đình: {family.Name}");
            textBuilder.AppendLine($"Tên các thành viên: {string.Join(", ", members.Select(m => m.FullName))}");
            // Sơ đồ quan hệ chính: requires more complex logic to extract key relationships
            textBuilder.AppendLine("Sơ đồ quan hệ chính: [TODO: Thêm logic để tóm tắt quan hệ chính]");
            textBuilder.AppendLine($"Sự kiện quan trọng của gia đình: {string.Join(", ", events.Select(e => e.Name))}");
            // Điểm nổi bật lịch sử: requires analysis
            textBuilder.AppendLine("Điểm nổi bật lịch sử: [TODO: Thêm logic để tóm tắt điểm nổi bật lịch]");
            textBuilder.AppendLine($"Câu chuyện truyền thống: {string.Join(", ", stories.Select(s => s.Title))}"); // Using story titles as a proxy

            return new FamilyEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = family.Id.ToString(), // Convert Guid to string for RecordId
                Text = textBuilder.ToString(),
                Metadata = new FamilyOverviewMetadataDto
                {
                    FamilyName = family.Name,
                    Origin = family.Address ?? "", // Assuming Address can be a proxy for origin
                    NotableMembers = members.OrderByDescending(m => m.DateOfBirth).Take(5).Select(m => m.FullName).ToList(), // Example: 5 oldest members
                    MemberCount = members.Count(),
                    EventCount = events.Count(),
                    StoryCount = stories.Count()
                }
            };
        }

        private async Task<EventEmbeddingsDto?> GenerateEventEmbeddingsDto(string familyId, string eventId, CancellationToken cancellationToken)
        {
            var eventGuid = Guid.Parse(eventId);
            var familyGuid = Guid.Parse(familyId);

            var @event = await _context.Events
                                      .Include(e => e.EventMembers).ThenInclude(em => em.Member) // Include related members
                                      .Where(e => e.Id == eventGuid && e.FamilyId == familyGuid)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(cancellationToken);

            if (@event == null)
            {
                _logger.LogWarning("Event with ID {EventId} not found in family {FamilyId}.", eventId, familyId);
                return null;
            }

            var relatedMembers = @event.EventMembers.Select(em => em.Member).Where(m => m != null).ToList();

            var textBuilder = new StringBuilder();
            textBuilder.AppendLine($"Tên sự kiện: {@event.Name}");
            textBuilder.AppendLine($"Ngày diễn ra: {@event.StartDate?.ToShortDateString()} - {@event.EndDate?.ToShortDateString()}");
            textBuilder.AppendLine($"Ai tham gia: {string.Join(", ", relatedMembers.Select(m => m.FullName))}");
            textBuilder.AppendLine($"Ý nghĩa: [TODO: Thêm logic để tóm tắt ý nghĩa sự kiện]");
            textBuilder.AppendLine($"Chi tiết mô tả: {@event.Description}");

            return new EventEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = @event.Id.ToString(), // Convert Guid to string for RecordId
                Text = textBuilder.ToString(),
                Metadata = new EventMetadataDto
                {
                    EventType = @event.Type.ToString(),
                    Date = @event.StartDate?.ToShortDateString() ?? "",
                    Location = @event.Location ?? "",
                    MembersInvolved = relatedMembers.Select(m => m.FullName).ToList()
                }
            };
        }

        private async Task<StoryEmbeddingsDto?> GenerateStoryEmbeddingsDto(string familyId, string storyId, CancellationToken cancellationToken)
        {
            var storyGuid = Guid.Parse(storyId);
            var familyGuid = Guid.Parse(familyId);

            var story = await _context.MemberStories
                                      .Include(s => s.Member) // Include the main member for the story
                                      .Where(s => s.Id == storyGuid && s.Member.FamilyId == familyGuid)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(cancellationToken);

            if (story == null)
            {
                _logger.LogWarning("Story with ID {StoryId} not found in family {FamilyId}.", storyId, familyId);
                return null;
            }

            var mainCharacter = story.Member;

            var textBuilder = new StringBuilder();
            textBuilder.AppendLine($"Tiêu đề câu chuyện: {story.Title}");
            textBuilder.AppendLine($"Nhân vật chính: {mainCharacter?.FullName ?? "Không rõ"}");
            textBuilder.AppendLine($"Bối cảnh: [TODO: Thêm logic để tóm tắt bối cảnh câu chuyện]");
            textBuilder.AppendLine($"Cốt truyện: {story.Story}");
            textBuilder.AppendLine($"Ý nghĩa và cảm xúc: [TODO: Thêm logic để tóm tắt ý nghĩa và cảm xúc]");

            return new StoryEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = story.Id.ToString(), // Convert Guid to string for RecordId
                Text = textBuilder.ToString(),
                Metadata = new StoryMetadataDto
                {
                    Title = story.Title,
                    Summary = story.Story, // Using Story property as summary
                    Characters = mainCharacter != null ? new List<string> { mainCharacter.FullName } : new List<string>() // Only main character for now
                }
            };
        }
    }
}
