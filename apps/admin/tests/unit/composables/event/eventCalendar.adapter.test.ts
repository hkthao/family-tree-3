import { describe, it, expect } from 'vitest';
import { DayjsDateAdapter, LunarJsDateAdapter } from '@/composables/event/eventCalendar.adapter';
import dayjs from 'dayjs';
import { Lunar, Solar } from 'lunar-javascript';

describe('DayjsDateAdapter', () => {
  const adapter = new DayjsDateAdapter();

  it('should return the start of the month', () => {
    const date = new Date('2023-07-15');
    const expected = new Date('2023-07-01T00:00:00.000');
    expect(adapter.startOfMonth(date).toISOString()).toMatch(expected.toISOString().split('T')[0]);
  });

  it('should return the end of the month', () => {
    const date = new Date('2023-07-15');
    const expected = new Date('2023-07-31T23:59:59.999');
    expect(adapter.endOfMonth(date).toISOString()).toMatch(expected.toISOString().split('T')[0]);
  });

  it('should return the full year', () => {
    const date = new Date('2023-07-15');
    expect(adapter.getFullYear(date)).toBe(2023);
  });

  it('should return the month (0-indexed)', () => {
    const date = new Date('2023-07-15');
    expect(adapter.getMonth(date)).toBe(6); // July is 6 (0-indexed)
  });

  it('should return the date', () => {
    const date = new Date('2023-07-15');
    expect(adapter.getDate(date)).toBe(15);
  });

  it('should create a new date with specified year, month, and day', () => {
    const expected = new Date(2023, 0, 1); // January 1, 2023
    const actual = adapter.newDate(2023, 0, 1);
    expect(actual.getFullYear()).toBe(expected.getFullYear());
    expect(actual.getMonth()).toBe(expected.getMonth());
    expect(actual.getDate()).toBe(expected.getDate());
  });

  it('should create a new date without arguments (current date)', () => {
    const now = new Date();
    const actual = adapter.newDate();
    expect(actual.getFullYear()).toBe(now.getFullYear());
    expect(actual.getMonth()).toBe(now.getMonth());
    expect(actual.getDate()).toBe(now.getDate());
  });
});

describe('LunarJsDateAdapter', () => {
  const adapter = new LunarJsDateAdapter();

  it('should create a Lunar object from year, month, day', () => {
    const lunar = adapter.lunarFromYmd(2023, 5, 1); // Lunar May 1, 2023
    expect(lunar.getYear()).toBe(2023);
    expect(lunar.getMonth()).toBe(5);
    expect(lunar.getDay()).toBe(1);
  });

  it('should create a Solar object from year, month, day', () => {
    const solar = adapter.solarFromYmd(2023, 7, 15); // Solar July 15, 2023
    expect(solar.getYear()).toBe(2023);
    expect(solar.getMonth()).toBe(7);
    expect(solar.getDay()).toBe(15);
  });

  it('should convert Solar to Lunar', () => {
    const solar = Solar.fromYmd(2023, 7, 15); // Solar July 15, 2023
    const lunar = adapter.fromSolar(solar);
    // Expected Lunar date for Solar 2023-07-15 is 2023-05-28
    expect(lunar.getYear()).toBe(2023);
    expect(lunar.getMonth()).toBe(5);
    expect(lunar.getDay()).toBe(28);
  });

  it('should convert Lunar to Solar', () => {
    const lunar = Lunar.fromYmd(2023, 5, 28); // Lunar May 28, 2023
    const solar = adapter.getSolar(lunar);
    // Expected Solar date for Lunar 2023-05-28 is 2023-07-15
    expect(solar.getYear()).toBe(2023);
    expect(solar.getMonth()).toBe(7);
    expect(solar.getDay()).toBe(15);
  });

  it('should get the number of lunar days in a month', () => {
    // Lunar 2023, month 5 (May) has 30 days
    expect(adapter.getLunarDaysInMonth(2023, 5)).toBe(30);
    // Lunar 2023, month 6 (June) has 29 days
    expect(adapter.getLunarDaysInMonth(2023, 6)).toBe(29);
  });
});
