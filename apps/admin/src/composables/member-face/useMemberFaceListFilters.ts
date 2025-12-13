import { ref, reactive, computed } from 'vue';
import type { MemberFaceFilter } from '@/types';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

export const useMemberFaceListFilters = () => {
  const searchQuery = ref<string>('');
  const filters = reactive<MemberFaceFilter>({});

  const page = ref(1);
  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
  const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([]);

  // Combined filters for API calls
  const combinedFilters = computed(() => {
    return {
      ...filters,
      searchQuery: searchQuery.value,
      page: page.value,
      pageSize: itemsPerPage.value,
      sortBy: sortBy.value.length > 0 ? sortBy.value[0].key : undefined,
      sortOrder: sortBy.value.length > 0 ? (sortBy.value[0].order === 'desc' ? 'descending' : 'ascending') : undefined,
    };
  });

  const setPage = (newPage: number) => {
    page.value = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    itemsPerPage.value = newItemsPerPage;
    page.value = 1; // Reset page when items per page changes
  };

  const setSortBy = (newSortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    sortBy.value = newSortBy;
  };

  const setFilters = (newFilters: MemberFaceFilter) => {
    Object.assign(filters, newFilters);
    page.value = 1; // Reset page when filters change
  };

  const setSearchQuery = (newSearchQuery: string) => {
    searchQuery.value = newSearchQuery;
    page.value = 1; // Reset page when search query changes
  };

  return {
    searchQuery,
    filters,
    page,
    itemsPerPage,
    sortBy,
    combinedFilters,
    setPage,
    setItemsPerPage,
    setSortBy,
    setFilters,
    setSearchQuery,
  };
};
