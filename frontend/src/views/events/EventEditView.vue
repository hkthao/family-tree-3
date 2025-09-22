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
import { useFamilyEventStore } from '@/stores/family-event.store';
import { useNotificationStore } from '@/stores/notification.store';
import EventForm from '@/components/events/EventForm.vue';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { FamilyEvent } from '@/types/family-event';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const familyEventsStore = useFamilyEventStore();
const notificationStore = useNotificationStore();

const event = ref<FamilyEvent | undefined>(undefined);

// ... (rest of the code)

const handleUpdateEvent = async (eventData: FamilyEvent) => {
  try {
    await familyEventsStore.updateFamilyEvent(eventData);
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
