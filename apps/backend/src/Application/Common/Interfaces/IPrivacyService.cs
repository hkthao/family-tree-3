using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers; // For MemberListDto
using backend.Application.Events.Queries; // For EventDto
using backend.Application.Events.Queries.GetEventById; // For EventDetailDto
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Families.Queries.GetFamilyById; // For FamilyDetailDto
using backend.Application.FamilyLocations; // For FamilyLocationDto
using backend.Application.MemoryItems.DTOs; // For MemoryItemDto

namespace backend.Application.Common.Interfaces;

public interface IPrivacyService
{
    /// <summary>
    /// Lọc các thuộc tính của MemberDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberDto">MemberDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemberDto đã được lọc.</returns>
    Task<MemberDto> ApplyPrivacyFilter(MemberDto memberDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các MemberDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberDtos">Danh sách MemberDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà các thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách MemberDto đã được lọc.</returns>
    Task<List<MemberDto>> ApplyPrivacyFilter(List<MemberDto> memberDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của MemberDetailDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberDetailDto">MemberDetailDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemberDetailDto đã được lọc.</returns>
    Task<MemberDetailDto> ApplyPrivacyFilter(MemberDetailDto memberDetailDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của MemberListDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberListDto">MemberListDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemberListDto đã được lọc.</returns>
    Task<MemberListDto> ApplyPrivacyFilter(MemberListDto memberListDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các MemberListDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberListDtos">Danh sách MemberListDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà các thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách MemberListDto đã được lọc.</returns>
    Task<List<MemberListDto>> ApplyPrivacyFilter(List<MemberListDto> memberListDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của EventDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="eventDto">EventDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà sự kiện thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>EventDto đã được lọc.</returns>
    Task<EventDto> ApplyPrivacyFilter(EventDto eventDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các EventDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="eventDtos">Danh sách EventDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà các sự kiện thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách EventDto đã được lọc.</returns>
    Task<List<EventDto>> ApplyPrivacyFilter(List<EventDto> eventDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của EventDetailDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="eventDetailDto">EventDetailDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà sự kiện thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>EventDetailDto đã được lọc.</returns>
    Task<EventDetailDto> ApplyPrivacyFilter(EventDetailDto eventDetailDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của FamilyDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyDto">FamilyDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>FamilyDto đã được lọc.</returns>
    Task<FamilyDto> ApplyPrivacyFilter(FamilyDto familyDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các FamilyDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyDtos">Danh sách FamilyDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách FamilyDto đã được lọc.</returns>
    Task<List<FamilyDto>> ApplyPrivacyFilter(List<FamilyDto> familyDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của FamilyDetailDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyDetailDto">FamilyDetailDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>FamilyDetailDto đã được lọc.</returns>
    Task<FamilyDetailDto> ApplyPrivacyFilter(FamilyDetailDto familyDetailDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của FamilyLocationDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyLocationDto">FamilyLocationDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>FamilyLocationDto đã được lọc.</returns>
    Task<FamilyLocationDto> ApplyPrivacyFilter(FamilyLocationDto familyLocationDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các FamilyLocationDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyLocationDtos">Danh sách FamilyLocationDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách FamilyLocationDto đã được lọc.</returns>
    Task<List<FamilyLocationDto>> ApplyPrivacyFilter(List<FamilyLocationDto> familyLocationDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của MemoryItemDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memoryItemDto">MemoryItemDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemoryItemDto đã được lọc.</returns>
    Task<MemoryItemDto> ApplyPrivacyFilter(MemoryItemDto memoryItemDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các MemoryItemDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memoryItemDtos">Danh sách MemoryItemDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách MemoryItemDto đã được lọc.</returns>
    Task<List<MemoryItemDto>> ApplyPrivacyFilter(List<MemoryItemDto> memoryItemDtos, Guid familyId, CancellationToken cancellationToken);
}

public interface IPrivacyService
{
    /// <summary>
    /// Lọc các thuộc tính của MemberDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberDto">MemberDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemberDto đã được lọc.</returns>
    Task<MemberDto> ApplyPrivacyFilter(MemberDto memberDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các MemberDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberDtos">Danh sách MemberDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà các thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách MemberDto đã được lọc.</returns>
    Task<List<MemberDto>> ApplyPrivacyFilter(List<MemberDto> memberDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của MemberDetailDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberDetailDto">MemberDetailDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemberDetailDto đã được lọc.</returns>
    Task<MemberDetailDto> ApplyPrivacyFilter(MemberDetailDto memberDetailDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của MemberListDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberListDto">MemberListDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>MemberListDto đã được lọc.</returns>
    Task<MemberListDto> ApplyPrivacyFilter(MemberListDto memberListDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các MemberListDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="memberListDtos">Danh sách MemberListDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà các thành viên thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách MemberListDto đã được lọc.</returns>
    Task<List<MemberListDto>> ApplyPrivacyFilter(List<MemberListDto> memberListDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của EventDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="eventDto">EventDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà sự kiện thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>EventDto đã được lọc.</returns>
    Task<EventDto> ApplyPrivacyFilter(EventDto eventDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các EventDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="eventDtos">Danh sách EventDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà các sự kiện thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách EventDto đã được lọc.</returns>
    Task<List<EventDto>> ApplyPrivacyFilter(List<EventDto> eventDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của EventDetailDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="eventDetailDto">EventDetailDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình mà sự kiện thuộc về.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>EventDetailDto đã được lọc.</returns>
    Task<EventDetailDto> ApplyPrivacyFilter(EventDetailDto eventDetailDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của FamilyDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyDto">FamilyDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>FamilyDto đã được lọc.</returns>
    Task<FamilyDto> ApplyPrivacyFilter(FamilyDto familyDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các FamilyDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyDtos">Danh sách FamilyDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách FamilyDto đã được lọc.</returns>
    Task<List<FamilyDto>> ApplyPrivacyFilter(List<FamilyDto> familyDtos, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của FamilyDetailDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyDetailDto">FamilyDetailDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>FamilyDetailDto đã được lọc.</returns>
    Task<FamilyDetailDto> ApplyPrivacyFilter(FamilyDetailDto familyDetailDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc các thuộc tính của FamilyLocationDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyLocationDto">FamilyLocationDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>FamilyLocationDto đã được lọc.</returns>
    Task<FamilyLocationDto> ApplyPrivacyFilter(FamilyLocationDto familyLocationDto, Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Lọc danh sách các FamilyLocationDto dựa trên cấu hình quyền riêng tư của gia đình.
    /// Admin sẽ luôn thấy toàn bộ dữ liệu.
    /// </summary>
    /// <param name="familyLocationDtos">Danh sách FamilyLocationDto cần lọc.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách FamilyLocationDto đã được lọc.</returns>
    Task<List<FamilyLocationDto>> ApplyPrivacyFilter(List<FamilyLocationDto> familyLocationDtos, Guid familyId, CancellationToken cancellationToken);
}