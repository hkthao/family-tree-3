<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-edit-title">{{ t('event.form.editTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <EventForm ref="eventFormRef" v-if="props.initialEvent" :initial-event-data="props.initialEvent" :read-only="false" data-testid="event-edit-form" />
      <v-progress-circular v-else indeterminate color="primary" data-testid="event-edit-loading-spinner"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm" data-testid="event-edit-cancel-button">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateEvent" data-testid="event-edit-save-button">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventStore } from '@/stores/event.store';
import { useNotificationStore } from '@/stores/notification.store';
import { EventForm } from '@/components/event';
import type { Event } from '@/types';

interface EventFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Event | Omit<Event, 'id'>;
}

interface EventEditViewProps {
  initialEvent: Event;
}

const props = defineProps<EventEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const eventFormRef = ref<EventFormExposed | null>(null);

const { t } = useI18n();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const handleUpdateEvent = async () => {
  if (!eventFormRef.value) return;
  const isValid = await eventFormRef.value.validate();
  if (!isValid) return;

  const eventData = eventFormRef.value.getFormData() as Event;
  if (!props.initialEvent.id) { // Use props.initialEvent.id for the check
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
    return;
  }

  try {
    await eventStore.updateItem(eventData);
    if (!eventStore.error) {
      notificationStore.showSnackbar(t('event.messages.updateSuccess'), 'success');
      emit('saved'); // Emit saved event
    } else {
      notificationStore.showSnackbar(eventStore.error || t('event.messages.saveError'), 'error');
    }
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>
