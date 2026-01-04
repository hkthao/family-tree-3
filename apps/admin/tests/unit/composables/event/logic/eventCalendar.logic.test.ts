import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import {
  getSolarDateFromLunarDate,
  getLunarDateForSolarDay,
  getLunarDateRangeFiltersLogic,
  formatEventsForCalendarLogic,
  getWeekdaysLogic,
  canManageEventLogic,
  getCalendarTitleLogic,
} from '@/composables/event/logic/eventCalendar.logic';
import type { DateAdapter, LunarDateAdapter, LunarInstance, SolarInstance } from '@/composables/event/eventCalendar.adapter';
import { type EventDto, type LunarDate, EventType } from '@/types';
import { CalendarType } from '@/types/enums';

// Mock implementations for DateAdapter
const mockDateAdapter: DateAdapter = {
  startOfMonth: vi.fn((date: Date | string) => new Date(new Date(date).getFullYear(), new Date(date).getMonth(), 1)) as Mock,
  endOfMonth: vi.fn((date: Date | string) => {
    const d = new Date(date);
    return new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59, 999);
  }) as Mock,
  getFullYear: vi.fn((date: Date) => date.getFullYear()) as Mock,
  getMonth: vi.fn((date: Date) => date.getMonth()) as Mock,
  getDate: vi.fn((date: Date) => date.getDate()) as Mock,
  newDate: vi.fn((year?: number, month?: number, day?: number) => {
    if (year !== undefined && month !== undefined && day !== undefined) {
      return new Date(year, month, day);
    }
    return new Date();
  }) as Mock,
  isSameDay: vi.fn((date1: Date, date2: Date) => date1.toDateString() === date2.toDateString()) as Mock, // ADD THIS LINE
};

// Mock implementations for LunarDateAdapter
const mockLunarDateAdapter: LunarDateAdapter = {
  lunarFromYmd: vi.fn((year: number, month: number, day: number) => {
    // Simplified mock for LunarInstance
    return {
      getYear: () => year,
      getMonth: () => month,
      getDay: () => day,
      getSolar: vi.fn(() => { // Mock getSolar to return a SolarInstance
        // For simplicity, this mock returns a SolarInstance that corresponds to the given Lunar date
        // The actual conversion logic is handled by the real Lunar.fromYmd().toSolar()
        // Provide consistent mocking for getSolar result in lunarFromYmd
        return mockLunarDateAdapter.solarFromYmd(year, month + 2, day + 15); // Example approximation
      })
    } as unknown as LunarInstance;
  }) as Mock,
  solarFromYmd: vi.fn((year: number, month: number, day: number) => {
    // Simplified mock for SolarInstance
    return {
      getYear: () => year,
      getMonth: () => month,
      getDay: () => day,
      getLunar: vi.fn(() => { // Mock getLunar to return a LunarInstance
        // For simplicity, this mock returns a LunarInstance that corresponds to the given Solar date
        // The actual conversion logic is handled by the real Solar.fromYmd().toLunar()
        // Provide consistent mocking for getLunar result in solarFromYmd
        return mockLunarDateAdapter.lunarFromYmd(year, month - 2, day - 15); // Example approximation
      })
    } as unknown as SolarInstance;
  }) as Mock,
  fromSolar: vi.fn((solar: SolarInstance) => {
    // Example mapping, adjust based on actual conversion logic if needed
    // For simplicity, let's say Solar 2023-07-15 is Lunar 2023-05-28
    if (solar.getYear() === 2023 && solar.getMonth() === 7 && solar.getDay() === 15) {
      return mockLunarDateAdapter.lunarFromYmd(2023, 5, 28);
    }
    return mockLunarDateAdapter.lunarFromYmd(solar.getYear(), solar.getMonth(), solar.getDay());
  }) as Mock,

  getSolar: vi.fn((lunar: LunarInstance) => {
    // Example mapping, adjust based on actual conversion logic if needed
    // For simplicity, let's say Lunar 2023-05-28 is Solar 2023-07-15
    if (lunar.getYear() === 2023 && lunar.getMonth() === 5 && lunar.getDay() === 28) {
      return {
        getYear: () => 2023,
        getMonth: () => 7, // July (1-indexed for Solar)
        getDay: () => 15,
        getLunar: vi.fn()
      } as unknown as SolarInstance;
    }
    if (lunar.getYear() === 2023 && lunar.getMonth() === 5 && lunar.getDay() === 30) { // Add specific mapping for adjusted day
      return mockLunarDateAdapter.solarFromYmd(2023, 7, 16);
    }
    // Fallback if no specific mapping, just return a solar instance
    return mockLunarDateAdapter.solarFromYmd(lunar.getYear(), lunar.getMonth(), lunar.getDay());
  }) as Mock,

  getLunarDaysInMonth: vi.fn((year: number, month: number) => {
    // Mock known values for specific months for testing
    if (year === 2023 && month === 5) return 30;
    if (year === 2023 && month === 6) return 29;
    return 30; // Default
  }) as Mock,

};

describe('eventCalendar.logic', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getSolarDateFromLunarDate', () => {
    it('should convert lunar date to solar date', () => {
      const year = 2023;
      const lunarDate: LunarDate = { day: 28, month: 5, isLeapMonth: false }; // Lunar May 28, 2023
      const expectedSolarDate = new Date(2023, 6, 15); // Solar July 15, 2023 (0-indexed month)

      // Directly mock getSolar to return the expected SolarInstance for the lunarFromYmd call
      (mockLunarDateAdapter.getSolar as Mock).mockReturnValueOnce({
        getYear: () => expectedSolarDate.getFullYear(),
        getMonth: () => expectedSolarDate.getMonth() + 1, // Solar month is 1-indexed
        getDay: () => expectedSolarDate.getDate()
      } as unknown as SolarInstance);
      // Ensure lunarFromYmd is mocked to return a valid LunarInstance without recursive solarFromYmd
      (mockLunarDateAdapter.lunarFromYmd as Mock).mockImplementationOnce((y, m, d) => ({
        getYear: () => y, getMonth: () => m, getDay: () => d, getSolar: vi.fn() // Simplified getSolar here
      }) as unknown as LunarInstance);


      const result = getSolarDateFromLunarDate(year, lunarDate, mockLunarDateAdapter, mockDateAdapter);

      expect(result.getFullYear()).toBe(expectedSolarDate.getFullYear());
      expect(result.getMonth()).toBe(expectedSolarDate.getMonth());
      expect(result.getDate()).toBe(expectedSolarDate.getDate());
      expect(mockLunarDateAdapter.getLunarDaysInMonth).toHaveBeenCalledWith(year, lunarDate.month);
    });

    it('should adjust day if lunarDate.day is too high', () => {
      const year = 2023;
      const lunarDate: LunarDate = { day: 31, month: 5, isLeapMonth: false }; // Lunar May 31, 2023 (invalid day)
      const adjustedDay = 30; // Max days in mock Lunar May 2023
      const expectedSolarDate = new Date(2023, 6, 16); // Example: Solar July 16, 2023 for adjusted day (0-indexed month)

      (mockLunarDateAdapter.getLunarDaysInMonth as Mock).mockReturnValueOnce(30); // Max day for mock month
      (mockLunarDateAdapter.lunarFromYmd as Mock).mockImplementationOnce((y: number, m: number, d: number) => {
        expect(d).toBe(adjustedDay); // Ensure day is adjusted before calling lunarFromYmd
        return {
          getYear: () => y,
          getMonth: () => m,
          getDay: () => d,
          getSolar: vi.fn(() => ({ // Mock getSolar to return the expected solar date
            getYear: () => expectedSolarDate.getFullYear(),
            getMonth: () => expectedSolarDate.getMonth() + 1, // Solar month is 1-indexed
            getDay: () => expectedSolarDate.getDate()
          }) as unknown as SolarInstance)
        } as unknown as LunarInstance;
      });

      const result = getSolarDateFromLunarDate(year, lunarDate, mockLunarDateAdapter, mockDateAdapter);
      expect(result.getFullYear()).toBe(expectedSolarDate.getFullYear());
      expect(result.getMonth()).toBe(expectedSolarDate.getMonth());
      expect(result.getDate()).toBe(expectedSolarDate.getDate());
    });

    it('should handle errors during conversion and return fallback', () => {
      const year = 2023;
      const lunarDate: LunarDate = { day: 1, month: 1, isLeapMonth: false };
      (mockLunarDateAdapter.lunarFromYmd as Mock).mockImplementationOnce(() => { throw new Error('Lunar conversion failed'); });
      const spy = vi.spyOn(console, 'error').mockImplementation(() => { }); // Mock console.error
      const result = getSolarDateFromLunarDate(year, lunarDate, mockLunarDateAdapter, mockDateAdapter);
      expect(spy).toHaveBeenCalledWith('Error during lunar to solar conversion:', expect.any(Error));
      expect(result.getFullYear()).toBe(year);
      expect(result.getMonth()).toBe(lunarDate.month - 1);
      expect(result.getDate()).toBe(lunarDate.day);
      spy.mockRestore();
    });
  });

  describe('getLunarDateForSolarDay', () => {
    it('should convert solar date to lunar date string', () => {
      const solarDate = new Date(2023, 6, 15); // Solar July 15, 2023 (0-indexed month)
      const expectedLunarString = '28/5'; // Lunar May 28
      (mockDateAdapter.getFullYear as Mock).mockReturnValueOnce(2023);
      (mockDateAdapter.getMonth as Mock).mockReturnValueOnce(6);
      (mockDateAdapter.getDate as Mock).mockReturnValueOnce(15);
      (mockLunarDateAdapter.solarFromYmd as Mock).mockReturnValueOnce({
        getYear: () => 2023, getMonth: () => 7, getDay: () => 15, // Solar month is 1-indexed
        getLunar: () => ({ getDay: () => 28, getMonth: () => 5 }) as unknown as LunarInstance
      } as unknown as SolarInstance);


      const result = getLunarDateForSolarDay(solarDate, mockLunarDateAdapter, mockDateAdapter);
      expect(result).toBe(expectedLunarString);
      expect(mockDateAdapter.getFullYear).toHaveBeenCalledWith(solarDate);
      expect(mockLunarDateAdapter.solarFromYmd).toHaveBeenCalled();
    });

    it('should handle invalid solarDate input', () => {
      const invalidDate = new Date('invalid date');
      const spy = vi.spyOn(console, 'error').mockImplementation(() => { });

      const result = getLunarDateForSolarDay(invalidDate, mockLunarDateAdapter, mockDateAdapter);
      expect(result).toBe('Ngày không hợp lệ (Âm)');
      expect(spy).toHaveBeenCalledWith('Invalid solarDate input to getLunarDateForSolarDay:', invalidDate);
      spy.mockRestore();
    });

    it('should handle errors during conversion and return fallback', () => {
      const solarDate = new Date(2023, 6, 15);
      (mockDateAdapter.getFullYear as Mock).mockReturnValueOnce(2023);
      (mockDateAdapter.getMonth as Mock).mockReturnValueOnce(6);
      (mockDateAdapter.getDate as Mock).mockReturnValueOnce(15);
      (mockLunarDateAdapter.solarFromYmd as Mock).mockImplementationOnce(() => { throw new Error('Solar conversion failed'); });

      const spy = vi.spyOn(console, 'error').mockImplementation(() => { }); // Mock console.error
      const result = getLunarDateForSolarDay(solarDate, mockLunarDateAdapter, mockDateAdapter);

      expect(spy).toHaveBeenCalledWith('Error during solar to lunar conversion:', expect.any(Error));
      expect(result).toBe('Lỗi chuyển đổi (Âm)');
      spy.mockRestore();
    });
  });

  describe('getLunarDateRangeFiltersLogic', () => {
    it('should compute lunar date range filters based on selected solar date', () => {
      const selectedDate = new Date(2023, 6, 15); // Solar July 15, 2023
      const startOfMonth = new Date(2023, 6, 1);
      const endOfMonth = new Date(2023, 7, 0, 23, 59, 59, 999);

      (mockDateAdapter.startOfMonth as Mock).mockReturnValue(startOfMonth);
      (mockDateAdapter.endOfMonth as Mock).mockReturnValue(endOfMonth);
      (mockDateAdapter.getFullYear as Mock).mockReturnValue(2023);
      (mockDateAdapter.getMonth as Mock)
        .mockReturnValueOnce(6) // startOfMonth.getMonth()
        .mockReturnValueOnce(7) // endOfMonth.getMonth() - this is 1-indexed in lunar calls
        .mockReturnValueOnce(7); // endOfMonth.getMonth() - for getLunarDaysInMonth

      // Mock fromYmd and getLunar for start and end of month
      (mockLunarDateAdapter.solarFromYmd as Mock)
        .mockImplementationOnce((y: number, m: number, d: number) => ({ getYear: () => y, getMonth: () => m, getDay: () => d, getLunar: () => ({ getMonth: () => 5, getDay: () => 14 }) })) // Start of month (Solar 2023-07-01 -> Lunar 2023-05-14)
        .mockImplementationOnce((y: number, m: number, d: number) => ({ getYear: () => y, getMonth: () => m, getDay: () => d, getLunar: () => ({ getMonth: () => 6, getDay: () => 14 }) })) as Mock; // End of month (Solar 2023-07-31 -> Lunar 2023-06-14)

      (mockLunarDateAdapter.getLunarDaysInMonth as Mock).mockReturnValue(30); // for endLunar month 6

      const result = getLunarDateRangeFiltersLogic(selectedDate, mockDateAdapter, mockLunarDateAdapter);

      expect(result).toEqual({
        lunarMonthRange: [5, 6], // Expected Lunar May to June
      });
      expect(mockLunarDateAdapter.solarFromYmd).toHaveBeenCalledTimes(2);
    });

    it('should return default filters if Solar instances are invalid', () => {
      const selectedDate = new Date(2023, 6, 15);
      (mockDateAdapter.startOfMonth as Mock).mockReturnValue(new Date(2023, 6, 1));
      (mockDateAdapter.endOfMonth as Mock).mockReturnValue(new Date(2023, 7, 0));
      (mockLunarDateAdapter.solarFromYmd as Mock).mockReturnValue(null as any); // Simulate invalid Solar instance

      const spy = vi.spyOn(console, 'error').mockImplementation(() => { });
      const result = getLunarDateRangeFiltersLogic(selectedDate, mockDateAdapter, mockLunarDateAdapter);

      expect(result).toEqual({
        lunarMonthRange: [1, 12],
      });
      spy.mockRestore();
    });

    it('should return default filters if Lunar instances are invalid', () => {
      const selectedDate = new Date(2023, 6, 15);
      (mockDateAdapter.startOfMonth as Mock).mockReturnValue(new Date(2023, 6, 1));
      (mockDateAdapter.endOfMonth as Mock).mockReturnValue(new Date(2023, 7, 0));
      (mockLunarDateAdapter.solarFromYmd as Mock)
        .mockReturnValueOnce({ getLunar: () => null } as any) // Simulate invalid Lunar instance
        .mockReturnValueOnce({ getLunar: () => ({ getMonth: () => 6, getDay: () => 14 }) } as any);

      const spy = vi.spyOn(console, 'error').mockImplementation(() => { });
      const result = getLunarDateRangeFiltersLogic(selectedDate, mockDateAdapter, mockLunarDateAdapter);

      expect(result).toEqual({
        lunarMonthRange: [1, 12],
      });
      expect(spy).toHaveBeenCalledWith('Invalid Lunar instances during lunar date range calculation.');
      spy.mockRestore();
    });
  });

  describe('formatEventsForCalendarLogic', () => {
    const mockEvent1: EventDto = {
      id: '1', name: 'Solar EventDto', type: EventType.Other, solarDate: new Date(2023, 6, 10),
      familyId: '', calendarType: CalendarType.Solar, lunarDate: null, repeatRule: 0, description: '', code: '', eventMemberIds: []
    };
    const mockEvent2: EventDto = {
      id: '2', name: 'Lunar EventDto', type: EventType.Other, lunarDate: { day: 28, month: 5, isLeapMonth: false },
      familyId: '', calendarType: CalendarType.Lunar, solarDate: null, repeatRule: 0, description: '', code: '', eventMemberIds: []
    };
    const mockEventOutsideMonth: EventDto = {
      id: '3', name: 'Outside EventDto', type: EventType.Other, solarDate: new Date(2023, 5, 1),
      familyId: '', calendarType: CalendarType.Solar, lunarDate: null, repeatRule: 0, description: '', code: '', eventMemberIds: []
    };

    it('should format solar events for calendar', () => {
      const selectedDate = new Date(2023, 6, 15); // July 2023
      const events = [mockEvent1];

      const result = formatEventsForCalendarLogic(events, selectedDate, mockDateAdapter, mockLunarDateAdapter);

      expect(result).toHaveLength(1);
      expect(result[0].title).toBe('Solar EventDto');
      expect(result[0].start.getMonth()).toBe(6); // July
      expect(result[0].start.getDate()).toBe(10);
      expect(result[0].eventObject).toEqual(mockEvent1);
    });

    it('should format lunar events for calendar', () => {
      const selectedDate = new Date(2023, 6, 15); // July 2023
      const events = [mockEvent2];

      (mockLunarDateAdapter.getLunarDaysInMonth as Mock).mockReturnValue(30);

      const result = formatEventsForCalendarLogic(events, selectedDate, mockDateAdapter, mockLunarDateAdapter);

      expect(result).toHaveLength(1);
      expect(result[0].title).toBe('Lunar EventDto');
      expect(result[0].start.getMonth()).toBe(6); // July
      expect(result[0].start.getDate()).toBe(15);
      expect(result[0].eventObject).toEqual(mockEvent2);
    });

    it('should filter out events outside the selected month', () => {
      const selectedDate = new Date(2023, 6, 15); // July 2023
      const events = [mockEvent1, mockEventOutsideMonth];

      const result = formatEventsForCalendarLogic(events, selectedDate, mockDateAdapter, mockLunarDateAdapter);

      expect(result).toHaveLength(1);
      expect(result[0].title).toBe('Solar EventDto');
    });

    it('should return empty array if no events are provided', () => {
      const selectedDate = new Date(2023, 6, 15);
      const result = formatEventsForCalendarLogic(null as any, selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(result).toHaveLength(0);
    });

    it('should handle events without valid dates', () => {
      const eventWithoutDate: EventDto = {
        id: '4', name: 'No Date EventDto', type: EventType.Other, solarDate: null,
        familyId: '', calendarType: CalendarType.Solar, lunarDate: null, repeatRule: 0, description: '', code: '', eventMemberIds: []
      };
      const selectedDate = new Date(2023, 6, 15);
      const events = [eventWithoutDate];

      const result = formatEventsForCalendarLogic(events, selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(result).toHaveLength(0);
    });
  });

  describe('getWeekdaysLogic', () => {
    it('should return an array of weekdays starting from Sunday', () => {
      expect(getWeekdaysLogic()).toEqual([0, 1, 2, 3, 4, 5, 6]);
    });
  });

  describe('canManageEventLogic', () => {
    const isFamilyManager = vi.fn((familyId: string) => familyId === 'family1');

    it('should return true if not readOnly and is admin', () => {
      expect(canManageEventLogic(false, true, isFamilyManager, 'family2')).toBe(true);
    });

    it('should return true if not readOnly and is family manager', () => {
      expect(canManageEventLogic(false, false, isFamilyManager, 'family1')).toBe(true);
    });

    it('should return false if readOnly', () => {
      expect(canManageEventLogic(true, true, isFamilyManager, 'family1')).toBe(false);
    });

    it('should return false if not admin and not family manager', () => {
      expect(canManageEventLogic(false, false, isFamilyManager, 'family2')).toBe(false);
    });

    it('should handle undefined familyId correctly', () => {
      expect(canManageEventLogic(false, false, isFamilyManager, undefined)).toBe(false);
    });
  });

  describe('getCalendarTitleLogic', () => {
    it('should return the title from calendarRefValue', () => {
      const calendarRefValue = { title: 'Test Calendar Title' };
      expect(getCalendarTitleLogic(calendarRefValue)).toBe('Test Calendar Title');
    });

    it('should return empty string if calendarRefValue is null', () => {
      expect(getCalendarTitleLogic(null)).toBe('');
    });

    it('should return empty string if calendarRefValue has no title', () => {
      const calendarRefValue = {};
      expect(getCalendarTitleLogic(calendarRefValue as any)).toBe('');
    });
  });
});
