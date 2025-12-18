import { ref, watch } from 'vue';
import type { Family } from '@/types';
import { ApiFamilyService } from '@/services/family/api.family.service';
import apiClient from '@/plugins/axios';

const familyService = new ApiFamilyService(apiClient);

export function useFamilyAutocompleteData(
  modelValue: string | string[] | undefined | null,
  multiple: boolean,
) {
  const preloadedFamilies = ref<Family[]>([]);
  const internalValue = ref<Family | Family[] | null>(null);
  const isLoadingPreload = ref(false);

  const fetchFamilyByIds = async (ids: string[]) => {
    if (!ids || ids.length === 0) {
      preloadedFamilies.value = [];
      return;
    }
    isLoadingPreload.value = true;
    try {
      const result = await familyService.getByIds(ids);
      if (result.ok) {
        preloadedFamilies.value = result.value;
      } else {
        console.error('Error preloading families:', result.error);
        preloadedFamilies.value = [];
      }
    } finally {
      isLoadingPreload.value = false;
    }
  };

  watch(() => modelValue, async (newModelValue) => {
    if (newModelValue) {
      if (multiple && Array.isArray(newModelValue)) {
        await fetchFamilyByIds(newModelValue as string[]);
        internalValue.value = preloadedFamilies.value;
      } else if (!multiple && typeof newModelValue === 'string') {
        await fetchFamilyByIds([newModelValue as string]);
        internalValue.value = preloadedFamilies.value[0] || null;
      }
    } else {
      internalValue.value = null;
    }
  }, { immediate: true });

  const fetchItems = async (query: string): Promise<Family[]> => {
    const result = await familyService.search({ page: 1, itemsPerPage: 10 }, { name: query });
    if (result.ok) {
      return result.value.items;
    }
    console.error('Error fetching families:', result.error);
    return [];
  };

  return {
    internalValue,
    isLoadingPreload,
    fetchItems,
  };
}
