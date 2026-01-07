import { ref, reactive, watch, type Ref } from 'vue';
import type { ListOptions, FilterOptions } from '@/types/pagination.d'; // Use ListOptions and FilterOptions

export const useUserPushTokenDataManagement = (_userId: Ref<string | undefined | null> | string | null) => { // Update userId type
  const paginationOptions = reactive<ListOptions>({ // Use ListOptions
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
  });

  const filters = reactive<FilterOptions>({
    search: undefined, // Initialize search filter
  });

  const setPage = (page: number) => {
    paginationOptions.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    paginationOptions.itemsPerPage = itemsPerPage;
  };

  const setSortBy = (sortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    paginationOptions.sortBy = sortBy;
  };

  const setFilters = (newFilters: FilterOptions) => {
    Object.assign(filters, newFilters);
  };

  // Watch for changes in userId and reset pagination and filters if userId changes
  watch(
    () => _userId,
    () => {
      paginationOptions.page = 1;
      paginationOptions.itemsPerPage = 10;
      paginationOptions.sortBy = [];
      filters.search = undefined; // Reset search filter
    }
  );

  return {
    state: {
      paginationOptions,
      filters,
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setFilters, // Expose setFilters
    },
  };
};
