import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import { EventType } from '@/types';
import { CalendarType } from '@/types/enums'; // Import CalendarType

export function useEventSearch(emit: (event: 'update:filters', ...args: any[]) => void) {
  const { t } = useI18n();

  const expanded = ref(false);

  const filters = ref<EventFilter>({ // Use EventFilter, not Omit
    type: undefined,
    memberId: null,
    startDate: undefined,
    endDate: undefined,
    calendarType: undefined, // New filter
  });

  const eventTypes = [
    { title: t('event.type.birth'), value: EventType.Birth },
    { title: t('event.type.marriage'), value: EventType.Marriage },
    { title: t('event.type.death'), value: EventType.Death },
    { title: t('event.type.other'), value: EventType.Other }, // Removed Migration
  ];

  const calendarTypes = [
    { title: t('event.calendarType.solar'), value: CalendarType.Solar },
    { title: t('event.calendarType.lunar'), value: CalendarType.Lunar },
  ];

  watch(
    filters.value,
    () => {
      applyFilters();
    },
    { deep: true },
  );

  const applyFilters = () => {
    emit('update:filters', filters.value);
  };

  const resetFilters = () => {
    filters.value = {
      type: undefined,
      memberId: null,
      startDate: undefined,
      endDate: undefined,
      calendarType: undefined,
    };
    emit('update:filters', filters.value);
  };

  return {
    t,
    expanded,
    filters,
    eventTypes,
    calendarTypes, // Expose new calendarTypes
    applyFilters,
    resetFilters,
  };
}
