import { ref, reactive, watch, type Ref } from 'vue';
import type { ListOptions } from '@/types/pagination.d'; // Use ListOptions

export const useUserPushTokenDataManagement = (_userId: Ref<string | undefined> | string) => {
  const paginationOptions = reactive<ListOptions>({ // Use ListOptions
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
  });

  // We are not currently using filters for UserPushTokens, but keep the structure for consistency if needed later
  const filters = reactive({
    // Example: status: null,
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

  // Watch for changes in userId and reset pagination if userId changes
  watch(
    () => _userId,
    () => {
      paginationOptions.page = 1;
      paginationOptions.itemsPerPage = 10;
      paginationOptions.sortBy = [];
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
    },
  };
};
