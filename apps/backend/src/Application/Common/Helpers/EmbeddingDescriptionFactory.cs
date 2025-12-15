using backend.Domain.Entities;
using backend.Domain.Enums; // Add this
using backend.Domain.ValueObjects; // Add this

namespace backend.Application.Common.Helpers;

public static class EmbeddingDescriptionFactory
{
    public static (object EntityData, string Description) CreateFamilyData(Family family)
    {
        var description = $"Tên gia đình: {family.Name}. Mô tả: {family.Description}. Địa chỉ: {family.Address}. Tổng số thành viên: {family.TotalMembers}. Tổng số thế hệ: {family.TotalGenerations}.";
        var entityData = new
        {
            family.Id,
            family.Name,
            family.Description,
            family.Address,
            family.Visibility,
            family.TotalMembers,
            family.TotalGenerations
        };
        return (entityData, description);
    }

    public static (object EntityData, string Description) CreateMemberData(Member member)
    {
        var description = $"Tên thành viên: {member.FullName}. Tiểu sử: {member.Biography}. Nghề nghiệp: {member.Occupation}. Nơi sinh: {member.PlaceOfBirth}. Giới tính: {member.Gender}. Ngày sinh: {member.DateOfBirth?.ToShortDateString()}. Ngày mất: {member.DateOfDeath?.ToShortDateString()}. Thời gian sống: {(member.DateOfBirth != null && member.DateOfDeath != null ? (member.DateOfDeath.Value - member.DateOfBirth.Value).TotalDays / 365.25 : null)} năm.";
        var entityData = new
        {
            member.Id,
            member.FamilyId,
            member.FirstName,
            member.LastName,
            member.FullName,
            member.Nickname,
            member.Gender,
            member.DateOfBirth,
            member.DateOfDeath,
            member.PlaceOfBirth,
            member.PlaceOfDeath,
            member.Occupation,
            member.Biography
        };
        return (entityData, description);
    }

    public static (object EntityData, string Description) CreateRelationshipData(Relationship relationship)
    {
        var description = $"Loại quan hệ: {relationship.Type}. ID thành viên nguồn: {relationship.SourceMemberId}. ID thành viên đích: {relationship.TargetMemberId}. ID gia đình: {relationship.FamilyId}.";
        var entityData = new
        {
            relationship.Id,
            relationship.FamilyId,
            relationship.SourceMemberId,
            relationship.TargetMemberId,
            relationship.Type,
            relationship.Order
        };
        return (entityData, description);
    }

    public static (object EntityData, string Description) CreateEventData(Event @event)
    {
        string dateDescription = string.Empty;
        if (@event.CalendarType == CalendarType.Solar && @event.SolarDate.HasValue)
        {
            dateDescription = $"Ngày dương: {@event.SolarDate.Value.ToShortDateString()}";
        }
        else if (@event.CalendarType == CalendarType.Lunar && @event.LunarDate != null)
        {
            dateDescription = $"Ngày âm: {@event.LunarDate.Day}/{@event.LunarDate.Month}{(@event.LunarDate.IsLeapMonth ? " (nhuận)" : "")}";
        }

        string repeatRuleDescription = @event.RepeatRule == RepeatRule.Yearly ? " (lặp hàng năm)" : "";

        var description = $"Tên sự kiện: {@event.Name}. Mô tả: {@event.Description}. {dateDescription}{repeatRuleDescription}. Loại: {@event.Type}. ID gia đình: {@event.FamilyId}.";
        var entityData = new
        {
            @event.Id,
            @event.FamilyId,
            @event.Name,
            @event.Description,
            @event.CalendarType,
            @event.SolarDate,
            @event.LunarDate,
            @event.RepeatRule,
            @event.Type,
            @event.Color
        };
        return (entityData, description);
    }
}
