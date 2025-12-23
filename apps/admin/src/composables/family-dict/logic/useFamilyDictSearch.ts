import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDictFilter } from '@/types';
import { getFamilyDictLineageOptions, getFamilyDictRegionOptions } from '@/composables/utils/familyDictOptions';

export function useFamilyDictSearch(emit: (event: 'update:filters', value: FamilyDictFilter) => void) {
  const { t } = useI18n();
  const expanded = ref(false);

  const filters = ref<FamilyDictFilter>({
    searchQuery: '',
    lineage: undefined,
    region: undefined,
  });

  const familyDictLineages = getFamilyDictLineageOptions(t);

  const regions = getFamilyDictRegionOptions(t);

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
      searchQuery: '',
      lineage: undefined,
      region: undefined,
    };
    emit('update:filters', filters.value);
  };

  return {
    state: {
      expanded,
      filters,
      familyDictLineages,
      regions,
    },
    actions: {
      applyFilters,
      resetFilters,
    },
  };
}
