import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import { EventType } from '@/types';

export function useEventSearch(emit: (event: 'update:filters', ...args: any[]) => void) {
  const { t } = useI18n();

  const expanded = ref(false);

  const filters = ref<Omit<EventFilter, 'searchQuery'>>({
    type: undefined,
    memberId: null,
    startDate: undefined,
    endDate: undefined,
  });

  const eventTypes = [
    { title: t('event.type.birth'), value: EventType.Birth },
    { title: t('event.type.marriage'), value: EventType.Marriage },
    { title: t('event.type.death'), value: EventType.Death },
    { title: t('event.type.migration'), value: EventType.Migration },
    { title: t('event.type.other'), value: EventType.Other },
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
    };
    emit('update:filters', filters.value);
  };

  return {
    t,
    expanded,
    filters,
    eventTypes,
    applyFilters,
    resetFilters,
  };
}