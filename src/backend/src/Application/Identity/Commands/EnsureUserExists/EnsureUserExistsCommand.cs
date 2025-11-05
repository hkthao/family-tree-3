namespace backend.Application.Identity.Commands.EnsureUserExists;

/// <summary>
/// DTO chứa kết quả trả về từ command
/// </summary>
public class EnsureUserExistsResult
{
    public Guid UserId { get; set; }
    public Guid? ProfileId { get; set; }
}

/// <summary>
/// Command để đảm bảo người dùng tồn tại trong DB
/// </summary>
public class EnsureUserExistsCommand : IRequest<EnsureUserExistsResult>
{
    public string? ExternalId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
}
