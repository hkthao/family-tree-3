<template>
  <v-container fluid>
    <EventSearch @update:filters="handleFilterUpdate" />

    <v-tabs v-model="selectedTab" class="mb-4">
      <v-tab value="table">{{ t('event.view.table') }}</v-tab>
      <v-tab value="timeline">{{ t('event.view.timeline') }}</v-tab>
      <v-tab value="calendar">{{ t('event.view.calendar') }}</v-tab>
    </v-tabs>

    <v-window v-model="selectedTab">
      <v-window-item value="table">
        <EventList
          :events="familyEventStore.items"
          :total-events="familyEventStore.totalItems"
          :loading="familyEventStore.loading"
          @update:options="handleListOptionsUpdate"
          @view="openViewDialog"
          @edit="navigateToEditEvent"
          @delete="confirmDelete"
          @create="navigateToCreateView"
        />
      </v-window-item>
      <v-window-item value="timeline">
        <EventTimeline :events="familyEventStore.items" />
      </v-window-item>
      <v-window-item value="calendar">
        <EventCalendar
          :events="familyEventStore.items"
          @viewEvent="openViewDialog"
        />
      </v-window-item>
    </v-window>

    <v-alert
      v-if="
        (selectedTab === 'timeline' || selectedTab === 'calendar') &&
        !currentFilters.familyId &&
        !familyEventStore.loading
      "
      type="info"
      class="mt-4"
      variant="tonal"
    >
      {{ t('event.messages.selectFamily') }}
    </v-alert>

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
      :message="
        t('event.list.confirmDelete', { name: eventToDelete?.name || '' })
      "
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Global Snackbar -->
    <v-snackbar
      v-if="notificationStore.snackbar"
      v-model="notificationStore.snackbar.show"
      :color="notificationStore.snackbar.color"
      timeout="3000"
    >
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyEventStore } from '@/stores/family-event.store';
import type { FamilyEvent } from '@/types/family-event';
import type { EventFilter } from '@/services/family-event/family-event.service.interface';
import EventSearch from '@/components/events/EventSearch.vue';
import EventList from '@/components/events/EventList.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import EventForm from '@/components/events/EventForm.vue';
import EventTimeline from '@/components/events/EventTimeline.vue';
import EventCalendar from '@/components/events/EventCalendar.vue';
import { useNotificationStore } from '@/stores/notification.store';

const { t } = useI18n();
const familyEventStore = useFamilyEventStore();
const notificationStore = useNotificationStore();

const currentFilters = ref<EventFilter>({});
const currentPage = ref(1);
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

// ... (rest of the file)

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
const selectedTab = ref('table');

const router = useRouter();

const deleteConfirmDialog = ref(false);
const eventToDelete = ref<FamilyEvent | undefined>(undefined);
const viewDialog = ref(false);
const selectedEventForView = ref<FamilyEvent | null>(null);

const loadEvents = async (fetchItemsPerPage: number = itemsPerPage.value) => {
  if (
    (selectedTab.value === 'timeline' || selectedTab.value === 'calendar') &&
    !currentFilters.value.familyId
  ) {
    familyEventStore.items = [];
    familyEventStore.totalItems = 0;
    familyEventStore.loading = false;
    return;
  }

  await familyEventStore.searchItems(
    currentFilters.value.name || '',
    currentFilters.value.familyId || undefined,
  );
};

watch(selectedTab, (newTab) => {
  if (newTab === 'calendar') {
    loadEvents(-1); // Fetch all events for calendar view
  } else {
    loadEvents(); // Use current itemsPerPage for other views
  }
});
const openViewDialog = (event: FamilyEvent) => {
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

const navigateToEditEvent = (event: FamilyEvent) => {
  router.push(`/events/edit/${event.id}`);
};

const handleFilterUpdate = (filters: EventFilter) => {
  currentFilters.value = filters;
  currentPage.value = 1; // Reset to first page on filter change
  loadEvents();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
}) => {
  familyEventStore.setPage(options.page);
  familyEventStore.setItemsPerPage(options.itemsPerPage);
};

const confirmDelete = (event: FamilyEvent) => {
  eventToDelete.value = event;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (eventToDelete.value) {
    try {
      await familyEventStore.deleteItem(eventToDelete.value.id);
      notificationStore.showSnackbar(
        t('event.messages.deleteSuccess'),
        'success',
      );
      await loadEvents(); // Reload events after deletion
    } finally {
      // Added finally block for consistent dialog closing
      deleteConfirmDialog.value = false;
      eventToDelete.value = undefined;
    }
  }
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  eventToDelete.value = undefined;
};

onMounted(() => {
  loadEvents();
});
</script>
