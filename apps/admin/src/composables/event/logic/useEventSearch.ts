import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import { EventType } from '@/types';
import { CalendarType } from '@/types/enums';

export function useEventSearch(emit: (event: 'update:filters', ...args: any[]) => void) {
  const { t } = useI18n();

  const expanded = ref(false);

  const filters = ref<EventFilter>({
    type: undefined,
    memberId: null,
    startDate: undefined,
    endDate: undefined,
    calendarType: undefined,
  });

  const eventTypes = computed(() => [
    { title: t('event.type.birth'), value: EventType.Birth },
    { title: t('event.type.marriage'), value: EventType.Marriage },
    { title: t('event.type.death'), value: EventType.Death },
    { title: t('event.type.other'), value: EventType.Other },
  ]);

  const calendarTypes = computed(() => [
    { title: t('event.calendarType.solar'), value: CalendarType.Solar },
    { title: t('event.calendarType.lunar'), value: CalendarType.Lunar },
  ]);

  const applyFilters = () => {
    emit('update:filters', filters.value);
  };

  const handleFilterChange = () => {
    applyFilters();
  };

  watch(
    filters.value,
    handleFilterChange,
    { deep: true },
  );

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
    state: {
      expanded,
      filters,
      eventTypes,
      calendarTypes,
    },
    actions: {
      applyFilters,
      resetFilters,
      t, // Expose t for template usage
    },
  };
}
