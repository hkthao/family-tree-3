import { ref, watch, computed, type Ref } from 'vue';
import { useRoute, useRouter, type LocationQueryValue } from 'vue-router';
import { type ListOptions, type FilterOptions } from '@/types';

type SortOrder = 'asc' | 'desc'; // Define SortOrder type

export const useImageRestorationJobDataManagement = (familyId: Ref<string>) => {
  const route = useRoute();
  const router = useRouter();

  const defaultSortBy = [{ key: 'created', order: 'desc' as SortOrder }];

  const parseSortBy = (sortByParam: LocationQueryValue | LocationQueryValue[] | undefined): { key: string; order: SortOrder }[] => {
    if (Array.isArray(sortByParam)) {
      return sortByParam.map(item => {
        const parts = String(item).split(':');
        return { key: parts[0], order: (parts[1] as SortOrder) || 'asc' };
      });
    } else if (typeof sortByParam === 'string') {
      const parts = sortByParam.split(':');
      return [{ key: parts[0], order: (parts[1] as SortOrder) || 'asc' }];
    }
    return defaultSortBy;
  };

  const paginationOptions = ref<ListOptions>({ // Changed type
    page: parseInt(route.query.page as string) || 1,
    itemsPerPage: parseInt(route.query.itemsPerPage as string) || 10,
    sortBy: parseSortBy(route.query.sortBy), // New parsing function
  });

  const filters = ref<FilterOptions>({}); // Changed type

  const setPage = (page: number) => {
    paginationOptions.value.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    paginationOptions.value.itemsPerPage = itemsPerPage;
  };

  const setSortBy = (sortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    paginationOptions.value.sortBy = sortBy;
  };

  // Watch for changes in paginationOptions and update route query
  watch(
    paginationOptions,
    (newOptions) => {
      router.push({
        query: {
          ...route.query,
          page: (newOptions.page ?? 1).toString(),
          itemsPerPage: (newOptions.itemsPerPage ?? 10).toString(),
          sortBy: newOptions.sortBy?.map(s => `${s.key}:${s.order}`).join(','), // Convert sortBy array to string
        },
      });
    },
    { deep: true }
  );

  const queryParams = computed<ListOptions>(() => ({ // Changed type
    page: paginationOptions.value.page,
    itemsPerPage: paginationOptions.value.itemsPerPage,
    sortBy: paginationOptions.value.sortBy,
    // filters are handled separately now
  }));

  return {
    state: {
      paginationOptions,
      filters,
      queryParams, // this is now just list options, not all query params
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
    },
  };
};