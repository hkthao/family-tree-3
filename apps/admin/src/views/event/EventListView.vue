<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="list.items" :total-events="list.totalItems" :loading="list.loading"
    :search="currentFilters.searchQuery || ''" @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate" @view="(event: Event) => openDetailDrawer(event.id)"
    @edit="(event: Event) => openEditDrawer(event.id)" @delete="confirmDelete" @create="openAddDrawer" />

  <!-- Add Event Drawer -->
  <BaseCrudDrawer v-model="addDrawer" @close="closeAddDrawer">
    <EventAddView v-if="addDrawer" :family-id="currentFilters.familyId || undefined" @close="closeAddDrawer"
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
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventStore } from '@/stores/event.store';
import type { Event, EventFilter } from '@/types';
import { EventSearch, EventList } from '@/components/event';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import
import EventAddView from '@/views/event/EventAddView.vue'; // New import
import EventEditView from '@/views/event/EventEditView.vue'; // New import
import EventDetailView from '@/views/event/EventDetailView.vue'; // New import

const { t } = useI18n();
const eventStore = useEventStore();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const { list } = storeToRefs(eventStore);

const currentFilters = ref<EventFilter>({});
// const currentPage = ref(1); // Removed
// const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE); // Removed

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  // initialData, // No longer needed for EventEditView
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const fetchEventsData = async () => {
  eventStore.list.filters = {
    ...currentFilters.value,
    searchQuery: currentFilters.value.searchQuery || '',
  };
  await eventStore._loadItems(); // Call _loadItems directly
};

const handleFilterUpdate = (filters: Omit<EventFilter, 'searchQuery'>) => {
  currentFilters.value = { ...currentFilters.value, ...filters };
  fetchEventsData();
};

const handleSearchUpdate = (searchQuery: string) => {
  currentFilters.value.searchQuery = searchQuery;
  fetchEventsData();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  eventStore.setListOptions(options); // Use the new setListOptions action
  fetchEventsData(); // Fetch data after options are updated
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
    try {
      await eventStore.deleteItem(event.id!);
      showSnackbar(
        t('event.messages.deleteSuccess'),
        'success',
      );
      await fetchEventsData(); // Reload events after deletion
    } catch (error) {
      showSnackbar(
        t('event.messages.deleteError'),
        'error',
      );
    }
  }
};

const handleEventSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  fetchEventsData(); // Reload list after save
};

onMounted(() => {
  fetchEventsData(); // Initial load when component is mounted
});
</script>
