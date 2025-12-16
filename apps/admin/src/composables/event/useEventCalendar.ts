import { ref, computed, watch, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event, LunarDate } from '@/types'; // Import LunarDate
import { CalendarType } from '@/types/enums'; // Import CalendarType
import { useAuth } from '@/composables';
import { useUpcomingEvents } from '@/composables/data/useUpcomingEvents';
import { Solar, Lunar } from 'lunar-javascript'; // Import lunar-javascript library

type CalendarEventColorFunction = (event: { [key: string]: any }) => string;

// Helper function to convert LunarDate to a Solar Date using lunar-javascript library.
// NOTE: The LunarDate interface (src/types/lunar-date.d.ts) does not currently include a 'year' property.
// For accurate conversion, a year is crucial for lunar-to-solar calculation.
// As a workaround, the current solar year is used as the lunar year for conversion.
// This means the conversion is accurate for lunar events in the *current solar year* only.
// If historical or future lunar events are needed, the LunarDate interface must be updated to include a 'year'.
const getSolarDateFromLunarDate = (lunarDate: LunarDate): Date => {
  try {
    const currentSolarYear = new Date().getFullYear();
    const lunar = Lunar.fromYmd(currentSolarYear, lunarDate.month, lunarDate.day, lunarDate.isLeapMonth);
    const solar = lunar.getSolar();
    return new Date(solar.year, solar.month - 1, solar.day);
  } catch (e) {
    console.error('Error during lunar to solar conversion using lunar-javascript:', e);
    // Fallback to the old rough estimation if conversion fails
    const currentYear = new Date().getFullYear();
    return new Date(currentYear, lunarDate.month - 1, lunarDate.day);
  }
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
  const calendarType: 'month' = 'month';

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
    // Since calendarType is now fixed to 'month', always open detail drawer
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

  watch(
    selectedDate,
    (_newDate) => {
      // Logic here for calendar view changes if needed, but not data fetching
    },
    { immediate: true },
  );

  const getLunarDateForSolarDay = (solarDate: Date): string => {
    if (!(solarDate instanceof Date) || isNaN(solarDate.getTime())) {
      return 'Ngày không hợp lệ (Âm)';
    }

    try {
      const year = solarDate.getFullYear();
      const month = solarDate.getMonth() + 1; // getMonth() is 0-indexed
      const day = solarDate.getDate();
      const solar = Solar.fromYmd(year, month, day); // Corrected usage
      const lunar = Lunar.fromSolar(solar); // Corrected conversion method
      return `${lunar.getDay()}/${lunar.getMonth()}`;
    } catch (e) {
      console.error('Error during solar to lunar conversion:', e);
      return 'Lỗi chuyển đổi (Âm)';
    }
  }; return {
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
    getLunarDateForSolarDay,
  };
}