using backend.Application.Users.Queries;

namespace backend.Application.Common.Models;

public class UsersSearchResultDto
{
    public List<UserDto> Items { get; set; } = new List<UserDto>();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
