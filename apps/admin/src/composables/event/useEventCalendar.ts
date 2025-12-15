import { ref, computed, watch, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event, LunarDate } from '@/types'; // Import LunarDate
import { CalendarType } from '@/types/enums'; // Import CalendarType
import { useAuth } from '@/composables';
import { useUpcomingEvents } from '@/composables/data/useUpcomingEvents';

type CalendarEventColorFunction = (event: { [key: string]: any }) => string;

// Helper function to convert LunarDate to a rough Solar Date for display purposes
// This is a simplified conversion and might not be perfectly accurate for all lunar dates
const getSolarDateFromLunarDate = (lunarDate: LunarDate): Date => {
  // This is a very rough estimation. A proper conversion would require a more complex algorithm.
  // For now, we'll assume a fixed offset or just return a placeholder date for the current year.
  // In a real app, you'd integrate a proper lunar-to-solar calendar conversion library.
  const currentYear = new Date().getFullYear();
  return new Date(currentYear, lunarDate.month - 1, lunarDate.day);
};

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
      .map((event: Event) => {
        let eventStart: Date | null = null;
        let eventEnd: Date | null = null;

        if (event.calendarType === CalendarType.Solar && event.solarDate) {
          eventStart = new Date(event.solarDate);
          eventEnd = new Date(event.solarDate); // Assuming single day events for now
        } else if (event.calendarType === CalendarType.Lunar && event.lunarDate) {
          eventStart = getSolarDateFromLunarDate(event.lunarDate);
          eventEnd = getSolarDateFromLunarDate(event.lunarDate); // Assuming single day events for now
        }

        if (!eventStart) return null; // Skip events without a valid start date

        return {
          title: event.name,
          start: eventStart,
          end: eventEnd,
          color: event.color || 'primary',
          timed: true,
          eventObject: event,
        };
      })
      .filter((e) => e !== null); // Filter out null events
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