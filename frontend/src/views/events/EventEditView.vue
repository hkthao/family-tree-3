<template>
  <v-card>
    <v-card-text>
      <EventForm
        v-if="event"
        :title="t('event.form.editTitle')"
        :initial-event-data="event"
        @close="closeForm"
        @submit="handleUpdateEvent"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { useEventStore } from '@/stores/event.store';
import { useNotificationStore } from '@/stores/notification.store';
import EventForm from '@/components/events/EventForm.vue';

import type { Event } from '@/types/event/event';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const event = ref<Event | undefined>(undefined);

onMounted(async () => {
  const eventId = route.params.id as string;
  if (eventId) {
    event.value = await eventStore.fetchItemById(eventId);
  }
});

const handleUpdateEvent = async (eventData: Event) => {
  try {
    await eventStore.updateItem(eventData);
    notificationStore.showSnackbar(t('event.messages.updateSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/events');
};
</script>
