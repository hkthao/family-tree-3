<template>
  <v-card v-if="event" class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ event.name }}
      <v-spacer></v-spacer>
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
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="navigateToEditEvent(event.id)">
        {{ t('common.edit') }}
      </v-btn>
      <v-btn color="blue-darken-1"  @click="closeView">
        {{ t('common.close') }}
      </v-btn>
    </v-card-actions>
  </v-card>
  <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
    {{ t('common.noData') }}
  </v-alert>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useEventStore } from '@/stores/event.store';
import EventForm from '@/components/events/EventForm.vue';
import type { Event } from '@/types/event/event';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const eventStore = useEventStore();

const event = ref<Event | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');

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
