// tests/unit/composables/event/logic/eventCalendar.logic.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  getSolarDateFromLunarDate,
  getLunarDateForSolarDay,
  getLunarDateRangeFiltersLogic,
  getEventFilterLogic,
  formatEventsForCalendarLogic,
  getWeekdaysLogic,
  canManageEventLogic,
  getCalendarTitleLogic,
} from '@/composables/event/logic/eventCalendar.logic';
import type { Event, LunarDate } from '@/types';
import { CalendarType } from '@/types/enums';
import type { DateAdapter, LunarDateAdapter } from '@/composables/event/eventCalendar.adapter';

describe('eventCalendar.logic', () => {
  let mockDateAdapter: DateAdapter;
  let mockLunarDateAdapter: LunarDateAdapter;
  let mockT: vi.Mock;

  beforeEach(() => {
    vi.clearAllMocks();

    mockDateAdapter = {
      startOfMonth: vi.fn((date) => new Date(new Date(date).getFullYear(), new Date(date).getMonth(), 1)),
      endOfMonth: vi.fn((date) => new Date(new Date(date).getFullYear(), new Date(date).getMonth() + 1, 0)),
      getFullYear: vi.fn((date) => date.getFullYear()),
      getMonth: vi.fn((date) => date.getMonth()),
      getDate: vi.fn((date) => date.getDate()),
      newDate: vi.fn((year, month, day) => new Date(year, month, day)),
    };

    mockLunarDateAdapter = {
      fromYmd: vi.fn((year, month, day) => ({
        getSolar: vi.fn(() => ({ getYear: () => year, getMonth: () => month, getDay: () => day })),
        getLunar: vi.fn(() => ({ getYear: () => year, getMonth: () => month, getDay: () => day })),
      } as any)),
      fromSolar: vi.fn((solar) => ({
        getYear: () => solar.getYear() - 1, // Mock lunar year
        getMonth: () => solar.getMonth() + 11, // Mock lunar month
        getDay: () => solar.getDay() + 4, // Mock lunar day
        getSolar: vi.fn(() => solar),
      } as any)),
      getSolar: vi.fn((lunar) => lunar.getSolar()),
    };
    mockT = vi.fn((key) => key);
  });

  describe('getSolarDateFromLunarDate', () => {
    it('should convert lunar date to solar date', () => {
      const result = getSolarDateFromLunarDate(2024, { day: 1, month: 1, isLeapMonth: false }, mockLunarDateAdapter, mockDateAdapter);
      expect(mockLunarDateAdapter.fromYmd).toHaveBeenCalledWith(2024, 1, 1);
      expect(result).toEqual(new Date(2024, 0, 1)); // Month is 0-indexed in new Date
    });

    it('should handle error during conversion and return fallback', () => {
      mockLunarDateAdapter.fromYmd.mockImplementation(() => {
        throw new Error('Invalid lunar date');
      });
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      const result = getSolarDateFromLunarDate(2024, { day: 31, month: 20, isLeapMonth: false }, mockLunarDateAdapter, mockDateAdapter);
      expect(consoleSpy).toHaveBeenCalled();
      expect(result).toEqual(new Date(2024, 19, 31)); // Fallback uses original values
      consoleSpy.mockRestore();
    });
  });

  describe('getLunarDateForSolarDay', () => {
    it('should convert solar date to lunar date', () => {
      const solarDate = new Date(2024, 0, 15);
      const result = getLunarDateForSolarDay(solarDate, mockLunarDateAdapter, mockDateAdapter);
      expect(mockDateAdapter.getFullYear).toHaveBeenCalledWith(solarDate);
      expect(mockLunarDateAdapter.fromSolar).toHaveBeenCalled();
      expect(result).toBe('19/12'); // Based on mockLunarDateAdapter.fromSolar
    });

    it('should handle invalid solarDate and return fallback string', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      const result = getLunarDateForSolarDay(new Date('invalid'), mockLunarDateAdapter, mockDateAdapter);
      expect(consoleSpy).toHaveBeenCalled();
      expect(result).toBe('Ngày không hợp lệ (Âm)');
      consoleSpy.mockRestore();
    });

    it('should handle error during conversion and return fallback string', () => {
      mockLunarDateAdapter.fromYmd.mockImplementation(() => {
        throw new Error('Conversion error');
      });
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      const result = getLunarDateForSolarDay(new Date(2024, 0, 1), mockLunarDateAdapter, mockDateAdapter);
      expect(consoleSpy).toHaveBeenCalled();
      expect(result).toBe('Lỗi chuyển đổi (Âm)');
      consoleSpy.mockRestore();
    });
  });

  describe('getLunarDateRangeFiltersLogic', () => {
    it('should compute correct lunar date range filters', () => {
      const selectedDate = new Date(2024, 0, 15);
      const result = getLunarDateRangeFiltersLogic(selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(mockDateAdapter.startOfMonth).toHaveBeenCalledWith(selectedDate);
      expect(mockDateAdapter.endOfMonth).toHaveBeenCalledWith(selectedDate);
      expect(result).toEqual({ lunarStartDay: 1, lunarStartMonth: 1, lunarEndDay: 31, lunarEndMonth: 1 });
    });
  });

  describe('getEventFilterLogic', () => {
    it('should construct the EventFilter correctly', () => {
      const selectedDate = new Date(2024, 0, 15);
      const lunarDateRangeFilters = { lunarStartDay: 1, lunarStartMonth: 1, lunarEndDay: 30, lunarEndMonth: 1 };
      const result = getEventFilterLogic(selectedDate, 'fam1', 'mem1', lunarDateRangeFilters, mockDateAdapter);
      expect(mockDateAdapter.startOfMonth).toHaveBeenCalledWith(selectedDate);
      expect(mockDateAdapter.endOfMonth).toHaveBeenCalledWith(selectedDate);
      expect(result).toEqual({
        familyId: 'fam1',
        startDate: new Date(2024, 0, 1),
        endDate: new Date(2024, 1, 0),
        lunarStartDay: 1,
        lunarStartMonth: 1,
        lunarEndDay: 30,
        lunarEndMonth: 1,
        memberId: 'mem1',
      });
    });
  });

  describe('formatEventsForCalendarLogic', () => {
    const mockEvents: Event[] = [
      { id: '1', name: 'Solar Event', calendarType: CalendarType.Solar, solarDate: new Date(2024, 0, 10), familyId: 'f1', type: 'Other', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' },
      { id: '2', name: 'Lunar Event', calendarType: CalendarType.Lunar, lunarDate: { day: 5, month: 1, isLeapMonth: false }, familyId: 'f1', type: 'Other', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' },
      { id: '3', name: 'No Date Event', familyId: 'f1', type: 'Other', calendarType: CalendarType.Solar, solarDate: null, repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000' },
    ];
    const selectedDate = new Date(2024, 0, 1);

    it('should format solar events correctly', () => {
      const formatted = formatEventsForCalendarLogic([mockEvents[0]], selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(formatted).toHaveLength(1);
      expect(formatted[0]).toMatchObject({
        title: 'Solar Event',
        start: new Date(2024, 0, 10),
        end: new Date(2024, 0, 10),
        color: 'primary',
        timed: true,
        eventObject: mockEvents[0],
      });
    });

    it('should format lunar events correctly', () => {
      const formatted = formatEventsForCalendarLogic([mockEvents[1]], selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(formatted).toHaveLength(1);
      expect(formatted[0]).toMatchObject({
        title: 'Lunar Event',
        start: new Date(2024, 0, 5), // Based on mock getSolarDateFromLunarDate
        end: new Date(2024, 0, 5),
        color: 'primary',
        timed: true,
        eventObject: mockEvents[1],
      });
    });

    it('should filter out events without valid start dates', () => {
      const formatted = formatEventsForCalendarLogic(mockEvents, selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(formatted).toHaveLength(2); // No Date Event should be filtered out
      expect(formatted.some(e => e.title === 'No Date Event')).toBe(false);
    });

    it('should return empty array for empty events input', () => {
      const formatted = formatEventsForCalendarLogic([], selectedDate, mockDateAdapter, mockLunarDateAdapter);
      expect(formatted).toEqual([]);
    });
  });

  describe('getWeekdaysLogic', () => {
    it('should return an array of weekdays', () => {
      expect(getWeekdaysLogic()).toEqual([0, 1, 2, 3, 4, 5, 6]);
    });
  });

  describe('canManageEventLogic', () => {
    it('should return true if not readOnly and isAdmin', () => {
      expect(canManageEventLogic(false, true, vi.fn(), 'f1')).toBe(true);
    });
    it('should return true if not readOnly and isFamilyManager', () => {
      expect(canManageEventLogic(false, false, vi.fn(() => true), 'f1')).toBe(true);
    });
    it('should return false if readOnly', () => {
      expect(canManageEventLogic(true, true, vi.fn(() => true), 'f1')).toBe(false);
    });
    it('should return false if not admin and not family manager', () => {
      expect(canManageEventLogic(false, false, vi.fn(() => false), 'f1')).toBe(false);
    });
  });

  describe('getCalendarTitleLogic', () => {
    it('should return title from calendarRefValue', () => {
      expect(getCalendarTitleLogic({ title: 'Test Title' } as any)).toBe('Test Title');
    });
    it('should return empty string if calendarRefValue is null', () => {
      expect(getCalendarTitleLogic(null)).toBe('');
    });
    it('should return empty string if calendarRefValue has no title', () => {
      expect(getCalendarTitleLogic({} as any)).toBe('');
    });
  });
});
