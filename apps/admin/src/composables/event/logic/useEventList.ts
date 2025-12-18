import { onMounted, toRefs, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import { useEventListFilters } from '@/composables';
import { useEventsQuery } from '@/composables';
import { useDeleteEventMutation } from '@/composables';

export function useEventList(props: { familyId: string; readOnly?: boolean }, _emit: (event: 'saved' | 'close', ...args: any[]) => void) {
  const { t } = useI18n();
  const { showConfirmDialog } = useConfirmDialog();
  const { showSnackbar } = useGlobalSnackbar();

  const eventListFiltersComposables = useEventListFilters();
  const {
    searchQuery: eventListSearchQuery,
    filters,
  } = toRefs(eventListFiltersComposables);

  const {
    setPage,
    setItemsPerPage,
    setSortBy,
    setSearchQuery,
    setFilters,
  } = eventListFiltersComposables;

  const { events, totalItems, loading, refetch } = useEventsQuery(filters);
  const { mutate: deleteEvent } = useDeleteEventMutation();

  const {
    addDrawer,
    editDrawer,
    detailDrawer,
    selectedItemId,
    openAddDrawer,
    openEditDrawer,
    openDetailDrawer,
    closeAddDrawer,
    closeEditDrawer,
    closeDetailDrawer,
    closeAllDrawers,
  } = useCrudDrawer<string>();


  const handleFilterUpdate = (newFilters: Omit<EventFilter, 'searchQuery'>) => {
    setFilters(newFilters);
  };

  const handleSearchUpdate = (searchQuery: string) => {
    setSearchQuery(searchQuery);
  };

  const handleListOptionsUpdate = (options: {
    page: number;
    itemsPerPage: number;
    sortBy: { key: string; order: string }[];
  }) => {
    setPage(options.page);
    setItemsPerPage(options.itemsPerPage);
    setSortBy(options.sortBy as { key: string; order: 'asc' | 'desc' }[]);
  };

  const confirmDelete = async (eventId: string, eventName?: string) => {
    const confirmed = await showConfirmDialog({
      title: t('confirmDelete.title'),
      message: t('event.list.confirmDelete', { name: eventName || '' }),
      confirmText: t('common.delete'),
      cancelText: t('common.cancel'),
      confirmColor: 'error',
    });

    if (confirmed) {
      deleteEvent(eventId, {
        onSuccess: () => {
          showSnackbar(
            t('event.messages.deleteSuccess'),
            'success',
          );
          refetch(); // Refetch the list after successful deletion
        },
        onError: (error) => {
          showSnackbar(
            error.message || t('event.messages.deleteError'),
            'error',
          );
        },
      });
    }
  };

  const handleEventSaved = () => {
    closeAllDrawers(); // Close whichever drawer was open
    refetch(); // Refetch the list in case an event was added/updated
  };

  onMounted(() => {
    setFilters({ familyId: props.familyId });
  });

  // Watch for changes in familyId prop and update filters
  watch(() => props.familyId, (newFamilyId) => {
    setFilters({ familyId: newFamilyId });
  });

  return {
    t,
    eventListSearchQuery,
    filters,
    events,
    totalItems,
    loading,
    addDrawer,
    editDrawer,
    detailDrawer,
    selectedItemId,
    handleFilterUpdate,
    handleSearchUpdate,
    handleListOptionsUpdate,
    confirmDelete,
    handleEventSaved,
    openAddDrawer,
    openEditDrawer,
    openDetailDrawer,
    closeAddDrawer,
    closeEditDrawer,
    closeDetailDrawer,
  };
}