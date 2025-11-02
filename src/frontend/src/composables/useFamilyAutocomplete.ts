import { ref, watch } from 'vue';
import { useFamilyAutocompleteStore } from '@/stores/family-autocomplete.store';
import { debounce } from 'lodash';
import type { Family } from '@/types';

interface UseFamilyAutocompleteOptions {
  multiple?: boolean;
  initialValue?: string | string[];
}

export function useFamilyAutocomplete(options?: UseFamilyAutocompleteOptions) {
  const familyAutocompleteStore = useFamilyAutocompleteStore();
  const search = ref('');
  const loading = ref(false);
  const items = ref<Family[]>([]); // Local items state
  const selectedItems = ref<Family[]>([]);

  const loadItems = async (query: string) => {
    loading.value = true;
    try {
      const result = await familyAutocompleteStore.search({
        searchQuery: query,
      });
      items.value = result;
    } catch (error) {
      console.error('Error loading families:', error);
      items.value = [];
    } finally {
      loading.value = false;
    }
  };

  const debouncedLoadItems = debounce(loadItems, 300);

  const onSearchChange = (query: string) => {
    search.value = query;
    if (query) {
      debouncedLoadItems(query);
    } else {
      familyAutocompleteStore.clearItems();
      items.value = []; // Clear local items
    }
  };

  const preloadById = async (ids: string | string[] | undefined) => {
    if (!ids || (Array.isArray(ids) && ids.length === 0)) {
      selectedItems.value = [];
      return;
    }

    const idsArray = Array.isArray(ids) ? ids : [ids];
    try {
      const preloadedFamilies = await familyAutocompleteStore.getByIds(idsArray);
      selectedItems.value = preloadedFamilies;
      // Ensure preloaded items are also in the local items list if not already present
      preloadedFamilies.forEach((family: Family) => {
        if (!items.value.some(item => item.id === family.id)) {
          items.value.push(family);
        }
      });
    } catch (error) {
      console.error('Error preloading families:', error);
    }
  };

  // Watch for initialValue changes to preload families
  watch(() => options?.initialValue, (newVal) => {
    if (newVal) {
      preloadById(newVal);
    } else {
      selectedItems.value = [];
    }
  }, { immediate: true });

  return {
    search,
    loading,
    items,
    selectedItems,
    onSearchChange,
    preloadById,
  };
}
