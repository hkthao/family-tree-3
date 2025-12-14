import { ref, watch, onUnmounted } from 'vue';

export function useDebouncedSearch(initialSearchValue: string = '', delay: number = 300) {
  const searchQuery = ref(initialSearchValue);
  const debouncedSearchQuery = ref(initialSearchValue);
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  watch(
    searchQuery,
    (newValue) => {
      if (debounceTimer) {
        clearTimeout(debounceTimer);
      }
      debounceTimer = setTimeout(() => {
        debouncedSearchQuery.value = newValue;
      }, delay);
    },
  );

  onUnmounted(() => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
  });

  return {
    searchQuery,
    debouncedSearchQuery,
  };
}