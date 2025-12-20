// tests/unit/composables/event/eventCalendar.adapter.test.ts
import { describe, it, expect, vi } from 'vitest';
import { DayjsDateAdapter, LunarJsDateAdapter } from '@/composables/event/eventCalendar.adapter';
import dayjs from 'dayjs';
import { Solar, Lunar } from 'lunar-javascript';

// Mock dayjs to return a chainable object for .year().month().date()
vi.mock('dayjs', () => {
  const mockDayjsInstance = {
    startOf: vi.fn(() => ({ toDate: () => new Date('2024-01-01T00:00:00.000Z') })),
    endOf: vi.fn(() => ({ toDate: () => new Date('2024-01-31T23:59:59.999Z') })),
    year: vi.fn(function(y) { if (y) return this; return 2024; }),
    month: vi.fn(function(m) { if (m) return this; return 0; }), // January
    date: vi.fn(function(d) { if (d) return this; return 15; }),
    toDate: vi.fn(function() { return new Date(this._date || '2024-01-15T12:00:00.000Z'); }),
    // Internal property for toDate mock
    _date: undefined as Date | undefined,
  };

  const mockDayjs = vi.fn((date?: any) => {
    mockDayjsInstance._date = date ? new Date(date) : undefined;
    return mockDayjsInstance;
  });

  return {
    __esModule: true,
    default: mockDayjs,
  };
});

// Mock lunar-javascript to ensure instances have all expected methods
vi.mock('lunar-javascript', () => {
  const mockSolarInstance = (year: number, month: number, day: number) => ({
    getYear: () => year,
    getMonth: () => month,
    getDay: () => day,
    getLunar: vi.fn(() => mockLunarInstance(year - 1, month + 11, day + 4)), // Mocking some lunar date
  });

  const mockLunarInstance = (year: number, month: number, day: number) => ({
    getYear: () => year,
    getMonth: () => month,
    getDay: () => day,
    isLeap: () => false,
    getSolar: vi.fn(() => mockSolarInstance(year + 1, month - 11, day - 4)), // Mocking some solar date
    getDaysInMonth: vi.fn(() => 30), // Always return 30 for simplicity in mock
  });

  return {
    Solar: {
      fromYmd: vi.fn((year, month, day) => mockSolarInstance(year, month, day)),
    },
    Lunar: {
      fromYmd: vi.fn((year, month, day) => mockLunarInstance(year, month, day)),
      fromSolar: vi.fn((solar) => mockLunarInstance(solar.getYear() - 1, solar.getMonth() + 11, solar.getDay() + 4)),
    },
  };
});

describe('DayjsDateAdapter', () => {
  const adapter = new DayjsDateAdapter();

  it('should return start of month', () => {
    const date = new Date('2024-01-15');
    const result = adapter.startOfMonth(date);
    expect(dayjs).toHaveBeenCalledWith(date);
    expect(result).toEqual(new Date('2024-01-01T00:00:00.000Z'));
  });

  it('should return end of month', () => {
    const date = new Date('2024-01-15');
    const result = adapter.endOfMonth(date);
    expect(dayjs).toHaveBeenCalledWith(date);
    expect(result).toEqual(new Date('2024-01-31T23:59:59.999Z'));
  });

  it('should return full year', () => {
    const date = new Date('2024-01-15');
    expect(adapter.getFullYear(date)).toBe(2024);
    expect(dayjs).toHaveBeenCalledWith(date);
  });

  it('should return month (0-indexed)', () => {
    const date = new Date('2024-01-15');
    expect(adapter.getMonth(date)).toBe(0);
    expect(dayjs).toHaveBeenCalledWith(date);
  });

  it('should return date', () => {
    const date = new Date('2024-01-15');
    expect(adapter.getDate(date)).toBe(15);
    expect(dayjs).toHaveBeenCalledWith(date);
  });

  it('should create new Date with arguments', () => {
    const result = adapter.newDate(2023, 11, 25); // December 25, 2023
    expect(vi.mocked(dayjs).mock.results[0].value.year).toHaveBeenCalledWith(2023);
    expect(vi.mocked(dayjs).mock.results[0].value.month).toHaveBeenCalledWith(11);
    expect(vi.mocked(dayjs).mock.results[0].value.date).toHaveBeenCalledWith(25);
    expect(result).toBeInstanceOf(Date);
  });

  it('should create new Date without arguments', () => {
    const result = adapter.newDate();
    expect(dayjs).toHaveBeenCalledWith();
    expect(result).toBeInstanceOf(Date);
  });
});

describe('LunarJsDateAdapter', () => {
  const adapter = new LunarJsDateAdapter();

  it('should call Lunar.fromYmd via lunarFromYmd', () => {
    const result = adapter.lunarFromYmd(2024, 1, 15);
    expect(vi.mocked(Lunar.fromYmd)).toHaveBeenCalledWith(2024, 1, 15);
    expect(result).toBeInstanceOf(Object); // Mocked Lunar object
  });

  it('should call Solar.fromYmd via solarFromYmd', () => {
    const result = adapter.solarFromYmd(2024, 1, 15);
    expect(vi.mocked(Solar.fromYmd)).toHaveBeenCalledWith(2024, 1, 15);
    expect(result).toBeInstanceOf(Object); // Mocked Solar object
  });

  it('should call Lunar.fromSolar', () => {
    const mockSolar = vi.mocked(Solar.fromYmd)(2024, 1, 15);
    const result = adapter.fromSolar(mockSolar);
    expect(vi.mocked(Lunar.fromSolar)).toHaveBeenCalledWith(mockSolar);
    expect(result).toBeInstanceOf(Object); // Mocked Lunar object
  });

  it('should call lunar.getSolar', () => {
    const mockLunar = vi.mocked(Lunar.fromYmd)(2023, 12, 5);
    const result = adapter.getSolar(mockLunar);
    expect(mockLunar.getSolar).toHaveBeenCalled();
    expect(result).toBeInstanceOf(Object); // Mocked Solar object
  });

  it('should call lunar.getDaysInMonth', () => {
    const mockLunar = vi.mocked(Lunar.fromYmd)(2023, 12, 5);
    const result = adapter.getDaysInMonth(mockLunar);
    expect(mockLunar.getDaysInMonth).toHaveBeenCalled();
    expect(result).toBe(30);
  });
});
