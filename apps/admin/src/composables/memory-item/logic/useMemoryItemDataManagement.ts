import { ref, type Ref } from 'vue';
import type { EmotionalTag, ListOptions } from '@/types';
import type { MemoryItemFilter } from '@/services/memory-item/memory-item.service.interface';

export interface MemoryItemSearchCriteria {
  searchQuery?: string;
  startDate?: Date;
  endDate?: Date;
  emotionalTag?: EmotionalTag;
  memberId?: string;
}

export const useMemoryItemDataManagement = (_familyId: Ref<string | undefined> | string) => {
  const filters = ref<MemoryItemFilter>({});
  const paginationOptions = ref<ListOptions>({
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
  });

  const setFilters = (newFilters: MemoryItemFilter) => {
    const oldFilters = { ...filters.value };
    Object.assign(filters.value, newFilters);

    const hasNonPageFilterChanged = Object.keys(newFilters).some(key =>
      key !== 'page' && oldFilters[key] !== newFilters[key]
    );

    if (hasNonPageFilterChanged && newFilters.page === undefined) {
      paginationOptions.value.page = 1;
    }
  };

  const setPage = (page: number) => {
    paginationOptions.value.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    if (paginationOptions.value.itemsPerPage !== itemsPerPage) {
      paginationOptions.value.itemsPerPage = itemsPerPage;
      paginationOptions.value.page = 1;
    }
  };

  const setSortBy = (sortBy: Array<{ key: string; order: 'asc' | 'desc' }>) => {
    paginationOptions.value.sortBy = sortBy;
  };

  return {
    state: {
      filters,
      paginationOptions,
    },
    actions: {
      setFilters,
      setPage,
      setItemsPerPage,
      setSortBy,
    },
  };
};
