<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="events" :total-events="totalItems" :loading="loading"
    :search="eventListSearchQuery || ''" @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate" @view="openDetailDrawer"
    @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer" />

  <!-- Add Event Drawer -->
  <BaseCrudDrawer v-model="addDrawer" @close="closeAddDrawer">
    <EventAddView v-if="addDrawer" :family-id="filters.familyId || undefined" @close="closeAddDrawer"
      @saved="handleEventSaved" />
  </BaseCrudDrawer>

  <!-- Edit Event Drawer -->
  <BaseCrudDrawer v-model="editDrawer" @close="closeEditDrawer">
    <EventEditView v-if="selectedItemId && editDrawer" :event-id="selectedItemId" @close="closeEditDrawer"
      @saved="handleEventSaved" />
  </BaseCrudDrawer>

  <!-- Detail Event Drawer -->
  <BaseCrudDrawer v-model="detailDrawer" @close="closeDetailDrawer">
    <EventDetailView v-if="selectedItemId && detailDrawer" :event-id="selectedItemId" @close="closeDetailDrawer"
      @edit="openEditDrawer" />
  </BaseCrudDrawer>
</template>

<script setup lang="ts">
import { ref, onMounted, toRefs, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event, EventFilter } from '@/types';
import { EventSearch, EventList } from '@/components/event';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import EventEditView from '@/views/event/EventEditView.vue';
import EventDetailView from '@/views/event/EventDetailView.vue';
import { useEventListFilters, useEventsQuery, useDeleteEventMutation } from '@/composables/event'; // Import new composables

const props = defineProps<{
  familyId: string;
  readOnly?: boolean;
}>();

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const eventListFiltersComposables = useEventListFilters();
const {
  searchQuery: eventListSearchQuery,
  page,
  itemsPerPage,
  sortBy,
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

const confirmDelete = async (event: Event) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('event.list.confirmDelete', { name: event.name || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    deleteEvent(event.id!, {
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
</script>
