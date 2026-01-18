using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Commands.FixFamilyRelationships;

public class FixFamilyRelationshipsCommandHandler : IRequestHandler<FixFamilyRelationshipsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FixFamilyRelationshipsCommandHandler> _logger;

    public FixFamilyRelationshipsCommandHandler(IApplicationDbContext context, ILogger<FixFamilyRelationshipsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(FixFamilyRelationshipsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu xử lý lệnh FixFamilyRelationshipsCommand cho FamilyId: {FamilyId}", request.FamilyId);

        var members = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);

        if (!members.Any())
        {
            _logger.LogInformation("Không tìm thấy thành viên nào cho FamilyId: {FamilyId}. Không cần sửa lỗi.", request.FamilyId);
            return Result.Success();
        }

        var membersMap = members.ToDictionary(m => m.Id);
        int relationshipsFixedCount = 0;

        foreach (var member in members)
        {
            if (member.FatherId.HasValue || member.MotherId.HasValue)
            {
                // Lấy thông tin cha và mẹ hiện tại nếu có ID
                Member? currentFather = member.FatherId.HasValue && membersMap.TryGetValue(member.FatherId.Value, out var f) ? f : null;
                Member? currentMother = member.MotherId.HasValue && membersMap.TryGetValue(member.MotherId.Value, out var m) ? m : null;

                // Xác định giới tính hiệu quả
                string? currentFatherGender = currentFather?.Gender?.ToLowerInvariant();
                string? currentMotherGender = currentMother?.Gender?.ToLowerInvariant();

                bool fatherIsFemale = currentFather != null && currentFatherGender == "female";
                bool motherIsMale = currentMother != null && currentMotherGender == "male";

                // Kịch bản 1: Cha được gán là nữ VÀ Mẹ được gán là nam (trường hợp hoán đổi trực tiếp)
                if (fatherIsFemale && motherIsMale)
                {
                    _logger.LogWarning(
                        "Phát hiện lỗi quan hệ cho thành viên {MemberId} ({MemberFullName}): Cha ({FatherId}) được gán là nữ, Mẹ ({MotherId}) được gán là nam. Đang hoán đổi vị trí.",
                        member.Id, member.FullName, member.FatherId, member.MotherId);

                    // Thực hiện hoán đổi
                    Guid? tempId = member.FatherId;
                    string? tempFullName = member.FatherFullName;
                    string? tempAvatarUrl = member.FatherAvatarUrl;

                    member.FatherId = member.MotherId;
                    member.FatherFullName = member.MotherFullName;
                    member.FatherAvatarUrl = member.MotherAvatarUrl;

                    member.MotherId = tempId;
                    member.MotherFullName = tempFullName;
                    member.MotherAvatarUrl = tempAvatarUrl;

                    relationshipsFixedCount++;
                }
                // Kịch bản 2: Chỉ cha được gán là nữ (và mẹ không phải là nam hoặc không tồn tại)
                // Điều này có nghĩa là người được gán là cha thực ra phải là mẹ.
                else if (fatherIsFemale && (currentMother == null || currentMotherGender != "male"))
                {
                    _logger.LogWarning(
                        "Phát hiện lỗi quan hệ cho thành viên {MemberId} ({MemberFullName}): Cha ({FatherId}) được gán là nữ. Chuyển thành Mẹ.",
                        member.Id, member.FullName, member.FatherId);

                    member.MotherId = member.FatherId;
                    member.MotherFullName = member.FatherFullName;
                    member.MotherAvatarUrl = member.FatherAvatarUrl;

                    member.FatherId = null;
                    member.FatherFullName = null;
                    member.FatherAvatarUrl = null;

                    relationshipsFixedCount++;
                }
                // Kịch bản 3: Chỉ mẹ được gán là nam (và cha không phải là nữ hoặc không tồn tại)
                // Điều này có nghĩa là người được gán là mẹ thực ra phải là cha.
                else if (motherIsMale && (currentFather == null || currentFatherGender != "female"))
                {
                    _logger.LogWarning(
                        "Phát hiện lỗi quan hệ cho thành viên {MemberId} ({MemberFullName}): Mẹ ({MotherId}) được gán là nam. Chuyển thành Cha.",
                        member.Id, member.FullName, member.MotherId);

                    member.FatherId = member.MotherId;
                    member.FatherFullName = member.MotherFullName;
                    member.FatherAvatarUrl = member.MotherAvatarUrl;

                    member.MotherId = null;
                    member.MotherFullName = null;
                    member.MotherAvatarUrl = null;

                    relationshipsFixedCount++;
                }
            }
        }


        foreach (var member in members)
        {
            // NEW LOGIC START: Deduce missing parent based on spouse information

            // Case 1: Member has a MotherId but no FatherId
            if (member.MotherId.HasValue && !member.FatherId.HasValue)
            {
                Member? mother = null;
                if (membersMap.TryGetValue(member.MotherId.Value, out mother) && mother.HusbandId.HasValue)
                {
                    Member? inferredFather = null;
                    if (membersMap.TryGetValue(mother.HusbandId.Value, out inferredFather))
                    {
                        _logger.LogInformation(
                            "Suy luận Father cho thành viên {MemberId} ({MemberFullName}): Mother ({MotherId}) có HusbandId ({HusbandId}). Gán FatherId thành {InferredFatherId}.",
                            member.Id, member.FullName, mother.Id, mother.HusbandId.Value, inferredFather.Id);

                        member.FatherId = inferredFather.Id;
                        member.FatherFullName = inferredFather.FullName;
                        member.FatherAvatarUrl = inferredFather.AvatarUrl;
                        relationshipsFixedCount++;
                    }
                }
            }

            // Case 2: Member has a FatherId but no MotherId
            if (member.FatherId.HasValue && !member.MotherId.HasValue)
            {
                Member? father = null;
                if (membersMap.TryGetValue(member.FatherId.Value, out father) && father.WifeId.HasValue)
                {
                    Member? inferredMother = null;
                    if (membersMap.TryGetValue(father.WifeId.Value, out inferredMother))
                    {
                        _logger.LogInformation(
                            "Suy luận Mother cho thành viên {MemberId} ({MemberFullName}): Father ({FatherId}) có WifeId ({WifeId}). Gán MotherId thành {InferredMotherId}.",
                            member.Id, member.FullName, father.Id, father.WifeId.Value, inferredMother.Id);

                        member.MotherId = inferredMother.Id;
                        member.MotherFullName = inferredMother.FullName;
                        member.MotherAvatarUrl = inferredMother.AvatarUrl;
                        relationshipsFixedCount++;
                    }
                }
            }
            // NEW LOGIC END
        }

        if (relationshipsFixedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Hoàn tất FixFamilyRelationshipsCommand cho FamilyId: {FamilyId}. Đã sửa lỗi {Count} quan hệ.", request.FamilyId, relationshipsFixedCount);
        }
        else
        {
            _logger.LogInformation("Hoàn tất FixFamilyRelationshipsCommand cho FamilyId: {FamilyId}. Không có lỗi quan hệ nào được tìm thấy và sửa lỗi.", request.FamilyId);
        }

        return Result.Success();
    }
}
