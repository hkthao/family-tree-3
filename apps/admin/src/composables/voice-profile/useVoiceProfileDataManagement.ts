import { reactive, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

export function useVoiceProfileDataManagement(memberId: Ref<string>) {
  const router = useRouter();
  const route = useRoute();

  const paginationOptions = reactive({
    page: parseInt(route.query.page as string) || 1,
    itemsPerPage: parseInt(route.query.itemsPerPage as string) || 10,
    sortBy: (route.query.sortBy as string || 'name:asc')
      .split(',')
      .map(s => {
        const [key, order] = s.split(':');
        return { key, order: order === 'desc' ? 'desc' : 'asc' };
      }),
  });

  const filters = reactive({
    search: route.query.search as string || '',
    memberId: memberId.value,
  });

  watch(memberId, (newMemberId) => {
    filters.memberId = newMemberId;
  }, { immediate: true });

  watch(
    [paginationOptions, filters],
    () => {
      router.push({
        query: {
          ...route.query,
          page: paginationOptions.page,
          itemsPerPage: paginationOptions.itemsPerPage,
          sortBy: paginationOptions.sortBy.map(s => `${s.key}:${s.order}`).join(','),
          search: filters.search || undefined,
        },
      });
    },
    { deep: true }
  );

  const setPage = (page: number) => {
    paginationOptions.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    paginationOptions.itemsPerPage = itemsPerPage;
  };

  const setSortBy = (sortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    paginationOptions.sortBy = sortBy;
  };

  const setSearch = (search: string) => {
    filters.search = search;
    setPage(1); // Reset to first page when search changes
  };

  return {
    state: {
      paginationOptions,
      filters,
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setSearch,
    },
  };
}
