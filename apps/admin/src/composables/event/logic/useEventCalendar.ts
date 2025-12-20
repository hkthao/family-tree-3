import { ref, computed, watch, type Ref, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event, LunarDate, EventFilter } from '@/types';
import { CalendarType } from '@/types/enums';
import { useAuth, type UseAuthReturn } from '@/composables/auth/useAuth';
import { useUpcomingEvents, type UseUpcomingEventsReturn } from '@/composables/event/useUpcomingEvents';
import {
  type DateAdapter,
  DayjsDateAdapter,
  type LunarDateAdapter,
  LunarJsDateAdapter,
} from '@/composables/event/eventCalendar.adapter';
import {
  getSolarDateFromLunarDate,
  getLunarDateForSolarDay,
  getLunarDateRangeFiltersLogic,
  getEventFilterLogic,
  formatEventsForCalendarLogic,
  getWeekdaysLogic,
  canManageEventLogic,
  getCalendarTitleLogic,
} from '@/composables/event/logic/eventCalendar.logic';

interface UseEventCalendarDeps {
  useI18n: typeof useI18n;
  useAuth: () => UseAuthReturn;
  useUpcomingEvents: (baseFilter: Ref<EventFilter> | ComputedRef<EventFilter>) => UseUpcomingEventsReturn;
  dateAdapter: DateAdapter;
  lunarDateAdapter: LunarDateAdapter;
}

const defaultDeps: UseEventCalendarDeps = {
  useI18n,
  useAuth,
  useUpcomingEvents,
  dateAdapter: new DayjsDateAdapter(),
  lunarDateAdapter: new LunarJsDateAdapter(),
};

export function useEventCalendar(
  props: { familyId?: string; memberId?: string; readOnly?: boolean },
  emit: (event: 'refetchEvents', ...args: any[]) => void,
  deps: UseEventCalendarDeps = defaultDeps,
) {
  const { t, locale } = deps.useI18n();
  const { state: authState } = deps.useAuth();
  const { dateAdapter, lunarDateAdapter } = deps;

  const selectedDate = ref(dateAdapter.newDate()); // Use dateAdapter for new Date()

  const lunarDateRangeFilters = computed(() =>
    getLunarDateRangeFiltersLogic(selectedDate.value, dateAdapter, lunarDateAdapter),
  );

  const eventFilter = computed(() =>
    getEventFilterLogic(
      selectedDate.value,
      props.familyId,
      props.memberId,
      lunarDateRangeFilters.value,
      dateAdapter,
    ),
  );

  const { upcomingEvents: events, isLoading: loading, refetch: refetchEvents } = deps.useUpcomingEvents(eventFilter);

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
  const isDatePickerOpen = ref(false);

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

  const formattedEvents = computed(() =>
    formatEventsForCalendarLogic(events.value, selectedDate.value, dateAdapter, lunarDateAdapter),
  );

  const getEventColor = (event: { [key: string]: any }) => {
    return event.color;
  };

  const showEventDetails = (eventSlotScope: Event) => {
    selectedEventId.value = eventSlotScope.id;
    detailDrawer.value = true;
  };

  const handleEventSaved = () => {
    editDrawer.value = false;
    selectedEventId.value = null;
    refetchEvents(); // Refetch data
    emit('refetchEvents');
  };

  const handleEventClosed = () => {
    editDrawer.value = false;
    selectedEventId.value = null;
  };

  const handleAddSaved = () => {
    addDrawer.value = false;
    refetchEvents(); // Refetch data
    emit('refetchEvents');
  };

  const handleAddClosed = () => {
    addDrawer.value = false;
  };

  const handleDetailClosed = () => {
    detailDrawer.value = false;
    selectedEventId.value = null;
  };

  const handleDetailEdit = (event: Event) => {
    detailDrawer.value = false;
    selectedEventId.value = event.id;
    editDrawer.value = true;
  };

  const getLunarDateForSolarDayAction = (solarDate: Date): string => {
    return getLunarDateForSolarDay(solarDate, lunarDateAdapter, dateAdapter);
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
      isDatePickerOpen,
      calendarRef,
      calendarType,
      calendarTitle,
      formattedEvents,
      loading,
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
    },
  };
}