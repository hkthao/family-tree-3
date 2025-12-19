import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';

interface Item {
  [key: string]: any;
}

interface UseRemoteAutocompleteOptions {
  fetchItems: (query: string) => Promise<Item[]>;
  itemTitle: string | ((item: Item) => string);
  itemValue: string | ((item: Item) => any);
  debounceTime?: number;
}

export function useRemoteAutocomplete(options: UseRemoteAutocompleteOptions) {
  const { fetchItems, itemTitle, itemValue, debounceTime = 300 } = options;
  const { t } = useI18n();

  const loading = ref(false);
  const remoteItems = ref<Item[]>([]);
  const searchText = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;
  const hasBeenFocused = ref(false);

  const internalNoDataText = computed(() => t('common.no_data'));

  const getItemTitle = (item: Item): string => {
    if (typeof itemTitle === 'function') {
      return itemTitle(item);
    }
    return item[itemTitle] !== undefined ? String(item[itemTitle]) : '';
  };

  const getItemValue = (item: Item): any => {
    if (typeof itemValue === 'function') {
      return itemValue(item);
    }
    return item[itemValue];
  };

  const fetchRemoteData = async (query: string) => {
    loading.value = true;
    try {
      const result = await fetchItems(query);
      remoteItems.value = result.map(item => ({
        ...item,
        title: getItemTitle(item),
        value: getItemValue(item),
      }));
    } catch (error) {
      console.error('Error fetching remote items:', error);
      remoteItems.value = [];
    } finally {
      loading.value = false;
    }
  };

  watch(
    searchText,
    (newSearchText) => {
      if (debounceTimer) {
        clearTimeout(debounceTimer);
      }
      debounceTimer = setTimeout(() => {
        // Only fetch if there's a search text or if it's the initial focus with empty search
        if (newSearchText || hasBeenFocused.value) {
            fetchRemoteData(newSearchText);
        } else if (!newSearchText && !hasBeenFocused.value) {
          // If search text is empty and not focused yet, don't fetch, just clear items.
          remoteItems.value = [];
        }
      }, debounceTime);
    },
    { immediate: false }
  );

  const handleFocus = () => {
    // If not focused before and search text is empty, trigger initial fetch
    if (!hasBeenFocused.value && !searchText.value) {
      fetchRemoteData('');
      hasBeenFocused.value = true;
    }
  };

  return {
    loading,
    remoteItems,
    searchText,
    internalNoDataText,
    getItemTitle,
    getItemValue,
    handleFocus,
    fetchRemoteData, // Expose for initial loading if needed
  };
}
