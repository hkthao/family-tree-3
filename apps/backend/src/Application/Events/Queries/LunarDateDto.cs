namespace backend.Application.Events.Queries;

public class LunarDateDto
{
    public int Day { get; set; }
    public int Month { get; set; }
    public bool IsLeapMonth { get; set; }
}