import { ref, watch } from 'vue';
import { useMemberAutocompleteStore } from '@/stores/member-autocomplete.store';
import { debounce } from 'lodash';
import type { Member } from '@/types';
import { removeDiacritics } from '../utils/string.utils'; // NEW IMPORT

interface UseMemberAutocompleteOptions {
  familyId?: string;
  multiple?: boolean;
  initialValue?: string | string[];
}

export function useMemberAutocomplete(options?: UseMemberAutocompleteOptions) {
  const memberAutocompleteStore = useMemberAutocompleteStore();
  const search = ref('');
  const loading = ref(false);
  const items = ref<Member[]>([]); // Local items state
  const selectedItems = ref<Member[]>([]);

  const loadItems = async (query: string): Promise<Member[]> => {
    loading.value = true;
    try {
      const processedQuery = removeDiacritics(query); // NEW: Preprocess query
      const result = await memberAutocompleteStore.search({
        searchQuery: processedQuery, // Use processed query
        familyId: options?.familyId,
      });
      items.value = result;
      return result;
    } catch (error) {
      console.error('Error loading members:', error);
      items.value = [];
      return [];
    } finally {
      loading.value = false;
    }
  };

  const debouncedLoadItems = debounce(loadItems, 300);

  const onSearchChange = async (query: string): Promise<Member[]> => {
    search.value = query;
    if (query) {
      return await debouncedLoadItems(query) || [];
    } else {
      // When search query is empty, load the first page of items
      return await loadItems('');
    }
  };

  const preloadById = async (ids: string | string[] | undefined) => {
    if (!ids || (Array.isArray(ids) && ids.length === 0)) {
      selectedItems.value = [];
      return;
    }

    const idsArray = Array.isArray(ids) ? ids : [ids];
    try {
      const preloadedMembers = await memberAutocompleteStore.getByIds(idsArray);
      selectedItems.value = preloadedMembers;
      // Ensure preloaded items are also in the local items list if not already present
      preloadedMembers.forEach((member: Member) => {
        if (!items.value.some(item => item.id === member.id)) {
          items.value.push(member);
        }
      });
    } catch (error) {
      console.error('Error preloading members:', error);
    }
  };

  // Watch for initialValue changes to preload members
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