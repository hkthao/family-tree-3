namespace backend.Application.Events.Commands.Inputs;

public record class LunarDateInput
{
    public int Day { get; init; }
    public int Month { get; init; }
    public bool IsLeapMonth { get; init; }
}
