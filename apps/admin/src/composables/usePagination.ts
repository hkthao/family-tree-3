// apps/admin/src/composables/usePagination.ts
import { ref, computed, watch } from 'vue';

export function usePagination(initialPage = 1, initialItemsPerPage = 10) {
  const currentPage = ref(initialPage);
  const itemsPerPage = ref(initialItemsPerPage);
  const totalItems = ref(0);

  const numberOfPages = computed(() => Math.ceil(totalItems.value / itemsPerPage.value));

  watch(itemsPerPage, () => {
    currentPage.value = 1; // Reset to first page when items per page changes
  });

  const resetPagination = () => {
    currentPage.value = initialPage;
    itemsPerPage.value = initialItemsPerPage;
    totalItems.value = 0;
  };

  return {
    currentPage,
    itemsPerPage,
    totalItems,
    numberOfPages,
    resetPagination,
  };
}
