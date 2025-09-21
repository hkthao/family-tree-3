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
import { useFamilyEventsStore } from '@/stores/familyEvents';
import { useNotificationStore } from '@/stores/notification';
import EventForm from '@/components/events/EventForm.vue';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { FamilyEvent } from '@/services/familyEvent.service';

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
