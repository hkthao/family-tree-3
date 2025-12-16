import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import dayjs from 'dayjs'; // Import dayjs
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
const getSolarDateFromLunarDate = (year: number, lunarDate: LunarDate): Date => {
  try {
    const lunar = Lunar.fromYmd(year, lunarDate.month, lunarDate.day);
    const solar = lunar.getSolar();
    const resultDate = new Date(solar.getYear(), solar.getMonth() - 1, solar.getDay());
    return resultDate;
  } catch (e) {
    console.error('Error during lunar to solar conversion using lunar-javascript:', e);
    return new Date(year, lunarDate.month - 1, lunarDate.day);
  }
};

export function useEventCalendar(props: { familyId?: string; memberId?: string; readOnly?: boolean }, emit: (event: 'refetchEvents', ...args: any[]) => void) {
  const { t, locale } = useI18n();
  const { isAdmin, isFamilyManager } = useAuth();

  const selectedDate = ref(new Date());

  const lunarDateRangeFilters = computed(() => {
    const startOfMonth = dayjs(selectedDate.value).startOf('month');
    const endOfMonth = dayjs(selectedDate.value).endOf('month');

    const startSolar = Solar.fromYmd(startOfMonth.year(), startOfMonth.month() + 1, startOfMonth.date());
    const endSolar = Solar.fromYmd(endOfMonth.year(), endOfMonth.month() + 1, endOfMonth.date());

    const startLunar = startSolar.getLunar();
    const endLunar = endSolar.getLunar();

    const lunarStartDay = 1;
    const lunarStartMonth = startLunar.getMonth();
    let lunarEndDay = endLunar.getDay();
    const lunarEndMonth = endLunar.getMonth();

    if (lunarEndMonth > lunarStartMonth) {
      lunarEndDay = 30;
    }

    return {
      lunarStartDay,
      lunarStartMonth,
      lunarEndDay,
      lunarEndMonth,
    };
  });

  const eventFilter = computed(() => {
    const startOfMonth = dayjs(selectedDate.value).startOf('month').toDate();
    const endOfMonth = dayjs(selectedDate.value).endOf('month').toDate();

    return {
      familyId: props.familyId,
      startDate: startOfMonth,
      endDate: endOfMonth,
      lunarStartDay: lunarDateRangeFilters.value.lunarStartDay,
      lunarStartMonth: lunarDateRangeFilters.value.lunarStartMonth,
      lunarEndDay: lunarDateRangeFilters.value.lunarEndDay,
      lunarEndMonth: lunarDateRangeFilters.value.lunarEndMonth,
      memberId: props.memberId,
    };
  });

  const { upcomingEvents: events, isLoading: loading, refetch: refetchEvents } = useUpcomingEvents(eventFilter);

  const canAddEvent = computed(() => {
    return !props.readOnly && (isAdmin.value || isFamilyManager.value);
  });

  const canEditEvent = computed(() => {
    return !props.readOnly && (isAdmin.value || isFamilyManager.value);
  });

  const weekdays = computed(() => [0, 1, 2, 3, 4, 5, 6]);



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
  const calendarType = 'month' as const;

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
          const year = selectedDate.value.getFullYear();
          eventStart = getSolarDateFromLunarDate(year, event.lunarDate);
          eventEnd = getSolarDateFromLunarDate(year, event.lunarDate); // Assuming single day events for now
        }

        if (!eventStart) return null; // Skip events without a valid start date

        const formattedEvent = {
          title: event.name,
          start: eventStart,
          end: eventEnd,
          color: event.color || 'primary',
          timed: true,
          eventObject: event,
        };
        return formattedEvent;
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

  watch(events, (_newValue) => {
  }, { immediate: true });

  watch(formattedEvents, (_newValue) => {
  }, { immediate: true });

  const getLunarDateForSolarDay = (solarDate: Date): string => {
    if (!(solarDate instanceof Date) || isNaN(solarDate.getTime())) {
      console.error('Invalid solarDate input to getLunarDateForSolarDay:', solarDate);
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