using AutoMapper;
using backend.Domain.Entities;

namespace backend.Application.Identity.Queries.GetUserByUsernameOrEmail;

/// <summary>
/// DTO chứa thông tin người dùng cần thiết cho việc tra cứu.
/// </summary>
public class UserLookupDto
{
    /// <summary>
    /// ID của người dùng.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tên đầy đủ của người dùng.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Địa chỉ email của người dùng.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// URL hình đại diện của người dùng (có thể null).
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Cấu hình ánh xạ giữa User và UserLookupDto.
    /// </summary>
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserLookupDto>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Profile != null ? s.Profile.Name : string.Empty))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => s.Profile != null ? s.Profile.Avatar : null));
        }
    }
}
