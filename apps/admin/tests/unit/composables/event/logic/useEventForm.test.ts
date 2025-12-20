// tests/unit/composables/event/logic/useEventForm.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useI18n } from 'vue-i18n';
import { useEventForm } from '@/composables/event/logic/useEventForm';
import type { Event } from '@/types';
import { CalendarType, EventType, RepeatRule } from '@/types/enums';
import { useEventRules } from '@/validations/event.validation';
import * as eventFormLogic from '@/composables/event/logic/eventForm.logic'; // Import logic module to mock

// Mock external dependencies
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(),
}));
vi.mock('@/validations/event.validation', () => ({
  useEventRules: vi.fn(),
}));
vi.mock('lodash', () => ({
  cloneDeep: vi.fn((value) => JSON.parse(JSON.stringify(value))),
}));

// Mock logic functions
vi.mock('@/composables/event/logic/eventForm.logic', () => ({
  getInitialEventFormData: vi.fn(),
  getEventOptionTypes: vi.fn(),
  getCalendarTypes: vi.fn(),
  getRepeatRules: vi.fn(),
  getLunarDays: vi.fn(() => Array.from({ length: 30 }, (_, i) => i + 1)),
  getLunarMonths: vi.fn(() => Array.from({ length: 12 }, (_, i) => i + 1)),
  processEventFormDataForSave: vi.fn(),
}));

describe('useEventForm', () => {
  let mockT: vi.Mock;
  let mockUseEventRules: vi.Mock;
  let mockCloneDeep: vi.Mock;
  let mockFormRefValidate: vi.Mock;

  const mockEvent: Event = {
    id: 'event1',
    name: 'Test Event',
    description: 'Description',
    familyId: 'family1',
    type: EventType.Other,
    calendarType: CalendarType.Solar,
    solarDate: new Date('2024-01-01'),
    lunarDate: null,
    repeatRule: RepeatRule.None,
    relatedMemberIds: [],
    color: '#000000',
  };

  beforeEach(() => {
    vi.clearAllMocks();

    mockT = vi.fn((key) => key);
    vi.mocked(useI18n).mockReturnValue({ t: mockT } as any);

    mockUseEventRules = vi.fn(() => ({
      nameRules: [],
      familyIdRules: [],
      calendarTypeRules: [],
      solarDateRules: [],
      lunarDayRules: [],
      lunarMonthRules: [],
      repeatRuleRules: [],
      typeRules: [],
    }));
    vi.mocked(useEventRules).mockImplementation(mockUseEventRules);

    mockCloneDeep = vi.mocked(cloneDeep);
    // Ensure cloneDeep from lodash returns a deep copy for internal logic
    mockCloneDeep.mockImplementation((value) => JSON.parse(JSON.stringify(value)));

    mockFormRefValidate = vi.fn();

    // Mock logic functions
    vi.mocked(eventFormLogic.getInitialEventFormData).mockReturnValue(mockEvent);
    vi.mocked(eventFormLogic.getEventOptionTypes).mockReturnValue([{ title: 'Mock Type', value: 'Mock' }]);
    vi.mocked(eventFormLogic.getCalendarTypes).mockReturnValue([{ title: 'Mock Calendar', value: 'Mock' }]);
    vi.mocked(eventFormLogic.getRepeatRules).mockReturnValue([{ title: 'Mock Repeat', value: 'Mock' }]);
    vi.mocked(eventFormLogic.getLunarDays).mockReturnValue([1, 2, 3]);
    vi.mocked(eventFormLogic.getLunarMonths).mockReturnValue([1, 2, 3]);
    vi.mocked(eventFormLogic.processEventFormDataForSave).mockImplementation((data) => data);
  });

  it('should initialize formData using getInitialEventFormData', () => {
    const props = { familyId: 'f1' };
    const { state } = useEventForm(props);
    expect(eventFormLogic.getInitialEventFormData).toHaveBeenCalledWith(props);
    expect(state.formData.name).toBe(mockEvent.name);
  });

  it('should compute eventOptionTypes using getEventOptionTypes', () => {
    const props = { familyId: 'f1' };
    const { state } = useEventForm(props);
    expect(eventFormLogic.getEventOptionTypes).toHaveBeenCalledWith(mockT);
    expect(state.eventOptionTypes.value).toEqual([{ title: 'Mock Type', value: 'Mock' }]);
  });

  it('should compute calendarTypes using getCalendarTypes', () => {
    const props = { familyId: 'f1' };
    const { state } = useEventForm(props);
    expect(eventFormLogic.getCalendarTypes).toHaveBeenCalledWith(mockT);
    expect(state.calendarTypes.value).toEqual([{ title: 'Mock Calendar', value: 'Mock' }]);
  });

  it('should compute repeatRules using getRepeatRules', () => {
    const props = { familyId: 'f1' };
    const { state } = useEventForm(props);
    expect(eventFormLogic.getRepeatRules).toHaveBeenCalledWith(mockT);
    expect(state.repeatRules.value).toEqual([{ title: 'Mock Repeat', value: 'Mock' }]);
  });

  it('should compute lunarDays using getLunarDays', () => {
    const props = { familyId: 'f1' };
    const { state } = useEventForm(props);
    expect(eventFormLogic.getLunarDays).toHaveBeenCalled();
    expect(state.lunarDays.value).toEqual([1, 2, 3]);
  });

  it('should compute lunarMonths using getLunarMonths', () => {
    const props = { familyId: 'f1' };
    const { state } = useEventForm(props);
    expect(eventFormLogic.getLunarMonths).toHaveBeenCalled();
    expect(state.lunarMonths.value).toEqual([1, 2, 3]);
  });

  it('should call useEventRules with correct state', () => {
    const props = { familyId: 'f1' };
    useEventForm(props);
    expect(mockUseEventRules).toHaveBeenCalledTimes(1);
    // Check arguments passed to useEventRules
    const passedState = mockUseEventRules.mock.calls[0][0];
    expect(passedState.name.value).toBe(mockEvent.name);
    expect(passedState.familyId.value).toBe(mockEvent.familyId);
  });

  it('should validate form via formRef.value.validate', async () => {
    const props = { familyId: 'f1' };
    const { formRef, actions } = useEventForm(props);
    formRef.value = { validate: mockFormRefValidate.mockResolvedValue({ valid: true }) };

    const isValid = await actions.validate();
    expect(mockFormRefValidate).toHaveBeenCalledTimes(1);
    expect(isValid).toBe(true);
  });

  it('should return processed form data via getFormData', () => {
    const props = { familyId: 'f1' };
    const { actions } = useEventForm(props);
    actions.getFormData();
    expect(eventFormLogic.processEventFormDataForSave).toHaveBeenCalledWith(expect.objectContaining({ name: mockEvent.name }));
  });
});
