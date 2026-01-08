import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventDto } from '@/types';
import { useAuth, type UseAuthReturn } from '@/composables/auth/useAuth';
import { type IEventService } from '@/services/event/event.service.interface';
import { DefaultEventServiceAdapter } from '../event.adapter';
import {
  type DateAdapter,
  DayjsDateAdapter,
  type LunarDateAdapter,
  LunarJsDateAdapter,
} from '@/composables/event/eventCalendar.adapter';
import {
  formatEventsForCalendarLogic,
  getWeekdaysLogic,
  canManageEventLogic,
  getCalendarTitleLogic,
  getLunarDateForSolarDay,
} from '@/composables/event/logic/eventCalendar.logic';
import { CalendarType, RepeatRule } from '@/types/enums';
import { EventType } from '@/types/event.d';

// Import Vietnamese holidays data
import vietnameseHolidays from '@/data/vietnamese_holidays.json';

interface HolidayEvent {
  name: string;
  description: string;
  calendarType: 'solar' | 'lunar';
  date?: string; // "MM-DD" for solar
  lunarDay?: number;
  lunarMonth?: number;
  isLeapMonth?: boolean;
  repeatRule: string; // "yearly"
  isEditable: boolean; // false
  type: string; // "National Holiday", etc.
  color: string;
}

const transformHolidayToEventDto = (
  holiday: HolidayEvent,
  currentYear: number,
  dateAdapter: DateAdapter,
): EventDto => {
  const baseEvent: EventDto = {
    id: `holiday-${holiday.name}-${currentYear}`, // Generate a unique ID for holidays
    name: holiday.name,
    code: '', // Holidays don't have a specific code
    description: holiday.description,
    familyId: null, // Global holidays are not tied to a specific family
    eventMembers: [],
    type: EventType.Other, // Map all custom holiday types to EventType.Other
    color: holiday.color,
    calendarType: holiday.calendarType === 'solar' ? CalendarType.Solar : CalendarType.Lunar,
    repeatRule: RepeatRule.Yearly, // Map "yearly" to RepeatRule.Yearly
    isPrivate: false, // Holidays are public
    // isEditable: holiday.isEditable, // This is a custom prop for UI, not in EventDto
  };

  if (holiday.calendarType === 'solar' && holiday.date) {
    const [month, day] = holiday.date.split('-').map(Number);
    baseEvent.solarDate = dateAdapter.newDate(currentYear, month - 1, day);
  } else if (holiday.calendarType === 'lunar' && holiday.lunarDay && holiday.lunarMonth) {
    baseEvent.lunarDate = {
      day: holiday.lunarDay,
      month: holiday.lunarMonth,
      isLeapMonth: holiday.isLeapMonth || false,
    };
  }
  return baseEvent;
};


interface UseEventCalendarDeps {
  useI18n: typeof useI18n;
  useAuth: () => UseAuthReturn;
  eventService: IEventService;
  dateAdapter: DateAdapter;
  lunarDateAdapter: LunarDateAdapter;
}

const defaultDeps: UseEventCalendarDeps = {
  useI18n,
  useAuth,
  eventService: DefaultEventServiceAdapter,
  dateAdapter: new DayjsDateAdapter(),
  lunarDateAdapter: new LunarJsDateAdapter(),
};

export function useEventCalendar(
  props: { familyId?: string; readOnly?: boolean },
  emit: (event: 'refetchEvents', ...args: any[]) => void,
  deps: UseEventCalendarDeps = defaultDeps,
) {
  const { t, locale } = deps.useI18n();
  const { state: authState } = deps.useAuth();
  const { dateAdapter, lunarDateAdapter, eventService } = deps;

  const selectedDate = ref(dateAdapter.newDate()); // Use dateAdapter for new Date()

  const events = ref<EventDto[]>([]);
  const loading = ref(false);

  const fetchEvents = async () => {
    if (!props.familyId) {
      events.value = [];
      return;
    }
    loading.value = true;
    try {
      const response = await eventService.getEventsByFamilyId(props.familyId);
      if (response.ok) {
        events.value = response.value;
      } else {
        console.error('Error fetching events:', response.error);
        events.value = [];
      }
    } catch (error) {
      console.error('Error fetching events:', error);
      events.value = [];
    } finally {
      loading.value = false;
    }
  };

  watch(() => props.familyId, fetchEvents, { immediate: true });

  const refetch = () => {
    fetchEvents();
  };

  const canAddEvent = computed(() =>
    canManageEventLogic(
      props.readOnly,
      authState.isAdmin.value,
      authState.isFamilyManager.value,
      props.familyId,
    ),
  );

  const canEditEvent = computed(() =>
    canManageEventLogic(
      props.readOnly,
      authState.isAdmin.value,
      authState.isFamilyManager.value,
      props.familyId,
    ),
  );

  const weekdays = computed(() => getWeekdaysLogic());

  const editDrawer = ref(false);
  const addDrawer = ref(false);
  const detailDrawer = ref(false);
  const selectedEventId = ref<string | null>(null);
  const selectedEventDtoForDetail = ref<EventDto | null>(null); // New ref for passing full EventDto
  const isDatePickerOpen = ref(false);
  const showFamilyEventsOnly = ref(false);

  const todayDate = computed(() => dateAdapter.newDate()); // Current date

  const calendarRef = ref<{
    title: string;
    prev: () => void;
    next: () => void;
    value: Date;
  } | null>(null);
  const calendarType = 'month' as const; // This is a constant string, not a dynamic type

  const calendarTitle = computed(() => getCalendarTitleLogic(calendarRef.value));

  const prev = () => {
    if (calendarRef.value) {
      calendarRef.value.prev();
    }
  };

  const next = () => {
    if (calendarRef.value) {
      calendarRef.value.next();
    }
  };

  const setToday = () => {
    if (calendarRef.value) {
      calendarRef.value.value = dateAdapter.newDate(); // Use dateAdapter for new Date()
    }
  };

  // Combine API events with static holidays
  const combinedEvents = computed<EventDto[]>(() => {
    const currentYear = dateAdapter.getFullYear(selectedDate.value);
    const transformedHolidays = vietnameseHolidays.map(holiday =>
      transformHolidayToEventDto(holiday as HolidayEvent, currentYear, dateAdapter)
    );

    if (showFamilyEventsOnly.value) {
      return events.value;
    }
    return [...events.value, ...transformedHolidays];
  });


  const formattedEvents = computed(() =>
    formatEventsForCalendarLogic(combinedEvents.value, selectedDate.value, dateAdapter, lunarDateAdapter),
  );

  const getEventColor = (event: { [key: string]: any }) => {
    return event.color;
  };

  const showEventDetails = (eventSlotScope: EventDto) => {
    selectedEventId.value = eventSlotScope.id;
    selectedEventDtoForDetail.value = eventSlotScope; // Set the full EventDto
    detailDrawer.value = true;
  };

  const handleEventSaved = () => {
    editDrawer.value = false;
    selectedEventId.value = null;
    selectedEventDtoForDetail.value = null;
    refetch(); // Refetch data
    emit('refetchEvents');
  };

  const handleEventClosed = () => {
    editDrawer.value = false;
    selectedEventId.value = null;
    selectedEventDtoForDetail.value = null;
  };

  const handleAddSaved = () => {
    addDrawer.value = false;
    refetch(); // Refetch data
    emit('refetchEvents');
  };

  const handleAddClosed = () => {
    addDrawer.value = false;
  };

  const handleDetailClosed = () => {
    detailDrawer.value = false;
    selectedEventId.value = null;
    selectedEventDtoForDetail.value = null; // Reset on close
  };

  const handleDetailEdit = (event: EventDto) => {
    detailDrawer.value = false;
    selectedEventId.value = event.id;
    selectedEventDtoForDetail.value = event; // Pass the event for edit
    editDrawer.value = true;
  };

  const getLunarDateForSolarDayAction = (solarDate: Date): string => {
    return getLunarDateForSolarDay(solarDate, lunarDateAdapter, dateAdapter);
  };

  const isToday = (date: Date): boolean => {
    return dateAdapter.isSameDay(date, todayDate.value);
  };

  // No need for watch(events, ...) and watch(formattedEvents, ...) as they were empty

  return {
    state: {
      locale,
      canAddEvent,
      canEditEvent,
      weekdays,
      selectedDate,
      editDrawer,
      addDrawer,
      detailDrawer,
      selectedEventId,
      selectedEventDtoForDetail, // Expose new ref
      isDatePickerOpen,
      showFamilyEventsOnly,
      calendarRef,
      calendarType,
      calendarTitle,
      formattedEvents,
      loading,
      events, // Expose events from ref
    },
    actions: {
      t,
      prev,
      next,
      setToday,
      getEventColor,
      showEventDetails,
      handleEventSaved,
      handleEventClosed,
      handleAddSaved,
      handleAddClosed,
      handleDetailClosed,
      handleDetailEdit,
      getLunarDateForSolarDay: getLunarDateForSolarDayAction,
      isToday, // Expose isToday function
      refetch, // Expose the new refetch function
    },
  };
}