namespace backend.Application.Common.Dtos;

public class LunarDateDto
{
    public int Day { get; set; }
    public int Month { get; set; }
    public bool IsLeapMonth { get; set; }
    public bool IsEstimated { get; set; }
}
