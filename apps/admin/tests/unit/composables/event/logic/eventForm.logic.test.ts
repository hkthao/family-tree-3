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
import { EventType } from '@/types';
import { CalendarType, RepeatRule } from '@/types/enums';

describe('eventForm.logic', () => {
  // Mock translation function
  const mockTranslate = vi.fn((key: string) => key);

  describe('getInitialEventFormData', () => {
    it('should return default form data when no initialEventData is provided', () => {
      const result = getInitialEventFormData({});
      expect(result).toEqual({
        name: '',
        code: '',
        type: EventType.Other,
        familyId: null,
        calendarType: CalendarType.Solar,
        solarDate: null,
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        location: '', // ADD THIS
        color: '#1976D2',
        eventMemberIds: [],
      });
    });

    it('should return default form data with provided familyId', () => {
      const result = getInitialEventFormData({ familyId: 'testFamilyId' });
      expect(result.familyId).toBe('testFamilyId');
    });

    it('should return initial event data when provided', () => {
      const initialEvent = {
        id: '1',
        name: 'Existing EventDto',
        code: 'EVT001',
        type: EventType.Birth,
        familyId: 'fam1',
        calendarType: CalendarType.Lunar,
        solarDate: new Date('2023-01-01'),
        lunarDate: { day: 5, month: 3, isLeapMonth: false },
        repeatRule: RepeatRule.Yearly,
        description: 'Some description',
        location: 'Some location',
        color: '#000000',
        eventMemberIds: ['member1'],
      };
      const result = getInitialEventFormData({ initialEventData: initialEvent });
      expect(result).toEqual(initialEvent);
      expect(result.solarDate).toBeInstanceOf(Date);
    });

    it('should ensure lunarDate is always initialized with default values if missing in initialEventData', () => {
      const initialEvent = {
        id: '1',
        name: 'Existing EventDto',
        code: 'EVT001',
        type: EventType.Birth,
        familyId: 'fam1',
        calendarType: CalendarType.Lunar,
        solarDate: null,
        lunarDate: null, // Simulate missing lunarDate
        repeatRule: RepeatRule.Yearly,
        description: 'Some description',
        color: '#000000',
        eventMemberIds: ['member1'],
      };
      const result = getInitialEventFormData({ initialEventData: initialEvent });
      expect(result.lunarDate).toEqual({ day: 1, month: 1, isLeapMonth: false });
    });
  });

  describe('getEventOptionTypes', () => {
    it('should return an array of event types with translated titles and correct values', () => {
      const result = getEventOptionTypes(mockTranslate);
      expect(result).toEqual([
        { title: 'event.type.birth', value: EventType.Birth },
        { title: 'event.type.marriage', value: EventType.Marriage },
        { title: 'event.type.death', value: EventType.Death },
        { title: 'event.type.other', value: EventType.Other },
      ]);
      expect(mockTranslate).toHaveBeenCalledWith('event.type.birth');
      expect(mockTranslate).toHaveBeenCalledWith('event.type.marriage');
      expect(mockTranslate).toHaveBeenCalledWith('event.type.death');
      expect(mockTranslate).toHaveBeenCalledWith('event.type.other');
    });
  });

  describe('getCalendarTypes', () => {
    it('should return an array of calendar types with translated titles and correct values', () => {
      const result = getCalendarTypes(mockTranslate);
      expect(result).toEqual([
        { title: 'event.calendarType.solar', value: CalendarType.Solar },
        { title: 'event.calendarType.lunar', value: CalendarType.Lunar },
      ]);
      expect(mockTranslate).toHaveBeenCalledWith('event.calendarType.solar');
      expect(mockTranslate).toHaveBeenCalledWith('event.calendarType.lunar');
    });
  });

  describe('getRepeatRules', () => {
    it('should return an array of repeat rules with translated titles and correct values', () => {
      const result = getRepeatRules(mockTranslate);
      expect(result).toEqual([
        { title: 'event.repeatRule.none', value: RepeatRule.None },
        { title: 'event.repeatRule.yearly', value: RepeatRule.Yearly },
      ]);
      expect(mockTranslate).toHaveBeenCalledWith('event.repeatRule.none');
      expect(mockTranslate).toHaveBeenCalledWith('event.repeatRule.yearly');
    });
  });

  describe('getLunarDays', () => {
    it('should return an array of numbers from 1 to 30', () => {
      const result = getLunarDays();
      expect(result).toHaveLength(30);
      expect(result[0]).toBe(1);
      expect(result[29]).toBe(30);
    });
  });

  describe('getLunarMonths', () => {
    it('should return an array of numbers from 1 to 12', () => {
      const result = getLunarMonths();
      expect(result).toHaveLength(12);
      expect(result[0]).toBe(1);
      expect(result[11]).toBe(12);
    });
  });

  describe('processEventFormDataForSave', () => {
    it('should set lunarDate to null if calendarType is Solar', () => {
      const formData = {
        id: '1',
        name: 'Solar EventDto',
        code: 'SE001',
        type: EventType.Other,
        familyId: 'fam1',
        calendarType: CalendarType.Solar,
        solarDate: new Date('2023-01-01'),
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        color: '#000000',
        eventMemberIds: [],
      };
      const result = processEventFormDataForSave(formData);
      expect(result.solarDate).toEqual(new Date('2023-01-01'));
      expect(result.lunarDate).toBeNull();
    });

    it('should set solarDate to null if calendarType is Lunar', () => {
      const formData = {
        id: '1',
        name: 'Lunar EventDto',
        code: 'LE001',
        type: EventType.Other,
        familyId: 'fam1',
        calendarType: CalendarType.Lunar,
        solarDate: new Date('2023-01-01'),
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        color: '#000000',
        eventMemberIds: [],
      };
      const result = processEventFormDataForSave(formData);
      expect(result.solarDate).toBeNull();
      expect(result.lunarDate).toEqual({ day: 1, month: 1, isLeapMonth: false });
    });

    it('should convert solarDate string to Date object if it is a string', () => {
      const formData = {
        id: '1',
        name: 'Solar String Date EventDto',
        code: 'SSDE001',
        type: EventType.Other,
        familyId: 'fam1',
        calendarType: CalendarType.Solar,
        solarDate: '2023-05-10T00:00:00.000Z' as any, // Simulate string date
        lunarDate: null,
        repeatRule: RepeatRule.None,
        description: '',
        color: '#000000',
        eventMemberIds: [],
      };
      const result = processEventFormDataForSave(formData);
      expect(result.solarDate).toEqual(new Date('2023-05-10T00:00:00.000Z'));
      expect(result.solarDate).toBeInstanceOf(Date);
    });

    it('should clone the formData to avoid modifying the original object', () => {
      const formData = {
        id: '1',
        name: 'Original EventDto',
        code: 'OE001',
        type: EventType.Other,
        familyId: 'fam1',
        calendarType: CalendarType.Solar,
        solarDate: new Date('2023-01-01'),
        lunarDate: { day: 1, month: 1, isLeapMonth: false },
        repeatRule: RepeatRule.None,
        description: '',
        color: '#000000',
        eventMemberIds: [],
      };
      const originalFormData = { ...formData }; // Create a shallow copy to compare
      const result = processEventFormDataForSave(formData);

      // Ensure original object is not modified
      expect(formData).toEqual(originalFormData);
      expect(result).not.toBe(formData); // Check that it's a new object
    });
  });
});