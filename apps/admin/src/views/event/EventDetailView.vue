<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-detail-title">{{ t('event.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoadingInternal">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="errorInternal">
        <v-alert type="error" :text="errorInternal?.message || t('event.detail.errorLoading')"></v-alert>
      </div>
      <div v-else-if="currentEventData">
        <PrivacyAlert :is-private="currentEventData.isPrivate" />
        <EventForm ref="eventFormRef" v-if="currentEventData" :initial-event-data="currentEventData" :read-only="true"
          data-testid="event-detail-form" />
      </div>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeView" data-testid="event-detail-close-button">{{ t('common.close') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { watch, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { EventForm } from '@/components/event';
import { useEventQuery } from '@/composables'; // Import useEventQuery
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert
import type { EventDto } from '@/types';

interface EventDetailViewProps {
  eventId?: string; // Make eventId optional
  event?: EventDto; // New optional prop for passing EventDto directly
}

const props = defineProps<EventDetailViewProps>();
const emit = defineEmits(['close', 'edit']);
const { t } = useI18n();

const eventIdRef = toRef(props, 'eventId');
const { event: fetchedEventData, isLoading, error } = useEventQuery(eventIdRef); // Use useEventQuery

// Internal reactive state for event data
const currentEventData = computed<EventDto | undefined>(() => {
  return props.event || fetchedEventData.value;
});

const isLoadingInternal = computed(() => {
  return props.event ? false : isLoading.value;
});

const errorInternal = computed(() => {
  return props.event ? null : error.value;
});

const closeView = () => {
  emit('close');
};

watch(
  () => props.eventId,
  (newId) => {
    if (newId && !props.event) {
      // The useEventQuery composable will react to changes in eventIdRef
      // No explicit loadEvent call needed here, only fetch if event prop is not provided
    }
  },
);
</script>