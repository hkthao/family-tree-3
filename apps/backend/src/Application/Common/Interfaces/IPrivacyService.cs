using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers; // For MemberDetailDto
using backend.Application.Events.Queries; // For EventDto
using backend.Application.Events.Queries.GetEventById; // For EventDetailDto

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
}
