namespace backend.Application.Common.Models;

public abstract class PaginationModel
{
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 10;
}
