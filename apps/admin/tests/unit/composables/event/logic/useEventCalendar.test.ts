import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { ref, defineComponent, computed } from 'vue'; // Added computed
import { useEventCalendar } from '@/composables/event/logic/useEventCalendar';
import type { EventDto } from '@/types';
import { CalendarType, RepeatRule, EventType } from '@/types';
import type { IEventService } from '@/services/event/event.service.interface';
import type { DateAdapter, LunarDateAdapter } from '@/composables/event/eventCalendar.adapter';

// Mock dependencies
import type { Composer } from 'vue-i18n';
import type { Mock } from 'vitest';

const mockT = vi.fn((key: string) => key); // Mock i18n translation function
const mockLocale = ref('en'); // Mock i18n locale
const mockUseI18n = vi.fn(() => ({
  t: mockT,
  locale: mockLocale,
})) as any; // Cast to any to bypass strict Composer typing for simple mock

const mockIsAdmin = ref(false);
const mockIsFamilyManagerFn = vi.fn((familyId: string) => familyId === 'family1');
const mockIsFamilyManager = ref(mockIsFamilyManagerFn); // It's a ref holding a function
const mockUseAuth = vi.fn(() => ({
  state: {
    isLoggedIn: computed(() => true), // Added
    userRoles: computed(() => []), // Added
    isAdmin: computed(() => mockIsAdmin.value), // Adjusted to ComputedRef<boolean>
    isFamilyManager: computed(() => mockIsFamilyManager.value), // Adjusted to ComputedRef<Function>
    currentUser: computed(() => ({
      id: 'user1',
      name: 'Test User',
      externalId: 'ext1',
      email: 'test@example.com',
    })), // More complete mock for User
    userFamilyAccess: computed(() => []), // Changed to an empty array
  },
  actions: { // Add actions to mockUseAuth
    hasRole: vi.fn(),
    hasFamilyRole: vi.fn(),
  },
}));

const mockEventService: IEventService = {
  add: vi.fn() as Mock,
  update: vi.fn() as Mock,
  delete: vi.fn() as Mock,
  getById: vi.fn() as Mock,
  search: vi.fn() as Mock,
  getEventsByFamilyId: vi.fn() as Mock, // Explicitly cast here
  getByIds: vi.fn() as Mock,
  exportEvents: vi.fn() as Mock,
  importEvents: vi.fn() as Mock,
};

const mockDateAdapter = {
  newDate: vi.fn((year?: number, month?: number, day?: number) => {
    if (year !== undefined && month !== undefined && day !== undefined) {
      return new Date(year, month, day);
    }
    return new Date('2024-01-15T12:00:00.000Z'); // Fixed date for testing
  }) as Mock,
  getFullYear: vi.fn((date: Date) => date.getFullYear()) as Mock,
  getMonth: vi.fn((date: Date) => date.getMonth()) as Mock,
  getDate: vi.fn((date: Date) => date.getDate()) as Mock,
  startOfMonth: vi.fn((date: Date) => new Date(date.getFullYear(), date.getMonth(), 1)) as Mock,
  endOfMonth: vi.fn((date: Date) => new Date(date.getFullYear(), date.getMonth() + 1, 0)) as Mock,
  isSameDay: vi.fn((date1: Date, date2: Date) => date1.toDateString() === date2.toDateString()) as Mock, // ADDED isSameDay
};

const mockLunarDateAdapter: LunarDateAdapter = { // Add as LunarDateAdapter
  lunarFromYmd: vi.fn((year, month, day) => ({
    getYear: () => year,
    getMonth: () => month,
    getDay: () => day,
  })) as Mock,
  getSolar: vi.fn((lunar) => ({
    getYear: () => lunar.getYear(),
    getMonth: () => lunar.getMonth(),
    getDay: () => lunar.getDay(),
  })) as Mock,
  solarFromYmd: vi.fn((year, month, day) => ({
    getLunar: () => ({
      getYear: () => year,
      getMonth: () => month,
      getDay: () => day,
    }),
  })) as Mock,
  fromSolar: vi.fn((solar) => ({
    getYear: () => solar.getYear(),
    getMonth: () => solar.getMonth(),
    getDay: () => solar.getDay(),
  })) as Mock,
  getLunarDaysInMonth: vi.fn((year, month) => 30) as Mock, // Default to 30 days
} as LunarDateAdapter; // Explicitly cast here


const TestComponent = defineComponent({
  props: ['familyId', 'readOnly'],
  setup(props, { emit }) {
    return useEventCalendar(props as any, emit, {
      useI18n: mockUseI18n,
      useAuth: mockUseAuth,
      eventService: mockEventService,
      dateAdapter: mockDateAdapter as DateAdapter,
      lunarDateAdapter: mockLunarDateAdapter as LunarDateAdapter,
    });
  },
});

describe('useEventCalendar', () => {
  const emit = vi.fn();
  const mockFamilyId = 'family1';
  const mockEventDto: EventDto = {
    id: 'event1',
    name: 'Family Event',
    code: 'FE001',
    type: EventType.Other,
    familyId: mockFamilyId,
    calendarType: CalendarType.Solar,
    solarDate: new Date('2024-01-20T12:00:00.000Z'),
    repeatRule: RepeatRule.None,
    isPrivate: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    (mockEventService.getEventsByFamilyId as Mock<any>).mockResolvedValue({ ok: true, value: [mockEventDto] });
    mockIsAdmin.value = false;
    mockUseI18n.mockClear();
    mockUseAuth.mockClear();
    emit.mockClear();
  });

  it('should initialize with correct default state', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId }, global: { provide: { useI18n: mockUseI18n } } });
    const { state } = wrapper.vm;

    expect(state.loading.value).toBe(true); // Should be loading initially
    await vi.dynamicImportSettled(); // Wait for immediate watch effect to settle
    expect(state.selectedDate.value).toEqual(mockDateAdapter.newDate());
    expect(state.events.value).toEqual([mockEventDto]);
    expect(state.loading.value).toBe(false);
    expect(mockEventService.getEventsByFamilyId).toHaveBeenCalledWith(mockFamilyId);
  });

  it('should fetch events when familyId is provided', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(mockEventService.getEventsByFamilyId).toHaveBeenCalledWith(mockFamilyId);
    expect(state.events.value).toEqual([mockEventDto]);
    expect(state.loading.value).toBe(false);
  });

  it('should not fetch events if familyId is not provided', async () => {
    const wrapper = mount(TestComponent, { props: {} });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(mockEventService.getEventsByFamilyId).not.toHaveBeenCalled();
    expect(state.events.value).toEqual([]);
    expect(state.loading.value).toBe(false);
  });

  it('should handle error when fetching events', async () => {
    (mockEventService.getEventsByFamilyId as Mock<any>).mockResolvedValue({ ok: false, error: new Error('Failed to fetch') });
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(mockEventService.getEventsByFamilyId).toHaveBeenCalledWith(mockFamilyId);
    expect(state.events.value).toEqual([]);
    expect(state.loading.value).toBe(false);
  });

  it('should combine API events with holiday events', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    const combined = state.formattedEvents.value;
    expect(combined.length).toBeGreaterThan(1); // Should have API event + holidays
    expect(combined.some(e => e.eventObject.id === mockEventDto.id)).toBe(true); // API event is present
    expect(combined.some(e => e.eventObject.id.startsWith('holiday-'))).toBe(true); // Holidays are present
  });

  it('should correctly transform solar holiday events', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    const tetDuongLich = state.formattedEvents.value.find(
      e => e.eventObject.name === 'Tết Dương lịch',
    );

    expect(tetDuongLich).toBeDefined();
    expect(tetDuongLich.eventObject.calendarType).toBe(CalendarType.Solar);
    expect(mockDateAdapter.getFullYear(tetDuongLich.eventObject.solarDate)).toBe(
      mockDateAdapter.getFullYear(state.selectedDate.value),
    );
    expect(mockDateAdapter.getMonth(tetDuongLich.eventObject.solarDate) + 1).toBe(1); // January
    expect(mockDateAdapter.getDate(tetDuongLich.eventObject.solarDate)).toBe(1); // 1st
    expect(tetDuongLich.eventObject.repeatRule).toBe(RepeatRule.Yearly);
    expect(tetDuongLich.eventObject.type).toBe(EventType.Other);
  });

  it('should correctly transform lunar holiday events', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    const tetNguyenDan = state.formattedEvents.value.find(
      e => e.eventObject.name === 'Tết Nguyên Đán',
    );

    expect(tetNguyenDan).toBeDefined();
    expect(tetNguyenDan.eventObject.calendarType).toBe(CalendarType.Lunar);
    expect(tetNguyenDan.eventObject.lunarDate.day).toBe(1);
    expect(tetNguyenDan.eventObject.lunarDate.month).toBe(1);
    expect(tetNguyenDan.eventObject.repeatRule).toBe(RepeatRule.Yearly);
    expect(tetNguyenDan.eventObject.type).toBe(EventType.Other);
  });

  it('should populate selectedEventDtoForDetail and open detailDrawer for regular event', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state, actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    actions.showEventDetails(mockEventDto);

    expect(state.selectedEventId.value).toBe(mockEventDto.id);
    expect(state.selectedEventDtoForDetail.value).toEqual(mockEventDto);
    expect(state.detailDrawer.value).toBe(true);
  });

  it('should populate selectedEventDtoForDetail and open detailDrawer for holiday event', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state, actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    const tetNguyenDan = state.formattedEvents.value.find(
      e => e.eventObject.name === 'Tết Nguyên Đán',
    );
    expect(tetNguyenDan).toBeDefined();

    actions.showEventDetails(tetNguyenDan.eventObject);

    expect(state.selectedEventId.value).toBe(tetNguyenDan.eventObject.id);
    expect(state.selectedEventDtoForDetail.value).toEqual(tetNguyenDan.eventObject);
    expect(state.detailDrawer.value).toBe(true);
  });

  it('should navigate to previous month when prev is called', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    const mockCalendarRef = { prev: vi.fn(), next: vi.fn(), value: new Date(), title: 'Mock Title' };
    wrapper.vm.state.calendarRef.value = mockCalendarRef;

    actions.prev();
    expect(mockCalendarRef.prev).toHaveBeenCalled();
  });

  it('should navigate to next month when next is called', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    const mockCalendarRef = { prev: vi.fn(), next: vi.fn(), value: new Date(), title: 'Mock Title' };
    wrapper.vm.state.calendarRef.value = mockCalendarRef;

    actions.next();
    expect(mockCalendarRef.next).toHaveBeenCalled();
  });

  it('should set today when setToday is called', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    const mockCalendarRef = { prev: vi.fn(), next: vi.fn(), value: new Date(), title: 'Mock Title' };
    wrapper.vm.state.calendarRef.value = mockCalendarRef;
    mockDateAdapter.newDate.mockReturnValueOnce(new Date('2024-03-01T12:00:00.000Z')); // Mock today's date

    actions.setToday();
    expect(mockCalendarRef.value).toEqual(new Date('2024-03-01T12:00:00.000Z'));
  });

  it('should allow adding event if not readOnly and is an admin', async () => {
    mockIsAdmin.value = true;
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(state.canAddEvent.value).toBe(true);
  });

  it('should allow adding event if not readOnly and is a family manager', async () => {
    mockIsAdmin.value = false;
    mockIsFamilyManager.value.mockReturnValueOnce(true); // Mock the function inside the ref
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(state.canAddEvent.value).toBe(true);
  });

  it('should not allow adding event if readOnly is true', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId, readOnly: true } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(state.canAddEvent.value).toBe(false);
  });

  it('should not allow adding event if not admin and not family manager', async () => {
    mockIsAdmin.value = false;
    mockIsFamilyManager.value.mockReturnValueOnce(false); // Mock the function inside the ref
    const wrapper = mount(TestComponent, { props: { familyId: 'otherFamily' } });
    const { state } = wrapper.vm;
    await vi.dynamicImportSettled();

    expect(state.canAddEvent.value).toBe(false);
  });

  it('should emit refetchEvents and close drawer on handleEventSaved', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state, actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    state.editDrawer.value = true;
    state.selectedEventId.value = 'event1';
    actions.handleEventSaved();
    await vi.dynamicImportSettled();

    expect(state.editDrawer.value).toBe(false);
    expect(state.selectedEventId.value).toBe(null);
    expect(state.selectedEventDtoForDetail.value).toBe(null);
    expect(wrapper.emitted().refetchEvents).toHaveLength(1); // Check emitted events
    expect(mockEventService.getEventsByFamilyId).toHaveBeenCalledTimes(2); // Initial fetch + refetch
  });

  it('should emit refetchEvents and close drawer on handleAddSaved', async () => {
    const wrapper = mount(TestComponent, { props: { familyId: mockFamilyId } });
    const { state, actions } = wrapper.vm;
    await vi.dynamicImportSettled();

    state.addDrawer.value = true;
    actions.handleAddSaved();
    await vi.dynamicImportSettled();

    expect(state.addDrawer.value).toBe(false);
    expect(wrapper.emitted().refetchEvents).toHaveLength(1); // Check emitted events
    expect(mockEventService.getEventsByFamilyId).toHaveBeenCalledTimes(2); // Initial fetch + refetch
  });
});