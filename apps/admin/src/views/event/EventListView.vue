<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="list.items" :total-events="list.totalItems" :loading="list.loading"
    :search="currentFilters.searchQuery || ''" @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate" @view="openDetailDrawer"
    @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer" />

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


const handleFilterUpdate = (filters: Omit<EventFilter, 'searchQuery'>) => {
  list.value.filters = { ...currentFilters.value, ...filters };
  eventStore._loadItems()
};

const handleSearchUpdate = (searchQuery: string) => {
  list.value.filters.searchQuery = searchQuery;
  eventStore._loadItems()
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  eventStore.setListOptions(options); // Use the new setListOptions action
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
};

onMounted(() => {
});
</script>
