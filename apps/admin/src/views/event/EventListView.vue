<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="list.items" :total-events="list.totalItems" :loading="list.loading"
    :search="currentFilters.searchQuery || ''" @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate" @view="openDetailDrawer"
    @edit="(event: Event) => openEditDrawer(event.id, event)" @delete="confirmDelete" @create="openAddDrawer" />

  <!-- Add Event Drawer -->
  <BaseCrudDrawer v-model="addDrawer" :title="t('event.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
    <EventAddView v-if="addDrawer" :family-id="currentFilters.familyId || undefined" @close="closeAddDrawer"
      @saved="handleEventSaved" />
  </BaseCrudDrawer>

  <!-- Edit Event Drawer -->
  <BaseCrudDrawer v-model="editDrawer" :title="t('event.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
    <EventEditView v-if="selectedItemId && editDrawer" :event-id="selectedItemId as string" :initial-event="initialData"
      @close="closeEditDrawer" @saved="handleEventSaved" />
  </BaseCrudDrawer>

  <!-- Detail Event Drawer -->
  <BaseCrudDrawer v-model="detailDrawer" :title="t('event.detail.title')" icon="mdi-information-outline"
    @close="closeDetailDrawer">
    <EventDetailView v-if="selectedItemId && detailDrawer" :event-id="selectedItemId as string"
      @close="closeDetailDrawer" @edit="openEditDrawer" />
  </BaseCrudDrawer>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventStore } from '@/stores/event.store';
import type { Event, EventFilter } from '@/types';
import { EventSearch, EventList } from '@/components/event';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
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
const currentPage = ref(1);

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  initialData, // Expose initialData
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const loadEvents = async (
  page: number = currentPage.value,
  itemsPerPageCount: number = itemsPerPage.value,
) => {
  eventStore.list.filter = {
    ...currentFilters.value,
    searchQuery: currentFilters.value.searchQuery || '',
  };
  eventStore.setPage(page);
  eventStore.setItemsPerPage(itemsPerPageCount);

  await eventStore._loadItems(); // Call _loadItems directly
};

const handleFilterUpdate = (filters: Omit<EventFilter, 'searchQuery'>) => {
  currentFilters.value = { ...currentFilters.value, ...filters };
  loadEvents();
};

const handleSearchUpdate = (searchQuery: string) => {
  currentFilters.value.searchQuery = searchQuery;
  loadEvents();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  eventStore.setPage(options.page);
  eventStore.setItemsPerPage(options.itemsPerPage);
  // Handle sorting
  if (options.sortBy && options.sortBy.length > 0) {
    eventStore.setSortBy(options.sortBy);
  } else {
    eventStore.setSortBy([]); // Clear sort if no sortBy is provided
  }
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
      await loadEvents(); // Reload events after deletion
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
  loadEvents(); // Reload list after save
};

onMounted(() => {
  loadEvents();
});
</script>
