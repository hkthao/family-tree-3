namespace backend.Application.Common.Services;

public interface ILunarCalendarService
{
    DateTime? ConvertLunarToSolar(int lunarDay, int lunarMonth, int lunarYear, bool isLeapMonth);
}

public class LunarCalendarService : ILunarCalendarService
{
    /// <summary>
    /// Converts a lunar date to a solar date.
    /// </summary>
    /// <param name="lunarDay">The lunar day.</param>
    /// <param name="lunarMonth">The lunar month.</param>
    /// <param name="lunarYear">The lunar year.</param>
    /// <param name="isLeapMonth">True if the month is a leap month, otherwise false.</param>
    /// <returns>The corresponding solar date, or null if conversion fails.</returns>
    public DateTime? ConvertLunarToSolar(int lunarDay, int lunarMonth, int lunarYear, bool isLeapMonth)
    {
        try
        {
            var lunarMonthParam = isLeapMonth ? -lunarMonth : lunarMonth;
            var lunar = Lunar.Lunar.FromYmdHms(lunarYear, lunarMonthParam, lunarDay, 0, 0, 0);
            
            var solar = lunar.Solar;
            return new DateTime(solar.Year, solar.Month, solar.Day);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
