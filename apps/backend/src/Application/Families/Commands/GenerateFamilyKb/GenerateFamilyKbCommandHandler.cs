using System.Text;
using backend.Application.AI.DTOs.Embeddings;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums; // For Domain Entities
using Microsoft.Extensions.Logging;

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

        public Task<Result<string>> Handle(GenerateFamilyKbCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Embedding generation is temporarily disabled.");
            return Task.FromResult(Result<string>.Success("Embedding generation is temporarily disabled."));
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
            if (!string.IsNullOrEmpty(member.Occupation))
            {
                textBuilder.AppendLine($"Nghề nghiệp: {member.Occupation}");
            }

            var detailedRelationshipText = new StringBuilder();
            var metadataRelationships = new List<string>();

            if (!string.IsNullOrEmpty(member.FatherFullName))
            {
                detailedRelationshipText.AppendLine($"Cha: {member.FatherFullName}.");
                metadataRelationships.Add($"Cha: {member.FatherFullName}");
            }
            if (!string.IsNullOrEmpty(member.MotherFullName))
            {
                detailedRelationshipText.AppendLine($"Mẹ: {member.MotherFullName}.");
                metadataRelationships.Add($"Mẹ: {member.MotherFullName}");
            }

            // Handle spouse based on gender
            if (member.Gender == "Nam" && !string.IsNullOrEmpty(member.WifeFullName)) // Assuming "Nam" is male
            {
                detailedRelationshipText.AppendLine($"Vợ: {member.WifeFullName}.");
                metadataRelationships.Add($"Vợ: {member.WifeFullName}");
            }
            else if (member.Gender == "Nữ" && !string.IsNullOrEmpty(member.HusbandFullName)) // Assuming "Nữ" is female
            {
                detailedRelationshipText.AppendLine($"Chồng: {member.HusbandFullName}.");
                metadataRelationships.Add($"Chồng: {member.HusbandFullName}");
            }
            // If the member has other types of spouse relationships not covered by HusbandFullName/WifeFullName,
            // they would not be explicitly listed here with "Vợ:" or "Chồng:".
            // For children, query members where this member is their FatherId or MotherId
            var childrenData = await _context.Members
                                              .Where(m => m.FamilyId == familyGuid && (m.FatherId == memberGuid || m.MotherId == memberGuid))
                                              .Select(m => new { m.FullName, m.Order }) // Select FullName and Order
                                              .OrderBy(m => m.Order) // Sort by Order
                                              .AsNoTracking()
                                              .ToListAsync(cancellationToken);

            var childrenDescriptions = new List<string>();
            foreach (var child in childrenData)
            {
                if (child.Order.HasValue)
                {
                    childrenDescriptions.Add($"Con thứ {child.Order}: {child.FullName}");
                }
                else
                {
                    childrenDescriptions.Add(child.FullName); // Fallback if Order is not set
                }
            }

            if (childrenDescriptions.Any())
            {
                detailedRelationshipText.AppendLine($"Con cái: {string.Join(", ", childrenDescriptions)}.");
                metadataRelationships.Add($"Con cái: {string.Join(", ", childrenDescriptions)}");
            }

            textBuilder.AppendLine($"Quan hệ: {detailedRelationshipText.ToString().Trim()}");
            textBuilder.AppendLine($"Tiểu sử tóm tắt: {member.Biography}");

            // Fetch significant events for the member
            var memberEvents = await _context.Events
                                             .Include(e => e.EventMembers)
                                             .Where(e => e.FamilyId == familyGuid && e.EventMembers.Any(em => em.MemberId == memberGuid))
                                             .AsNoTracking()
                                             .ToListAsync(cancellationToken);

            var formattedEvents = new List<string>();
            foreach (var evt in memberEvents)
            {
                var eventDetails = new StringBuilder();
                eventDetails.Append($"Tên sự kiện: {evt.Name}");

                string eventDateDescription = string.Empty;
                if (evt.CalendarType == CalendarType.Solar && evt.SolarDate.HasValue)
                {
                    eventDateDescription = evt.SolarDate.Value.ToShortDateString();
                }
                else if (evt.CalendarType == CalendarType.Lunar && evt.LunarDate != null)
                {
                    eventDateDescription = $"{evt.LunarDate.Day}/{@evt.LunarDate.Month}{(@evt.LunarDate.IsLeapMonth ? " (nhuận)" : "")}";
                }
                if (!string.IsNullOrEmpty(eventDateDescription))
                {
                    eventDetails.Append($", Thời gian: {eventDateDescription}");
                }
                if (evt.RepeatRule == RepeatRule.Yearly)
                {
                    eventDetails.Append(" (lặp hàng năm)");
                }

                if (!string.IsNullOrEmpty(evt.Description))
                {
                    eventDetails.Append($", Mô tả: {evt.Description}");
                }
                formattedEvents.Add(eventDetails.ToString());
            }

            textBuilder.AppendLine($"Sự kiện quan trọng: {string.Join("; ", formattedEvents)}");

            return new MemberEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = memberId,
                Text = textBuilder.ToString(),
                Metadata = new MemberMetadataDto
                {
                    FullName = member.FullName,
                    Gender = member.Gender ?? string.Empty,
                    BirthDate = member.DateOfBirth?.ToShortDateString() ?? "",
                    DeathDate = member.DateOfDeath?.ToShortDateString() ?? "",
                    Bio = member.Biography ?? "",
                    Relationships = metadataRelationships, // Use the new detailed list
                    Events = formattedEvents, // Populate with detailed event strings
                    Occupation = member.Occupation // Populate new Occupation property
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



            // Fetch Manager Name
            var managerName = "Không rõ";
            var managerProfile = await _context.FamilyUsers
                                               .Where(fu => fu.FamilyId == familyGuid && fu.Role == FamilyRole.Manager)
                                               .Join(_context.UserProfiles, // Join with UserProfiles
                                                     fu => fu.UserId,
                                                     up => up.UserId,
                                                     (fu, up) => up.Name) // Select the Name from UserProfile
                                               .FirstOrDefaultAsync(cancellationToken);
            if (managerProfile != null)
            {
                managerName = managerProfile;
            }

            // Calculate Statistics for Members
            var totalMembers = members.Count;
            var totalMales = members.Count(m => m.Gender == "Nam");
            var totalFemales = members.Count(m => m.Gender == "Nữ");
            var livingMembersCount = members.Count(m => !m.IsDeceased);
            var deceasedMembersCount = members.Count(m => m.IsDeceased);

            // Calculate Average Age
            double averageAge = 0;
            var membersWithBirthDate = members.Where(m => m.DateOfBirth.HasValue && !m.IsDeceased).ToList();
            if (membersWithBirthDate.Any())
            {
                averageAge = membersWithBirthDate.Average(m => (DateTime.Now.Year - m.DateOfBirth!.Value.Year) - (DateTime.Now.DayOfYear < m.DateOfBirth.Value.DayOfYear ? 1 : 0));
            }


            var textBuilder = new StringBuilder();
            textBuilder.AppendLine($"Tên gia đình: {family.Name}");
            textBuilder.AppendLine($"Mô tả chung: {family.Description ?? "Không có mô tả."}");
            textBuilder.AppendLine($"Tổng số thành viên: {totalMembers}");
            textBuilder.AppendLine($"Tổng số thế hệ: {family.TotalGenerations}");
            textBuilder.AppendLine($"Người quản lý: {managerName}");
            textBuilder.AppendLine($"Tuổi trung bình thành viên còn sống: {averageAge:F1}");
            textBuilder.AppendLine($"Số lượng thành viên nam: {totalMales}");
            textBuilder.AppendLine($"Số lượng thành viên nữ: {totalFemales}");
            textBuilder.AppendLine($"Số người còn sống: {livingMembersCount}");
            textBuilder.AppendLine($"Số người đã mất: {deceasedMembersCount}");

            return new FamilyEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = family.Id.ToString(), // Convert Guid to string for RecordId
                Text = textBuilder.ToString(),
                Metadata = new FamilyOverviewMetadataDto
                {
                    FamilyName = family.Name,
                    Origin = family.Address ?? "", // Assuming Address can be a proxy for origin
                    MemberCount = totalMembers, // Updated
                    TotalGenerations = family.TotalGenerations, // Updated
                    Description = family.Description ?? string.Empty, // Updated
                    ManagerName = managerName, // Updated
                    AverageAge = averageAge, // Updated
                    TotalMales = totalMales, // Updated
                    TotalFemales = totalFemales, // Updated
                    LivingMembersCount = livingMembersCount, // Updated
                    DeceasedMembersCount = deceasedMembersCount // Updated
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

            string eventDateDescription = string.Empty;
            if (@event.CalendarType == CalendarType.Solar && @event.SolarDate.HasValue)
            {
                eventDateDescription = @event.SolarDate.Value.ToShortDateString();
            }
            else if (@event.CalendarType == CalendarType.Lunar && @event.LunarDate != null)
            {
                eventDateDescription = $"{@event.LunarDate.Day}/{@event.LunarDate.Month}{(@event.LunarDate.IsLeapMonth ? " (nhuận)" : "")}";
            }
            if (!string.IsNullOrEmpty(eventDateDescription))
            {
                textBuilder.AppendLine($"Thời gian: {eventDateDescription}");
            }
            if (@event.RepeatRule == RepeatRule.Yearly)
            {
                textBuilder.AppendLine("Lặp lại hàng năm.");
            }

            textBuilder.AppendLine($"Ai tham gia: {string.Join(", ", relatedMembers.Select(m => m.FullName))}");
            textBuilder.AppendLine($"Ý nghĩa: [TODO: Thêm logic để tóm tắt ý nghĩa sự kiện]");
            textBuilder.AppendLine($"Chi tiết mô tả: {@event.Description}");

            // The EventMetadataDto needs to be updated to reflect the new properties as well.
            // For now, I will create a simplified version for Metadata
            return new EventEmbeddingsDto
            {
                FamilyId = familyId,
                RecordId = @event.Id.ToString(), // Convert Guid to string for RecordId
                Text = textBuilder.ToString(),
                Metadata = new EventMetadataDto
                {
                    EventType = @event.Type.ToString(),
                    Date = eventDateDescription, // Use the generated dateDescription
                    Location = "", // Location is removed, can be derived from Description if needed
                    MembersInvolved = relatedMembers.Select(m => m.FullName).ToList()
                }
            };
        }


    }
}
