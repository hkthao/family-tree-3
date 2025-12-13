<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-edit-title">{{ t('event.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingEvent || isUpdatingEvent" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <EventForm ref="eventFormRef" v-if="eventData" :initial-event-data="eventData" :read-only="false" />
      <v-progress-circular v-else indeterminate color="primary"
        data-testid="event-edit-loading-spinner"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm" data-testid="event-edit-cancel-button">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateEvent" data-testid="event-edit-save-button" :loading="isUpdatingEvent">{{ t('common.save')
      }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { EventForm } from '@/components/event';
import type { Event } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useEventQuery, useUpdateEventMutation } from '@/composables/event'; // Import new composables
import { useQueryClient } from '@tanstack/vue-query';

interface EventFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Event | Omit<Event, 'id'>;
}

interface EventEditViewProps {
  eventId: string;
}

const props = defineProps<EventEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const eventFormRef = ref<EventFormExposed | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const queryClient = useQueryClient();

const eventIdRef = toRef(props, 'eventId');
const { event: eventData, isLoading: isLoadingEvent } = useEventQuery(eventIdRef); // Use useEventQuery
const { mutate: updateEvent, isPending: isUpdatingEvent } = useUpdateEventMutation(); // Use useUpdateEventMutation

const handleUpdateEvent = async () => {
  if (!eventFormRef.value) return;
  const isValid = await eventFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const eventToUpdate = eventFormRef.value.getFormData() as Event;
  if (!eventToUpdate.id) {
    showSnackbar(t('event.messages.saveError'), 'error');
    return;
  }

  updateEvent(eventToUpdate, {
    onSuccess: () => {
      showSnackbar(t('event.messages.updateSuccess'), 'success');
      emit('saved');
      queryClient.invalidateQueries({ queryKey: ['events', 'detail', eventToUpdate.id] }); // Invalidate detail query after update
    },
    onError: (error) => {
      showSnackbar(error.message || t('event.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
