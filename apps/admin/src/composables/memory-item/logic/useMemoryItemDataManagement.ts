import { ref, type Ref } from 'vue';
import type { EmotionalTag, ListOptions } from '@/types';
import type { MemoryItemFilter } from '@/services/memory-item/memory-item.service.interface';

export interface MemoryItemSearchCriteria {
  searchTerm?: string;
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
    Object.assign(filters.value, newFilters);
    paginationOptions.value.page = 1;
  };

  const setPage = (page: number) => {
    paginationOptions.value.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    paginationOptions.value.itemsPerPage = itemsPerPage;
  };

  const setSortBy = (sortBy: Array<{ key: string; order: 'asc' | 'desc' }>) => {
    paginationOptions.value.sortBy = sortBy;
  };

  return {
    filters,
    paginationOptions,
    setFilters,
    setPage,
    setItemsPerPage,
    setSortBy,
  };
};
