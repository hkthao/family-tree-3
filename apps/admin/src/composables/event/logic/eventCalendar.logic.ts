// src/composables/event/logic/eventCalendar.logic.ts
import type { Event, LunarDate } from '@/types';
import { CalendarType } from '@/types/enums';
import type { DateAdapter, LunarDateAdapter, LunarInstance, SolarInstance } from '@/composables/event/eventCalendar.adapter';

/**
 * Helper function to convert LunarDate to a Solar Date using LunarDateAdapter.
 */
export function getSolarDateFromLunarDate(
  year: number,
  lunarDate: LunarDate,
  lunarDateAdapter: LunarDateAdapter,
  dateAdapter: DateAdapter,
): Date {
  try {
    // Get actual days in the lunar month to validate and adjust lunarDate.day if necessary
    const maxDayInMonth = lunarDateAdapter.getLunarDaysInMonth(year, lunarDate.month);
    const day = Math.min(lunarDate.day, maxDayInMonth); // Adjust day if it's too high

    const lunar: LunarInstance = lunarDateAdapter.lunarFromYmd(year, lunarDate.month, day);
    const solar: SolarInstance = lunarDateAdapter.getSolar(lunar);
    if (!solar) {
      console.warn(`Could not get solar date for lunar date: year=${year}, month=${lunarDate.month}, day=${day}. Falling back.`);
      return dateAdapter.newDate(year, lunarDate.month - 1, day); // Use adjusted day for fallback
    }
    return dateAdapter.newDate(solar.getYear(), solar.getMonth() - 1, solar.getDay());
  } catch (e) {
    console.error('Error during lunar to solar conversion:', e);
    // Fallback if conversion fails
    return dateAdapter.newDate(year, lunarDate.month - 1, lunarDate.day);
  }
}

/**
 * Helper function to convert Solar Date to a Lunar Date using LunarDateAdapter.
 */
export function getLunarDateForSolarDay(
  solarDate: Date,
  lunarDateAdapter: LunarDateAdapter,
  dateAdapter: DateAdapter,
): string {
  if (!(solarDate instanceof Date) || isNaN(solarDate.getTime())) {
    console.error('Invalid solarDate input to getLunarDateForSolarDay:', solarDate);
    return 'Ngày không hợp lệ (Âm)'; // Fallback in Vietnamese
  }

  try {
    const year = dateAdapter.getFullYear(solarDate);
    const month = dateAdapter.getMonth(solarDate) + 1; // getMonth() is 0-indexed
    const day = dateAdapter.getDate(solarDate);
    const solar: SolarInstance = lunarDateAdapter.solarFromYmd(year, month, day);
    const lunar: LunarInstance = lunarDateAdapter.fromSolar(solar);
    return `${lunar.getDay()}/${lunar.getMonth()}`;
  } catch (e) {
    console.error('Error during solar to lunar conversion:', e);
    return 'Lỗi chuyển đổi (Âm)'; // Fallback in Vietnamese
  }
}

/**
 * Computes lunar date range filters based on the selected solar date.
 */
export function getLunarDateRangeFiltersLogic(
  selectedDate: Date,
  dateAdapter: DateAdapter,
  lunarDateAdapter: LunarDateAdapter,
) {
  const startOfMonth = dateAdapter.startOfMonth(selectedDate);
  const endOfMonth = dateAdapter.endOfMonth(selectedDate);

  const startSolar = lunarDateAdapter.solarFromYmd(
    dateAdapter.getFullYear(startOfMonth),
    dateAdapter.getMonth(startOfMonth) + 1,
    dateAdapter.getDate(startOfMonth),
  );
  const endSolar = lunarDateAdapter.solarFromYmd(
    dateAdapter.getFullYear(endOfMonth),
    dateAdapter.getMonth(endOfMonth) + 1,
    dateAdapter.getDate(endOfMonth),
  );

  // Ensure Solar instances are valid before calling getLunar()
  if (!startSolar || !endSolar) {
    return {
      lunarMonthRange: [1, 12],
    };
  }

  const startLunar = startSolar.getLunar();
  const endLunar = endSolar.getLunar();

  // Ensure Lunar instances are valid before accessing properties
  if (!startLunar || !endLunar) {
    console.error('Invalid Lunar instances during lunar date range calculation.');
    return {
      lunarMonthRange: [1, 12],
    };
  }

  const lunarStartMonth = startLunar.getMonth();
  const lunarEndMonth = endLunar.getMonth();

  return {
    lunarMonthRange: [lunarStartMonth, lunarEndMonth],
  };
}



/**
 * Formats raw event data into a structure suitable for a calendar component.
 */
export function formatEventsForCalendarLogic(
  events: Event[],
  selectedDate: Date,
  dateAdapter: DateAdapter,
  lunarDateAdapter: LunarDateAdapter,
): any[] {
  if (!events) return [];

  const startOfMonth = dateAdapter.startOfMonth(selectedDate);
  const endOfMonth = dateAdapter.endOfMonth(selectedDate);

  return events
    .map((event: Event) => {
      let eventStart: Date | null = null;
      let eventEnd: Date | null = null;

      if (event.calendarType === CalendarType.Solar && event.solarDate) {
        eventStart = dateAdapter.newDate(
          dateAdapter.getFullYear(event.solarDate),
          dateAdapter.getMonth(event.solarDate),
          dateAdapter.getDate(event.solarDate),
        );
        eventEnd = dateAdapter.newDate(
          dateAdapter.getFullYear(event.solarDate),
          dateAdapter.getMonth(event.solarDate),
          dateAdapter.getDate(event.solarDate),
        ); // Assuming single day events for now
      } else if (event.calendarType === CalendarType.Lunar && event.lunarDate) {
        const year = dateAdapter.getFullYear(selectedDate);
        eventStart = getSolarDateFromLunarDate(year, event.lunarDate, lunarDateAdapter, dateAdapter);
        eventEnd = getSolarDateFromLunarDate(year, event.lunarDate, lunarDateAdapter, dateAdapter); // Assuming single day events for now
      }

      if (!eventStart || !eventEnd) return null; // Skip events without valid start/end dates

      // Client-side filtering for the current month view
      const eventFallsInSelectedMonth =
        (eventStart >= startOfMonth && eventStart <= endOfMonth) ||
        (eventEnd >= startOfMonth && eventEnd <= endOfMonth) ||
        (eventStart < startOfMonth && eventEnd > endOfMonth); // Event spans across the month

      if (!eventFallsInSelectedMonth) {
        return null; // Filter out events not in the current month
      }

      const formattedEvent = {
        title: event.name,
        start: eventStart,
        end: eventEnd,
        color: event.color || 'primary',
        timed: true,
        eventObject: event,
      };
      return formattedEvent;
    })
    .filter((e) => e !== null); // Filter out null events
}

/**
 * Provides an array of weekdays for calendar display.
 */
export function getWeekdaysLogic(): number[] {
  return [0, 1, 2, 3, 4, 5, 6];
}

/**
 * Determines if a user can manage (add/edit) an event based on their roles and family permissions.
 */
export function canManageEventLogic(
  readOnly: boolean | undefined,
  isAdmin: boolean,
  isFamilyManager: (familyId: string) => boolean,
  familyId: string | undefined,
): boolean {
  return !readOnly && (isAdmin || isFamilyManager(familyId || ''));
}

/**
 * Returns the calendar title from the calendar reference.
 */
export function getCalendarTitleLogic(calendarRefValue: { title: string } | null): string {
  return calendarRefValue?.title || '';
}