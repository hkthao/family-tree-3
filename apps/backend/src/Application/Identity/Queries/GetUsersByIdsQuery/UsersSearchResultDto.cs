using backend.Application.Users.Queries;

namespace backend.Application.Common.Models;

public class UsersSearchResultDto
{
    public List<UserDto> Items { get; set; } = new List<UserDto>();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
