import { onMounted, toRefs, watch, type Ref, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import {
  useConfirmDialog,
  type UseConfirmDialogReturn,
} from '@/composables/ui/useConfirmDialog';
import {
  useGlobalSnackbar,
  type UseGlobalSnackbarReturn,
} from '@/composables/ui/useGlobalSnackbar';
import {
  useCrudDrawer,
  type UseCrudDrawerReturn,
} from '@/composables/ui/useCrudDrawer';
import { useEventListFilters, type UseEventListFiltersReturn } from '@/composables/event/logic/useEventListFilters';
import { useEventsQuery, type UseEventsQueryReturn } from '@/composables/event/queries/useEventsQuery';
import { useDeleteEventMutation, type UseDeleteEventMutationReturn } from '@/composables/event/mutations/useDeleteEventMutation';

interface UseEventListDeps {
  useI18n: typeof useI18n;
  useConfirmDialog: () => UseConfirmDialogReturn;
  useGlobalSnackbar: () => UseGlobalSnackbarReturn;
  useCrudDrawer: <T>() => UseCrudDrawerReturn<T>;
  useEventListFilters: () => UseEventListFiltersReturn;
  useEventsQuery: (filters: Ref<EventFilter> | ComputedRef<EventFilter>) => UseEventsQueryReturn;
  useDeleteEventMutation: () => UseDeleteEventMutationReturn;
}

const defaultDeps: UseEventListDeps = {
  useI18n,
  useConfirmDialog,
  useGlobalSnackbar,
  useCrudDrawer,
  useEventListFilters,
  useEventsQuery,
  useDeleteEventMutation,
};

export function useEventList(
  props: { familyId: string; readOnly?: boolean },
  _emit: (event: 'saved' | 'close', ...args: any[]) => void,
  deps: UseEventListDeps = defaultDeps,
) {
  const {
    useI18n,
    useConfirmDialog,
    useGlobalSnackbar,
    useCrudDrawer,
    useEventListFilters,
    useEventsQuery,
    useDeleteEventMutation,
  } = deps;

  const { t } = useI18n();
  const { showConfirmDialog } = useConfirmDialog();
  const { showSnackbar } = useGlobalSnackbar();

  const eventListFiltersComposables = useEventListFilters();
  const { searchQuery: eventListSearchQuery, filters } = toRefs(eventListFiltersComposables);

  const { setPage, setItemsPerPage, setSortBy, setSearchQuery, setFilters } = eventListFiltersComposables;

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
          showSnackbar(t('event.messages.deleteSuccess'), 'success');
          refetch(); // Refetch the list after successful deletion
        },
        onError: (error: Error) => {
          showSnackbar(error.message || t('event.messages.deleteError'), 'error');
        },
      });
    }
  };

  const handleEventSaved = () => {
    closeAllDrawers(); // Close whichever drawer was open
    refetch(); // Refetch the list in case an event was added/updated
  };

  const handleFamilyIdChange = (newFamilyId: string) => {
    setFilters({ familyId: newFamilyId });
  };

  onMounted(() => {
    handleFamilyIdChange(props.familyId); // Initial set
  });

  watch(() => props.familyId, handleFamilyIdChange);

  return {
    state: {
      eventListSearchQuery,
      filters,
      events,
      totalItems,
      loading,
      addDrawer,
      editDrawer,
      detailDrawer,
      selectedItemId,
    },
    actions: {
      t, // Expose t for template usage
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
      refetchEvents: refetch, // Expose refetch
    },
  };
}