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
import { useFamilyEventsStore } from '@/stores/familyEvents';
import { useNotificationStore } from '@/stores/notification';
import EventForm from '@/components/events/EventForm.vue';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { FamilyEvent } from '@/services/familyEvent.service';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const familyEventsStore = useFamilyEventsStore();
const notificationStore = useNotificationStore();

const event = ref<Event | undefined>(undefined);

onMounted(() => {
  const eventId = route.params.id as string;
  event.value = familyEventsStore.items.find(e => e.id === eventId);
});

const handleUpdateEvent = async (eventData: Event) => {
  try {
    await familyEventsStore.update(eventData);
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
