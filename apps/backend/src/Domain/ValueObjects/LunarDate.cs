namespace backend.Domain.ValueObjects;

public class LunarDate : ValueObject
{
    public int? Day { get; private set; }
    public int? Month { get; private set; }
    public bool? IsLeapMonth { get; private set; }
    public bool IsEstimated { get; private set; }

    // Private constructor for EF Core and internal use
    private LunarDate() { }

    public LunarDate(int? day, int? month, bool? isLeapMonth, bool isEstimated)
    {
        if ((day.HasValue && !month.HasValue) || (!day.HasValue && month.HasValue))
        {
            throw new ArgumentException("Day and Month must both be null or both have a value.");
        }

        if (day.HasValue && (day < 1 || day > 31)) // Lunar day typically doesn't exceed 30, but 31 for robustness
        {
            throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31.");
        }

        if (month.HasValue && (month < 1 || month > 12))
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        Day = day;
        Month = month;
        IsLeapMonth = isLeapMonth;
        IsEstimated = isEstimated;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Day.HasValue ? (object)Day.Value : DBNull.Value;
        yield return Month.HasValue ? (object)Month.Value : DBNull.Value;
        yield return IsLeapMonth.HasValue ? (object)IsLeapMonth.Value : DBNull.Value;
        yield return IsEstimated;
    }

    public override string ToString()
    {
        if (!Day.HasValue || !Month.HasValue)
        {
            return "N/A";
        }

        var leapMonth = IsLeapMonth == true ? "(Nhuận)" : "";
        var estimated = IsEstimated ? "(Ước tính)" : "";

        return $"{Day.Value}/{Month.Value}{leapMonth} {estimated}".Trim();
    }
}
