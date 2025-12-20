// tests/unit/composables/event/logic/eventForm.logic.test.ts
import { describe, it, expect, vi } from 'vitest';
import {
  getInitialEventFormData,
  getEventOptionTypes,
  getCalendarTypes,
  getRepeatRules,
  getLunarDays,
  getLunarMonths,
  processEventFormDataForSave,
} from '@/composables/event/logic/eventForm.logic';
import type { Event } from '@/types';
import { EventType } from '@/types';
import { CalendarType, RepeatRule } from '@/types/enums';
import { cloneDeep } from 'lodash'; // Required for testing processEventFormDataForSave

vi.mock('lodash', async (importOriginal) => {
  const actual = await importOriginal<typeof import('lodash')>();
  return {
    ...actual,
    cloneDeep: vi.fn((value) => JSON.parse(JSON.stringify(value))), // Simple deep copy mock
  };
});

describe('eventForm.logic', () => {
  const mockT = vi.fn((key) => key);

  describe('getInitialEventFormData', () => {
    it('should return initial data for a new event', () => {
      const formData = getInitialEventFormData({ familyId: 'testFamily' });
      expect(formData).toEqual({
        name: '',
        code: '',
        type: EventType.Other,
        familyId: 'testFamily',
        calendarType: CalendarType.Solar,
        solarDate: null,
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        color: '#1976D2',
        relatedMemberIds: [],
      });
    });

    it('should return cloned initialEventData if provided', () => {
      const initialEvent: Event = {
        id: '1',
        name: 'Existing Event',
        code: 'EV001',
        type: EventType.Birth,
        familyId: 'existingFamily',
        calendarType: CalendarType.Lunar,
        solarDate: null,
        lunarDate: { day: 10, month: 5, isLeapMonth: false },
        repeatRule: RepeatRule.Yearly,
        description: 'Desc',
        color: '#FF0000',
        relatedMemberIds: ['member1'],
      };
      const formData = getInitialEventFormData({ initialEventData: initialEvent });
      expect(formData).toEqual(initialEvent);
      expect(vi.mocked(cloneDeep)).toHaveBeenCalledWith(initialEvent);
    });

    it('should initialize lunarDate if initialEventData.lunarDate is null/undefined', () => {
      const initialEventWithNullLunarDate: Event = {
        id: '1',
        name: 'Existing Event',
        familyId: 'f1',
        type: EventType.Other,
        calendarType: CalendarType.Lunar,
        solarDate: null,
        lunarDate: null,
        repeatRule: RepeatRule.None,
        description: '',
        color: '#000000',
        relatedMemberIds: [],
      };
      const formData = getInitialEventFormData({ initialEventData: initialEventWithNullLunarDate });
      expect(formData.lunarDate).toEqual({ day: 1, month: 1, isLeapMonth: false });

      const initialEventWithUndefinedLunarDate: Event = {
        id: '1',
        name: 'Existing Event',
        familyId: 'f1',
        type: EventType.Other,
        calendarType: CalendarType.Lunar,
        solarDate: null,
        repeatRule: RepeatRule.None,
        description: '',
        color: '#000000',
        relatedMemberIds: [],
      } as unknown as Event; // Cast to bypass type checking for the test
      const formDataUndefined = getInitialEventFormData({ initialEventData: initialEventWithUndefinedLunarDate });
      expect(formDataUndefined.lunarDate).toEqual({ day: 1, month: 1, isLeapMonth: false });
    });
  });

  describe('getEventOptionTypes', () => {
    it('should return correct event types', () => {
      const types = getEventOptionTypes(mockT);
      expect(types).toHaveLength(4);
      expect(types[0]).toEqual({ title: 'event.type.birth', value: EventType.Birth });
      expect(mockT).toHaveBeenCalledWith('event.type.birth');
    });
  });

  describe('getCalendarTypes', () => {
    it('should return correct calendar types', () => {
      const types = getCalendarTypes(mockT);
      expect(types).toHaveLength(2);
      expect(types[0]).toEqual({ title: 'event.calendarType.solar', value: CalendarType.Solar });
      expect(mockT).toHaveBeenCalledWith('event.calendarType.solar');
    });
  });

  describe('getRepeatRules', () => {
    it('should return correct repeat rules', () => {
      const rules = getRepeatRules(mockT);
      expect(rules).toHaveLength(2);
      expect(rules[0]).toEqual({ title: 'event.repeatRule.none', value: RepeatRule.None });
      expect(mockT).toHaveBeenCalledWith('event.repeatRule.none');
    });
  });

  describe('getLunarDays', () => {
    it('should return an array of 30 lunar days', () => {
      const lunarDays = getLunarDays();
      expect(lunarDays).toHaveLength(30);
      expect(lunarDays[0]).toBe(1);
      expect(lunarDays[29]).toBe(30);
    });
  });

  describe('getLunarMonths', () => {
    it('should return an array of 12 lunar months', () => {
      const lunarMonths = getLunarMonths();
      expect(lunarMonths).toHaveLength(12);
      expect(lunarMonths[0]).toBe(1);
      expect(lunarMonths[11]).toBe(12);
    });
  });

  describe('processEventFormDataForSave', () => {
    it('should null out lunarDate if calendarType is Solar', () => {
      const event: Event = {
        id: '1',
        name: 'Event',
        familyId: 'f1',
        type: EventType.Other,
        calendarType: CalendarType.Solar,
        solarDate: new Date(),
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        relatedMemberIds: [],
        color: '#000000',
      };
      const processed = processEventFormDataForSave(event);
      expect(processed.solarDate).toEqual(event.solarDate);
      expect(processed.lunarDate).toBeNull();
      expect(vi.mocked(cloneDeep)).toHaveBeenCalledWith(event);
    });

    it('should null out solarDate if calendarType is Lunar', () => {
      const event: Event = {
        id: '1',
        name: 'Event',
        familyId: 'f1',
        type: EventType.Other,
        calendarType: CalendarType.Lunar,
        solarDate: new Date(),
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        relatedMemberIds: [],
        color: '#000000',
      };
      const processed = processEventFormDataForSave(event);
      expect(processed.lunarDate).toEqual({ day: 1, month: 1, isLeapMonth: false });
      expect(processed.solarDate).toBeNull();
      expect(vi.mocked(cloneDeep)).toHaveBeenCalledWith(event);
    });

    it('should not change dates if calendarType is neither Solar nor Lunar (e.g., undefined)', () => {
      const event: Event = {
        id: '1',
        name: 'Event',
        familyId: 'f1',
        type: EventType.Other,
        calendarType: undefined, // Simulating an unhandled type or initial state
        solarDate: new Date(),
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        relatedMemberIds: [],
        color: '#000000',
      };
      const processed = processEventFormDataForSave(event);
      expect(processed.solarDate).toEqual(event.solarDate);
      expect(processed.lunarDate).toEqual(event.lunarDate);
    });
  });
});
