<template>
  <v-card v-if="event" class="mb-4">
      <v-card-title class="text-h6 d-flex align-center">
        {{ event.name }}
        <v-spacer></v-spacer>
      </v-card-title>
      <v-card-text>
        <v-tabs v-model="selectedTab" class="mb-4">
          <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
          <v-tab value="timeline">{{ t('event.view.timeline') }}</v-tab>
          <v-tab value="calendar">{{ t('event.view.calendar') }}</v-tab>
        </v-tabs>

        <v-window v-model="selectedTab">
          <v-window-item value="general">
            <EventForm
              :initial-event-data="event"
              :read-only="true"
              :title="t('event.detail.title')"
            />
          </v-window-item>

          <v-window-item value="timeline">
            <EventTimeline
              :family-id="event.familyId ?? undefined"
              :read-only="readOnly"
              @addEvent="handleAddEvent"
              @editEvent="handleEditEvent"
              @deleteEvent="handleDeleteEvent"
            />
          </v-window-item>

          <v-window-item value="calendar">
            <EventCalendar
              :events="[event]"
              @viewEvent="handleEditEvent"
            />
          </v-window-item>
        </v-window>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="navigateToEditEvent(event.id)">
          {{ t('common.edit') }}
        </v-btn>
        <v-btn color="blue-darken-1" variant="text" @click="closeView">
          {{ t('common.close') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
    {{ t('common.noData') }}
  </v-alert>
  <!-- Dialog for Event Form -->
  <v-dialog v-model="eventFormDialog" max-width="800px">
    <EventForm
      v-if="selectedEventForForm"
      :initial-event-data="selectedEventForForm"
      :read-only="readOnly"
      :title="
        isEditEventMode ? t('event.form.editTitle') : t('event.form.addTitle')
      "
      @close="handleCancelEventForm"
      @submit="handleSaveEventForm"
    />
  </v-dialog>
  <!-- Confirm Delete Dialog for Events -->
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
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useEventStore } from '@/stores/event.store';
import { useNotificationStore } from '@/stores/notification.store';
import EventForm from '@/components/events/EventForm.vue';
import EventTimeline from '@/components/events/EventTimeline.vue';
import EventCalendar from '@/components/events/EventCalendar.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import type { Event } from '@/types/event/event';
import { EventType } from '@/types/event/event-type';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const event = ref<Event | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');
const readOnly = ref(true); // EventDetailView is primarily for viewing

// Event Form Dialog State
const eventFormDialog = ref(false);
const selectedEventForForm = ref<Event | undefined>(undefined);
const isEditEventMode = ref(false);

// Event Delete Dialog State
const deleteConfirmDialog = ref(false);
const eventToDelete = ref<Event | undefined>(undefined);

const loadEvent = async () => {
  loading.value = true;
  const eventId = route.params.id as string;
  if (eventId) {
    event.value = await eventStore.fetchItemById(eventId);
  }
  loading.value = false;
};

const navigateToEditEvent = (id: string) => {
  router.push(`/events/edit/${id}`);
};

const closeView = () => {
  router.push('/events');
};

// Event Handlers for Timeline/Calendar
const handleAddEvent = () => {
  if (!event.value?.id) return;
  selectedEventForForm.value = {
    id: '',
    name: '',
    description: '',
    startDate: null,
    endDate: null,
    location: '',
    familyId: event.value.familyId || null, // Pre-fill familyId from current event
    relatedMembers: event.value.relatedMembers || [], // Pre-fill related members from current event
    type: EventType.Other,
    color: 'blue',
  };
  isEditEventMode.value = false;
  eventFormDialog.value = true;
};

const handleEditEvent = (eventToEdit: Event) => {
  selectedEventForForm.value = { ...eventToEdit };
  isEditEventMode.value = true;
  eventFormDialog.value = true;
};

const handleDeleteEvent = (eventToDeleteParam: Event) => {
  eventToDelete.value = eventToDeleteParam;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (!eventToDelete.value?.id) return;
  try {
    await eventStore.deleteItem(eventToDelete.value.id);
    notificationStore.showSnackbar(
      t('event.messages.deleteSuccess'),
      'success',
    );
    await loadEvent(); // Reload event and related data after deletion
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.deleteError'), 'error');
  } finally {
    deleteConfirmDialog.value = false;
    eventToDelete.value = undefined;
  }
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  eventToDelete.value = undefined;
};

const handleSaveEventForm = async (eventData: Event) => {
  try {
    if (isEditEventMode.value) {
      await eventStore.updateItem(eventData);
      notificationStore.showSnackbar(
        t('event.messages.updateSuccess'),
        'success',
      );
    } else {
      await eventStore.addItem(eventData);
      notificationStore.showSnackbar(t('event.messages.addSuccess'), 'success');
    }
    await loadEvent(); // Reload event and related data after saving
    eventFormDialog.value = false;
    selectedEventForForm.value = undefined;
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const handleCancelEventForm = () => {
  eventFormDialog.value = false;
  selectedEventForForm.value = undefined;
};

onMounted(() => {
  loadEvent();
});

watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadEvent();
    }
  },
);
</script>
