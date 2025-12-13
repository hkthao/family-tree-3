import { ref, computed, watch, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import { useAuth } from '@/composables';
import { useUpcomingEvents } from '@/composables/data/useUpcomingEvents';

type CalendarEventColorFunction = (event: { [key: string]: any }) => string;

export function useEventCalendar(props: { familyId?: string; memberId?: string; readOnly?: boolean }, emit: (event: 'refetchEvents', ...args: any[]) => void) {
  const { t, locale } = useI18n();
  const { isAdmin, isFamilyManager } = useAuth();

  const { upcomingEvents: events, isLoading: loading, refetch: refetchEvents } = useUpcomingEvents(toRef(props, 'familyId'));

  const canAddEvent = computed(() => {
    return !props.readOnly && (isAdmin.value || isFamilyManager.value);
  });

  const canEditEvent = computed(() => {
    return !props.readOnly && (isAdmin.value || isFamilyManager.value);
  });

  const weekdays = computed(() => [0, 1, 2, 3, 4, 5, 6]);

  const selectedDate = ref(new Date());
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
  const calendarType = ref<
    | 'month'
    | 'week'
    | 'day'
    | '4day'
    | 'category'
    | 'custom-daily'
    | 'custom-weekly'
  >('month');
  const calendarTypes = computed(() => [
    { title: t('event.calendar.viewMode.month'), value: 'month' },
    { title: t('event.calendar.viewMode.week'), value: 'week' },
    { title: t('event.calendar.viewMode.day'), value: 'day' },
    { title: t('event.calendar.viewMode.4day'), value: '4day' },
  ]);

  const calendarTitle = computed(() => {
    if (calendarRef.value) {
      return calendarRef.value.title;
    }
    return '';
  });

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
      calendarRef.value.value = new Date();
    }
  };

  const formattedEvents = computed(() => {
    if (!events.value) return [];
    return events.value
      .filter((event: Event) => event.startDate)
      .map((event: Event) => ({
        title: event.name,
        start: new Date(event.startDate as Date),
        end: event.endDate
          ? new Date(event.endDate)
          : new Date(event.startDate as Date),
        color: event.color || 'primary',
        timed: true,
        eventObject: event,
      }));
  });

  const getEventColor: CalendarEventColorFunction = (event: {
    [key: string]: any;
  }) => {
    return event.color;
  };

  const showEventDetails = (eventSlotScope: Event) => {
    selectedEventId.value = eventSlotScope.id;
    if (canEditEvent.value) {
      editDrawer.value = true;
    } else {
      detailDrawer.value = true;
    }
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

  watch(
    selectedDate,
    (_newDate) => {
      // Logic here for calendar view changes if needed, but not data fetching
    },
    { immediate: true },
  );

  return {
    t,
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
    calendarTypes,
    calendarTitle,
    prev,
    next,
    setToday,
    formattedEvents,
    getEventColor,
    showEventDetails,
    handleEventSaved,
    handleEventClosed,
    handleAddSaved,
    handleAddClosed,
    handleDetailClosed,
    handleDetailEdit,
    loading,
  };
}
