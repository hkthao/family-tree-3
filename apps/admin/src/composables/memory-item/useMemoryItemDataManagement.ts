import { ref, watch, unref, type Ref } from 'vue';
import type { EmotionalTag, ListOptions } from '@/types';
import type { MemoryItemFilter } from '@/services/memory-item/memory-item.service.interface';

export interface MemoryItemSearchCriteria {
  searchTerm?: string;
  startDate?: Date;
  endDate?: Date;
  emotionalTag?: EmotionalTag;
  memberId?: string;
}

export const useMemoryItemDataManagement = (familyId: Ref<string | undefined> | string) => {
  const filters = ref<MemoryItemFilter>({}); // Removed familyId from here
  const paginationOptions = ref<ListOptions>({
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
  });

  // Removed watch for familyId as it's not part of filters
  // Watch for changes in familyId prop
  // watch(
  //   () => unref(familyId),
  //   (newFamilyId) => {
  //     filters.value.familyId = newFamilyId;
  //   },
  //   { immediate: true },
  // );

  const setFilters = (newFilters: MemoryItemFilter) => {
    Object.assign(filters.value, newFilters);
    paginationOptions.value.page = 1; // Reset to first page on new filters
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
