import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDictFilter } from '@/types';
import { FamilyDictLineage } from '@/types';

export function useFamilyDictSearch(emit: (event: 'update:filters', value: FamilyDictFilter) => void) {
  const { t } = useI18n();
  const expanded = ref(false);

  const filters = ref<FamilyDictFilter>({
    searchQuery: '',
    lineage: undefined,
    region: undefined,
  });

  const familyDictLineages = computed(() => [
    { title: t('familyDict.lineage.noi'), value: FamilyDictLineage.Noi },
    { title: t('familyDict.lineage.ngoai'), value: FamilyDictLineage.Ngoai },
    { title: t('familyDict.lineage.noiNgoai'), value: FamilyDictLineage.NoiNgoai },
    { title: t('familyDict.lineage.other'), value: FamilyDictLineage.Other },
  ]);

  const regions = computed(() => [
    { title: t('familyDict.form.namesByRegion.north'), value: 'north' },
    { title: t('familyDict.form.namesByRegion.central'), value: 'central' },
    { title: t('familyDict.form.namesByRegion.south'), value: 'south' },
  ]);

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
    expanded,
    filters,
    familyDictLineages,
    regions,
    applyFilters,
    resetFilters,
  };
}
