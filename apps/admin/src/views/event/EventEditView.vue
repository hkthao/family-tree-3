<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-edit-title">{{ t('event.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading || update.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <EventForm ref="eventFormRef" v-if="event" :initial-event-data="event" :read-only="false" />
      <v-progress-circular v-else indeterminate color="primary"
        data-testid="event-edit-loading-spinner"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm" data-testid="event-edit-cancel-button">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateEvent" data-testid="event-edit-save-button" :loading="update.loading">{{ t('common.save')
      }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'; // Added onMounted, watch
import { useI18n } from 'vue-i18n';
import { useEventStore } from '@/stores/event.store';
import { EventForm } from '@/components/event';
import type { Event } from '@/types';
import { storeToRefs } from 'pinia'; // Added storeToRefs
import { useGlobalSnackbar } from '@/composables'; // Import useGlobalSnackbar

interface EventFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Event | Omit<Event, 'id'>;
}

interface EventEditViewProps {
  eventId: string; // Only eventId prop
}

const props = defineProps<EventEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const eventFormRef = ref<EventFormExposed | null>(null);

const { t } = useI18n();
const eventStore = useEventStore();
const { showSnackbar } = useGlobalSnackbar();

const { detail, update } = storeToRefs(eventStore); // Destructure loading and update from store

const event = ref<Event | undefined>(undefined); // Local ref for event data

const loadEvent = async (id: string) => {
  await eventStore.getById(id);
  if (eventStore.detail.item)
    event.value = eventStore.detail.item;
};

onMounted(async () => {
  if (props.eventId) {
    await loadEvent(props.eventId);
  }
});

watch(
  () => props.eventId,
  async (newId) => {
    if (newId) {
      await loadEvent(newId);
    }
  },
);

const handleUpdateEvent = async () => {
  if (!eventFormRef.value) return;
  const isValid = await eventFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const eventData = eventFormRef.value.getFormData() as Event;
  if (!eventData.id) { // Use eventData.id for the check
    showSnackbar(t('event.messages.saveError'), 'error');
    return;
  }

  try {
    await eventStore.updateItem(eventData as Event);
    if (!eventStore.error) {
      showSnackbar(t('event.messages.updateSuccess'), 'success');
      emit('saved'); // Emit saved event
    } else {
      showSnackbar(eventStore.error || t('event.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>
