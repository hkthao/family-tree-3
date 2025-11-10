<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-detail-title">{{ t('event.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <EventForm ref="eventFormRef" v-if="event" :initial-event-data="event" :read-only="true"
        data-testid="event-detail-form" />
      <v-progress-circular v-else indeterminate color="primary"
        data-testid="event-detail-loading-spinner"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeView" data-testid="event-detail-close-button">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="editEvent" data-testid="event-detail-edit-button" v-if="canEditEvent">
        {{ t('common.edit') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventStore } from '@/stores/event.store';
import { EventForm } from '@/components/event';
import type { Event } from '@/types';
import { useAuth } from '@/composables/useAuth';

interface EventDetailViewProps {
  eventId: string;
}

const props = defineProps<EventDetailViewProps>();
const emit = defineEmits(['close', 'edit']);

const { t } = useI18n();
const eventStore = useEventStore();
const { isAdmin, isFamilyManager } = useAuth();

const event = ref<Event | undefined>(undefined);

const canEditEvent = computed(() => {
  return isAdmin.value || isFamilyManager.value;
});

const loadEvent = async () => {
  if (props.eventId) {
    await eventStore.getById(props.eventId);
    if (!eventStore.error) {
      event.value = eventStore.detail.item as Event;
    } else {
      event.value = undefined;
    }
  }
};

const closeView = () => {
  emit('close');
};

const editEvent = () => {
  if (event.value) {
    emit('edit', event.value);
  }
};

onMounted(() => {
  loadEvent();
});

watch(
  () => props.eventId,
  (newId) => {
    if (newId) {
      loadEvent();
    }
  },
);
</script>