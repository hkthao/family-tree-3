<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-add-title">{{ t('event.form.addTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <EventForm
        ref="eventFormRef"
        @close="closeForm"
        data-testid="event-form"
        :family-id="props.familyId"
      />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey"  @click="closeForm" data-testid="event-add-cancel-button">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary"  @click="handleAddEvent" data-testid="event-add-save-button">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { useEventStore } from '@/stores/event.store';
import { useNotificationStore } from '@/stores/notification.store';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import EventForm from '@/components/event/EventForm.vue'; 

interface EventAddViewProps {
  familyId?: string;
}

const props = defineProps<EventAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const eventFormRef = ref<InstanceType<typeof EventForm> | null>(null);

const { t } = useI18n();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const handleAddEvent = async () => {
  if (!eventFormRef.value) return;
  const isValid = await eventFormRef.value.validate();
  if (!isValid) return;

  const eventData = eventFormRef.value.getFormData();

  try {
    await eventStore.addItem(eventData as Omit<Event, 'id'>);
    if (!eventStore.error) {
      notificationStore.showSnackbar(t('event.messages.addSuccess'), 'success');
      closeForm();
    } else {
      notificationStore.showSnackbar(eventStore.error || t('event.messages.saveError'), 'error');
    }
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>
