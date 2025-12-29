<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase" data-testid="event-detail-title">{{ t('event.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('event.detail.errorLoading')"></v-alert>
      </div>
      <div v-else-if="eventData">
        <PrivacyAlert :is-private="eventData.isPrivate" />
        <EventForm ref="eventFormRef" v-if="eventData" :initial-event-data="eventData" :read-only="true"
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
import { watch, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { EventForm } from '@/components/event';
import { useEventQuery } from '@/composables'; // Import useEventQuery
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert

interface EventDetailViewProps {
  eventId: string;
}

const props = defineProps<EventDetailViewProps>();
const emit = defineEmits(['close', 'edit']);
const { t } = useI18n();

const eventIdRef = toRef(props, 'eventId');
const { event: eventData, isLoading, error } = useEventQuery(eventIdRef); // Use useEventQuery

const closeView = () => {
  emit('close');
};

watch(
  () => props.eventId,
  (newId) => {
    if (newId) {
      // The useEventQuery composable will react to changes in eventIdRef
      // No explicit loadEvent call needed here
    }
  },
);
</script>