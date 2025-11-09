<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList
    :events="list.items"
    :total-events="list.totalItems"
    :loading="list.loading"
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
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useEventStore } from '@/stores/event.store';
import type { Event, EventFilter } from '@/types';
import { EventSearch, EventList, NLEventPopup } from '@/components/event';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { storeToRefs } from 'pinia';

const { t } = useI18n();
const router = useRouter();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();
const { showConfirmDialog } = useConfirmDialog();

const { list } = storeToRefs(eventStore);

const showNLEventPopup = ref(false);

const currentFilters = ref<EventFilter>({});
const currentPage = ref(1);

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

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
      notificationStore.showSnackbar(
        t('event.messages.deleteSuccess'),
        'success',
      );
      await loadEvents(); // Reload events after deletion
    } catch (error) {
      notificationStore.showSnackbar(
        t('event.messages.deleteError'),
        'error',
      );
    }
  }
};

const handleNLEventSaved = async () => {
  showNLEventPopup.value = false;
  await loadEvents();
};

onMounted(() => {
  loadEvents();
});
</script>
