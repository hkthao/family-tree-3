<template>
  <v-card>
    <v-card-text>
      <EventForm
        :title="t('event.form.addTitle')"
        @close="closeForm"
        @submit="handleAddEvent"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyEventStore } from '@/stores/family-event.store';
import { useNotificationStore } from '@/stores/notification.store';
import EventForm from '@/components/events/EventForm.vue';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { FamilyEvent } from '@/services/family-event';

const { t } = useI18n();
const router = useRouter();
const familyEventsStore = useFamilyEventsStore();
const notificationStore = useNotificationStore();

const handleAddEvent = async (eventData: Omit<Event, 'id'>) => {
  try {
    await familyEventsStore.add(eventData);
    notificationStore.showSnackbar(t('event.messages.addSuccess'), 'success');
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  router.push('/events');
};
</script>
