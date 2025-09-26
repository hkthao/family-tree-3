<template>
  <v-container fluid>
    <v-card v-if="event" class="mb-4">
      <v-card-title class="text-h6 d-flex align-center">
        {{ event.name }}
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="navigateToEditEvent(event.id)">
          {{ t('common.edit') }}
        </v-btn>
        <v-btn icon @click="closeView">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-title>
      <v-card-text>
        <v-tabs v-model="selectedTab" class="mb-4">
          <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
        </v-tabs>

        <v-window v-model="selectedTab">
          <v-window-item value="general">
            <EventForm
              :initial-event-data="event"
              :read-only="true"
              :title="t('event.detail.title')"
            />
          </v-window-item>
        </v-window>
      </v-card-text>
    </v-card>
    <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
      {{ t('common.noData') }}
    </v-alert>

    <!-- Global Snackbar -->
    <v-snackbar
      v-if="notificationStore.snackbar"
      v-model="notificationStore.snackbar.show"
      :color="notificationStore.snackbar.color"
      timeout="3000"
    >
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useEventStore } from '@/stores/event.store';
import { useNotificationStore } from '@/stores/notification.store';
import EventForm from '@/components/events/EventForm.vue';
import type { Event } from '@/types/event/event';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const event = ref<Event | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');
const readOnly = ref(true); // EventDetailView is primarily for viewing

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
