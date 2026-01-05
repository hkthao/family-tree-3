// src/composables/event/logic/eventForm.logic.ts
import type { EventDto, AddEventDto, UpdateEventDto } from '@/types';
import type { LunarDate } from '@/types/lunar-date';
import { EventType } from '@/types'; // Changed to type import
import { CalendarType, RepeatRule } from '@/types/enums';
import { cloneDeep } from 'lodash';

// Define a type for the translation function
type TranslateFunction = (key: string) => string;

interface GetInitialEventFormDataProps {
  initialEventData?: EventDto | null;
  familyId?: string;
}

/**
 * Generates the initial form data for an event.
 * @param props - Properties including initial event data and family ID.
 * @returns Initial event form data.
 */
export function getInitialEventFormData(props: GetInitialEventFormDataProps): AddEventDto | UpdateEventDto {
  if (props.initialEventData) {
    // This is an edit operation, return UpdateEventDto
    const updateData: UpdateEventDto = {
      ...cloneDeep(props.initialEventData),
      lunarDate: props.initialEventData.lunarDate ?? ({ day: 1, month: 1, isLeapMonth: false } as LunarDate),
      location: props.initialEventData.location,
      locationId: props.initialEventData.locationId, // Added
    };
    return updateData;
  } else {
    // This is an add operation, return AddEventDto
    const addData: AddEventDto = {
      name: '',
      code: '',
      type: EventType.Other,
      familyId: props.familyId || null,
      calendarType: CalendarType.Solar,
      solarDate: null,
      lunarDate: { day: 1, month: 1, isLeapMonth: false } as LunarDate,
      repeatRule: RepeatRule.None,
      description: '',
      location: '',
      locationId: null, // Added
      color: '#1976D2',
      eventMemberIds: [],
    };
    return addData;
  }
}

/**
 * Generates event type options for a select input.
 * @param t - Translation function.
 * @returns Array of event type options.
 */
export function getEventOptionTypes(t: TranslateFunction) {
  return [
    { title: t('event.type.birth'), value: EventType.Birth },
    { title: t('event.type.marriage'), value: EventType.Marriage },
    { title: t('event.type.death'), value: EventType.Death },
    { title: t('event.type.other'), value: EventType.Other },
  ];
}

/**
 * Generates calendar type options for a select input.
 * @param t - Translation function.
 * @returns Array of calendar type options.
 */
export function getCalendarTypes(t: TranslateFunction) {
  return [
    { title: t('event.calendarType.solar'), value: CalendarType.Solar },
    { title: t('event.calendarType.lunar'), value: CalendarType.Lunar },
  ];
}

/**
 * Generates repeat rule options for a select input.
 * @param t - Translation function.
 * @returns Array of repeat rule options.
 */
export function getRepeatRules(t: TranslateFunction) {
  return [
    { title: t('event.repeatRule.none'), value: RepeatRule.None },
    { title: t('event.repeatRule.yearly'), value: RepeatRule.Yearly },
  ];
}

/**
 * Generates an array of lunar days (1-30).
 * @returns Array of numbers representing lunar days.
 */
export function getLunarDays(): number[] {
  return Array.from({ length: 30 }, (_, i) => i + 1);
}

/**
 * Generates an array of lunar months (1-12).
 * @returns Array of numbers representing lunar months.
 */
export function getLunarMonths(): number[] {
  return Array.from({ length: 12 }, (_, i) => i + 1);
}

/**
 * Processes event form data for saving, nulling out irrelevant date fields.
 * @param formData - The current form data.
 * @returns Processed event data.
 */
export function processEventFormDataForSave(formData: AddEventDto | UpdateEventDto): AddEventDto | UpdateEventDto {
  const data = cloneDeep(formData);

  // If solarDate is a string, convert it back to a Date object
  if (typeof data.solarDate === 'string') {
    data.solarDate = new Date(data.solarDate);
  }

  if (data.calendarType === CalendarType.Solar) {
    data.lunarDate = null;
  } else if (data.calendarType === CalendarType.Lunar) {
    data.solarDate = null;
  }
  return data;
}
