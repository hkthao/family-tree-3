import { ref, watch } from 'vue';
import { debounce } from 'lodash';
import type { UserProfile } from '@/types';
import { useUserAutocompleteStore } from '@/stores/user-autocomplete.store';

interface UseUserAutocompleteOptions {
  multiple?: boolean;
  initialValue?: string | string[];
}

export function useUserAutocomplete(options?: UseUserAutocompleteOptions) {
  const userAutocompleteStore = useUserAutocompleteStore();
  const search = ref('');
  const loading = ref(false);
  const items = ref<UserProfile[]>([]); // Local items state
  const selectedItems = ref<UserProfile[]>([]);

  const loadItems = async (query: string): Promise<UserProfile[]> => {
    loading.value = true;
    try {
      const result = await userAutocompleteStore.search({ searchQuery: query });
      items.value = result;
      return result;
    } catch (error) {
      console.error('Error loading users:', error);
      items.value = [];
      return [];
    } finally {
      loading.value = false;
    }
  };

  const debouncedLoadItems = debounce(loadItems, 300);

  const onSearchChange = async (query: string): Promise<UserProfile[]> => {
    search.value = query;
    if (query) {
      return await debouncedLoadItems(query) || [];
    } else {
      // When search query is empty, load all items
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
      const preloadedUsers = await userAutocompleteStore.getByIds(idsArray);
      selectedItems.value = preloadedUsers;
      // Ensure preloaded items are also in the local items list if not already present
      preloadedUsers.forEach((user: UserProfile) => {
        if (!items.value.some(item => item.id === user.id)) {
          items.value.push(user);
        }
      });
    } catch (error) {
      console.error('Error preloading users:', error);
    }
  };

  // Watch for initialValue changes to preload users
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
