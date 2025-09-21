<template>
  <v-container fluid>
    <EventSearch @update:filters="handleFilterUpdate" />

    <EventList
      :events="events"
      :total-events="totalEvents"
      :loading="loading"
      @update:options="handleListOptionsUpdate"
      @view="openViewDialog"
      @edit="navigateToEditEvent"
      @delete="confirmDelete"
      @create="navigateToCreateView"
    />

    <v-dialog v-model="viewDialog" max-width="800px">
      <EventForm
        v-if="selectedEventForView"
        :initial-event-data="selectedEventForView"
        :read-only="true"
        :title="t('event.form.title')"
        @close="closeViewDialog"
      />
    </v-dialog>

    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      :title="t('confirmDelete.title')"
      :message="t('event.list.confirmDelete', { name: eventToDelete?.name || '' })"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Global Snackbar -->
    <v-snackbar v-if="notificationStore.snackbar" v-model="notificationStore.snackbar.show" :color="notificationStore.snackbar.color" timeout="3000">
      {{ notificationStore.snackbar.message }}
    </v-snackbar>

  
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useEvents } from '@/data/events';
import type { Event, EventFilter } from '@/types/event';
import EventSearch from '@/components/events/EventSearch.vue';
import EventList from '@/components/events/EventList.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import EventForm from '@/components/events/EventForm.vue';
import { useNotificationStore } from '@/stores/notification';

const { t } = useI18n();
const { getEvents, deleteEvent } = useEvents();
const notificationStore = useNotificationStore();

const events = ref<Event[]>([]);
const totalEvents = ref(0);
const loading = ref(true);
const currentFilters = ref<EventFilter>({});
const currentPage = ref(1);
const itemsPerPage = ref(10);

const router = useRouter();

const deleteConfirmDialog = ref(false);
const eventToDelete = ref<Event | undefined>(undefined);
const viewDialog = ref(false);
const selectedEventForView = ref<Event | null>(null);

const loadEvents = async () => {
  loading.value = true;
  const { events: fetchedEvents, total } = await getEvents(
    currentFilters.value,
    currentPage.value,
    itemsPerPage.value
  );
  events.value = fetchedEvents;
  totalEvents.value = total;
  loading.value = false;
};

const openViewDialog = (event: Event) => {
  selectedEventForView.value = event;
  viewDialog.value = true;
};

const closeViewDialog = () => {
  viewDialog.value = false;
  selectedEventForView.value = null;
};

const navigateToCreateView = () => {
  router.push('/events/add');
};

const navigateToEditEvent = (event: Event) => {
  router.push(`/events/edit/${event.id}`);
};

const handleFilterUpdate = (filters: EventFilter) => {
  currentFilters.value = filters;
  currentPage.value = 1; // Reset to first page on filter change
  loadEvents();
};

const handleListOptionsUpdate = (options: { page: number; itemsPerPage: number }) => {
  currentPage.value = options.page;
  itemsPerPage.value = options.itemsPerPage;
  loadEvents();
};

const confirmDelete = (event: Event) => {
  eventToDelete.value = event;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (eventToDelete.value) {
    loading.value = true;
    try {
      await deleteEvent(eventToDelete.value.id);
      notificationStore.showSnackbar(t('event.messages.deleteSuccess'), 'success');
      await loadEvents();
    } catch (error) {
      notificationStore.showSnackbar(t('event.messages.deleteError'), 'error');
    }
    loading.value = false;
  }
  deleteConfirmDialog.value = false;
  eventToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  eventToDelete.value = undefined;
};

onMounted(() => {
  loadEvents();
});
</script>
