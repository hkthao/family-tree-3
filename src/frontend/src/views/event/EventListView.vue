<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList
    :events="eventStore.items"
    :total-events="eventStore.totalItems"
    :loading="eventStore.loading"
    :search="currentFilters.searchQuery || ''"
    @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate"
    @view="navigateToDetailView"
    @edit="navigateToEditEvent"
    @delete="confirmDelete"
    @create="navigateToCreateView"
    @ai-create="showNLEventPopup = true"
  />
  <NLEventPopup v-model="showNLEventPopup" @saved="handleNLEventSaved" />
  <!-- Confirm Delete Dialog -->
  <ConfirmDeleteDialog
    :model-value="deleteConfirmDialog"
    :title="t('confirmDelete.title')"
    :message="
      t('event.list.confirmDelete', { name: eventToDelete?.name || '' })
    "
    @confirm="handleDeleteConfirm"
    @cancel="handleDeleteCancel"
  />

</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useEventStore } from '@/stores/event.store';
import type { Event, EventFilter } from '@/types';
import { EventSearch, EventList, NLEventPopup } from '@/components/event';
import { ConfirmDeleteDialog } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

const { t } = useI18n();
const router = useRouter();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const showNLEventPopup = ref(false);

const currentFilters = ref<EventFilter>({});
const currentPage = ref(1);

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const deleteConfirmDialog = ref(false);
const eventToDelete = ref<Event | undefined>(undefined);

const loadEvents = async (
  page: number = currentPage.value,
  itemsPerPageCount: number = itemsPerPage.value,
) => {
  eventStore.filter = {
    ...currentFilters.value,
    searchQuery: currentFilters.value.searchQuery || '',
  };
  eventStore.setPage(page);
  eventStore.setItemsPerPage(itemsPerPageCount);

  await eventStore._loadItems(); // Call _loadItems directly
};

const navigateToDetailView = (event: Event) => {
  router.push(`/event/detail/${event.id}`);
};

const navigateToCreateView = () => {
  router.push('/event/add');
};

const navigateToEditEvent = (event: Event) => {
  router.push(`/event/edit/${event.id}`);
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

const confirmDelete = (event: Event) => {
  eventToDelete.value = event;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (eventToDelete.value) {
    try {
      await eventStore.deleteItem(eventToDelete.value.id!);
      notificationStore.showSnackbar(
        t('event.messages.deleteSuccess'),
        'success',
      );
      await loadEvents(); // Reload events after deletion
    } finally {
      deleteConfirmDialog.value = false;
      eventToDelete.value = undefined;
    }
  }
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  eventToDelete.value = undefined;
};

const handleNLEventSaved = async () => {
  showNLEventPopup.value = false;
  await loadEvents();
};

onMounted(() => {
  loadEvents();
});
</script>
