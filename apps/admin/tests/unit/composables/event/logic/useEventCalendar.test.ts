// tests/unit/composables/event/logic/useEventCalendar.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuth } from '@/composables';
import { useUpcomingEvents } from '@/composables/event/useUpcomingEvents';
import { useEventCalendar } from '@/composables/event/logic/useEventCalendar';
import type { Event } from '@/types';
import { CalendarType } from '@/types/enums';
import type { DateAdapter, LunarDateAdapter, DayjsDateAdapter, LunarJsDateAdapter } from '@/composables/event/eventCalendar.adapter';
import * as eventCalendarLogic from '@/composables/event/logic/eventCalendar.logic';

// Mock Vue hooks and external dependencies
vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>();
  return {
    ...actual,
    watch: vi.fn(), // Mock watch to control execution
    ref: actual.ref,
    computed: actual.computed,
  };
});
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(),
}));
vi.mock('@/composables', () => ({
  useAuth: vi.fn(),
}));
vi.mock('@/composables/event/useUpcomingEvents', () => ({
  useUpcomingEvents: vi.fn(),
}));
vi.mock('@/composables/event/eventCalendar.adapter', () => ({
  DayjsDateAdapter: vi.fn(() => ({
    startOfMonth: vi.fn((d) => new Date(new Date(d).getFullYear(), new Date(d).getMonth(), 1)),
    endOfMonth: vi.fn((d) => new Date(new Date(d).getFullYear(), new Date(d).getMonth() + 1, 0)),
    getFullYear: vi.fn((d) => d.getFullYear()),
    getMonth: vi.fn((d) => d.getMonth()),
    getDate: vi.fn((d) => d.getDate()),
    newDate: vi.fn((y = 2024, m = 0, d = 15) => new Date(y, m, d)),
  })),
  LunarJsDateAdapter: vi.fn(() => ({
    fromYmd: vi.fn((y, m, d) => ({ getSolar: () => ({ getYear: () => y, getMonth: () => m, getDay: () => d }) })),
    fromSolar: vi.fn((_s) => ({ getDay: () => 1, getMonth: () => 1 })),
    getSolar: vi.fn((_l) => ({ getYear: () => 2024, getMonth: () => 1, getDay: () => 1 })),
  })),
}));
vi.mock('@/composables/event/logic/eventCalendar.logic', () => ({
  getSolarDateFromLunarDate: vi.fn(),
  getLunarDateForSolarDay: vi.fn(),
  getLunarDateRangeFiltersLogic: vi.fn(),
  getEventFilterLogic: vi.fn(),
  formatEventsForCalendarLogic: vi.fn(),
  getWeekdaysLogic: vi.fn(() => [0, 1, 2, 3, 4, 5, 6]),
  canManageEventLogic: vi.fn(),
  getCalendarTitleLogic: vi.fn(),
}));

describe('useEventCalendar', () => {
  let mockT: vi.Mock;
  let mockLocale: ReturnType<typeof ref>;
  let mockAuthIsAdmin: ReturnType<typeof ref>;
  let mockAuthIsFamilyManager: vi.Mock;
  let mockUpcomingEvents: ReturnType<typeof useUpcomingEvents>;
  let mockDateAdapter: DateAdapter;
  let mockLunarDateAdapter: LunarDateAdapter;
  let mockEmit: vi.Mock;

  const mockEvent: Event = {
    id: '1',
    name: 'Test Event',
    familyId: 'f1',
    type: 'Other',
    calendarType: CalendarType.Solar,
    solarDate: new Date('2024-01-01'),
    repeatRule: 'None',
    description: '',
    relatedMemberIds: [],
    color: '#000000',
  };

  beforeEach(() => {
    vi.clearAllMocks();

    mockT = vi.fn((key) => key);
    mockLocale = ref('en');
    vi.mocked(useI18n).mockReturnValue({ t: mockT, locale: mockLocale } as any);

    mockAuthIsAdmin = ref(false);
    mockAuthIsFamilyManager = vi.fn(() => false);
    vi.mocked(useAuth).mockReturnValue({
      state: {
        isAdmin: mockAuthIsAdmin,
        isFamilyManager: mockAuthIsFamilyManager,
      },
    } as any);

    mockUpcomingEvents = {
      upcomingEvents: ref([mockEvent]),
      isLoading: ref(false),
      refetch: vi.fn(),
      error: ref(null),
      isError: ref(false),
      isFetching: ref(false),
    };
    vi.mocked(useUpcomingEvents).mockReturnValue(mockUpcomingEvents);

    mockDateAdapter = new (vi.mocked(DayjsDateAdapter))();
    mockLunarDateAdapter = new (vi.mocked(LunarJsDateAdapter))();

    mockEmit = vi.fn();

    // Mock implementations for eventCalendarLogic functions
    vi.mocked(eventCalendarLogic.getLunarDateRangeFiltersLogic).mockReturnValue({ lunarStartDay: 1, lunarStartMonth: 1, lunarEndDay: 30, lunarEndMonth: 1 });
    vi.mocked(eventCalendarLogic.getEventFilterLogic).mockReturnValue({});
    vi.mocked(eventCalendarLogic.formatEventsForCalendarLogic).mockReturnValue([]);
    vi.mocked(eventCalendarLogic.canManageEventLogic).mockReturnValue(false);
    vi.mocked(eventCalendarLogic.getCalendarTitleLogic).mockReturnValue('');
    vi.mocked(eventCalendarLogic.getSolarDateFromLunarDate).mockReturnValue(new Date());
    vi.mocked(eventCalendarLogic.getLunarDateForSolarDay).mockReturnValue('1/1');
  });

  it('should initialize with default state', () => {
    const { state } = useEventCalendar({}, mockEmit);
    expect(state.selectedDate.value).toEqual(new Date(2024, 0, 15)); // Mocked newDate
    expect(state.editDrawer.value).toBe(false);
    expect(state.addDrawer.value).toBe(false);
    expect(state.detailDrawer.value).toBe(false);
    expect(state.selectedEventId.value).toBeNull();
    expect(state.isDatePickerOpen.value).toBe(false);
  });

  it('should call getLunarDateRangeFiltersLogic for lunarDateRangeFilters', () => {
    const { state } = useEventCalendar({}, mockEmit);
    // Access the computed property to trigger its evaluation
    state.selectedDate.value; // Access to trigger the computed
    expect(eventCalendarLogic.getLunarDateRangeFiltersLogic).toHaveBeenCalledWith(
      expect.any(Date), mockDateAdapter, mockLunarDateAdapter
    );
  });

  it('should call getEventFilterLogic for eventFilter', () => {
    const props = { familyId: 'f1', memberId: 'm1' };
    const { state } = useEventCalendar(props, mockEmit);
    // Access the computed property to trigger its evaluation
    state.selectedDate.value; // Access to trigger the computed
    expect(eventCalendarLogic.getEventFilterLogic).toHaveBeenCalledWith(
      expect.any(Date), props.familyId, props.memberId, expect.any(Object), mockDateAdapter
    );
  });

  it('should call canManageEventLogic for canAddEvent and canEditEvent', () => {
    const props = { readOnly: false, familyId: 'f1' };
    mockAuthIsAdmin.value = true; // Simulate admin
    const { state } = useEventCalendar(props, mockEmit);
    expect(state.canAddEvent.value).toBe(true);
    expect(state.canEditEvent.value).toBe(true);
    expect(eventCalendarLogic.canManageEventLogic).toHaveBeenCalledWith(
      props.readOnly, mockAuthIsAdmin.value, mockAuthIsFamilyManager, props.familyId
    );
    expect(eventCalendarLogic.canManageEventLogic).toHaveBeenCalledTimes(2);
  });

  it('should call getWeekdaysLogic for weekdays', () => {
    const { state } = useEventCalendar({}, mockEmit);
    expect(state.weekdays.value).toEqual([0, 1, 2, 3, 4, 5, 6]);
    expect(eventCalendarLogic.getWeekdaysLogic).toHaveBeenCalledTimes(1);
  });

  it('should call getCalendarTitleLogic for calendarTitle', () => {
    const { state } = useEventCalendar({}, mockEmit);
    state.calendarRef.value = { title: 'Mock Calendar Title', prev: vi.fn(), next: vi.fn(), value: new Date() };
    expect(state.calendarTitle.value).toBe('Mock Calendar Title');
    expect(eventCalendarLogic.getCalendarTitleLogic).toHaveBeenCalledWith(state.calendarRef.value);
  });

  it('should call formatEventsForCalendarLogic for formattedEvents', () => {
    const { state } = useEventCalendar({}, mockEmit);
    state.formattedEvents.value; // Access computed to trigger
    expect(eventCalendarLogic.formatEventsForCalendarLogic).toHaveBeenCalledWith(
      mockUpcomingEvents.upcomingEvents.value, expect.any(Date), mockDateAdapter, mockLunarDateAdapter
    );
  });

  it('should handle prev, next, setToday actions via calendarRef', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    const mockCalendarRef = {
      title: 'Mock', prev: vi.fn(), next: vi.fn(), value: new Date()
    };
    state.calendarRef.value = mockCalendarRef;

    actions.prev();
    expect(mockCalendarRef.prev).toHaveBeenCalledTimes(1);

    actions.next();
    expect(mockCalendarRef.next).toHaveBeenCalledTimes(1);

    actions.setToday();
    expect(mockCalendarRef.value).toEqual(new Date(2024, 0, 15)); // Mocked newDate
  });

  it('should call showEventDetails and set drawer state', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    actions.showEventDetails(mockEvent);
    expect(state.selectedEventId.value).toBe(mockEvent.id);
    expect(state.detailDrawer.value).toBe(true);
  });

  it('should handleDetailClosed and reset state', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    state.selectedEventId.value = 'some-id';
    state.detailDrawer.value = true;
    actions.handleDetailClosed();
    expect(state.selectedEventId.value).toBeNull();
    expect(state.detailDrawer.value).toBe(false);
  });

  it('should handleDetailEdit and set drawer state', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    actions.handleDetailEdit(mockEvent);
    expect(state.selectedEventId.value).toBe(mockEvent.id);
    expect(state.detailDrawer.value).toBe(false); // Should close detail drawer
    expect(state.editDrawer.value).toBe(true); // Should open edit drawer
  });

  it('should handleEventSaved and reset state, refetch, emit', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    state.editDrawer.value = true;
    state.selectedEventId.value = 'some-id';
    actions.handleEventSaved();
    expect(state.editDrawer.value).toBe(false);
    expect(state.selectedEventId.value).toBeNull();
    expect(mockUpcomingEvents.refetch).toHaveBeenCalledTimes(1);
    expect(mockEmit).toHaveBeenCalledWith('refetchEvents');
  });

  it('should handleEventClosed and reset state', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    state.editDrawer.value = true;
    state.selectedEventId.value = 'some-id';
    actions.handleEventClosed();
    expect(state.editDrawer.value).toBe(false);
    expect(state.selectedEventId.value).toBeNull();
  });

  it('should handleAddSaved and reset state, refetch, emit', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    state.addDrawer.value = true;
    actions.handleAddSaved();
    expect(state.addDrawer.value).toBe(false);
    expect(mockUpcomingEvents.refetch).toHaveBeenCalledTimes(1);
    expect(mockEmit).toHaveBeenCalledWith('refetchEvents');
  });

  it('should handleAddClosed and reset state', () => {
    const { actions, state } = useEventCalendar({}, mockEmit);
    state.addDrawer.value = true;
    actions.handleAddClosed();
    expect(state.addDrawer.value).toBe(false);
  });

  it('should call getLunarDateForSolarDay for getLunarDateForSolarDay action', () => {
    const { actions } = useEventCalendar({}, mockEmit);
    actions.getLunarDateForSolarDay(new Date());
    expect(eventCalendarLogic.getLunarDateForSolarDay).toHaveBeenCalledWith(
      expect.any(Date), mockLunarDateAdapter, mockDateAdapter
    );
  });

  it('should watch familyId and memberId changes and update selectedDate', () => {
    const props = ref({ familyId: 'f1', memberId: 'm1' });
    useEventCalendar(props.value, mockEmit);

    // Get the watch callback
    const watchCallback = vi.mocked(watch).mock.calls[0][1];

    // Simulate change
    watchCallback(['f2', 'm2'], ['f1', 'm1']);

    // Check if eventFilter logic (and thus useUpcomingEvents) was called with new filters
    // This implies that setFilters was called internally
    expect(eventCalendarLogic.getEventFilterLogic).toHaveBeenCalledWith(
      expect.any(Date), 'f2', 'm2', expect.any(Object), mockDateAdapter
    );
  });
});
