using backend.Application.Common.Services;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Common.Services;

public class LunarCalendarServiceTests
{
    private readonly LunarCalendarService _lunarCalendarService;

    public LunarCalendarServiceTests()
    {
        _lunarCalendarService = new LunarCalendarService();
    }

    [Theory]
    [InlineData(1, 1, 2024, false, 2024, 2, 10)] // Mùng 1 Tết Giáp Thìn (Lunar: 2024-01-01 -> Solar: 2024-02-10)
    [InlineData(15, 8, 2023, false, 2023, 9, 29)] // Tết Trung Thu (Lunar: 2023-08-15 -> Solar: 2023-09-29)
    [InlineData(20, 3, 2024, false, 2024, 4, 28)] // Lunar: 2024-03-20 -> Solar: 2024-04-28
    [InlineData(1, 1, 2020, false, 2020, 1, 25)] // Mùng 1 Tết Canh Tý (Lunar: 2020-01-01 -> Solar: 2020-01-25)
    [InlineData(1, 1, 2025, false, 2025, 1, 29)] // Mùng 1 Tết Ất Tỵ (Lunar: 2025-01-29)
    public void ConvertLunarToSolar_ShouldReturnCorrectSolarDate(
        int lunarDay, int lunarMonth, int lunarYear, bool isLeapMonth,
        int expectedSolarYear, int expectedSolarMonth, int expectedSolarDay)
    {
        // Act
        DateTime? solarDate = _lunarCalendarService.ConvertLunarToSolar(lunarDay, lunarMonth, lunarYear, isLeapMonth);

        // Assert
        solarDate.Should().NotBeNull();
        solarDate.Should().Be(new DateTime(expectedSolarYear, expectedSolarMonth, expectedSolarDay));
    }

    [Fact]
    public void ConvertLunarToSolar_WithLeapMonth_ShouldReturnCorrectSolarDate()
    {
        // Example for a leap month (e.g., Leap 2nd month in 2023)
        // Lunar 2023, Leap Month 2, Day 1 -> Solar 2023-03-22
        DateTime? solarDate = _lunarCalendarService.ConvertLunarToSolar(1, 2, 2023, true);

        solarDate.Should().NotBeNull();
        solarDate.Should().Be(new DateTime(2023, 3, 22));
    }

    [Theory]
    [InlineData(31, 1, 2024, false)] // Invalid day for lunar month (max 30)
    [InlineData(1, 13, 2024, false)] // Invalid month (max 12, or with leap 13)
    [InlineData(1, 0, 2024, false)] // Invalid month (min 1)
    [InlineData(0, 1, 2024, false)] // Invalid day (min 1)
    public void ConvertLunarToSolar_WithInvalidDate_ShouldReturnNull(
        int lunarDay, int lunarMonth, int lunarYear, bool isLeapMonth)
    {
        // Act
        DateTime? solarDate = _lunarCalendarService.ConvertLunarToSolar(lunarDay, lunarMonth, lunarYear, isLeapMonth);

        // Assert
        solarDate.Should().BeNull();
    }
}
