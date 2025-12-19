using backend.Domain.Common;

namespace backend.Domain.ValueObjects;

public class LunarDate : ValueObject
{
    public int Day { get; private set; }
    public int Month { get; private set; }
    public bool IsLeapMonth { get; private set; }

    private LunarDate() { } // Required for EF Core

    public LunarDate(int day, int month, bool isLeapMonth)
    {
        if (day < 1 || day > 30) // Lunar day typically ranges from 1 to 30
        {
            throw new ArgumentOutOfRangeException(nameof(day), "Ngày âm phải từ 1 đến 30.");
        }
        if (month < 1 || month > 12) // Lunar month typically ranges from 1 to 12
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Tháng âm phải từ 1 đến 12.");
        }

        Day = day;
        Month = month;
        IsLeapMonth = isLeapMonth;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Day;
        yield return Month;
        yield return IsLeapMonth;
    }
}
