<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.timeline.title') }}
    </v-card-title>
    <v-card-text>
      <v-timeline density="compact" side="end" truncate-line="both">
        <v-timeline-item
          v-for="event in events"
          :key="event.id"
          :dot-color="event.color || 'primary'"
          size="small"
        >
          <div class="d-flex justify-space-between flex-wrap">
            <div class="text-h6">{{ event.name }}</div>
            <div class="text-caption text-grey">{{ formatDate(event.startDate) }}</div>
          </div>
          <div class="text-body-2">{{ event.description }}</div>
          <div v-if="event.location" class="text-caption text-grey">
            <v-icon size="small">mdi-map-marker</v-icon> {{ event.location }}
          </div>
          <ChipLookup
            v-if="event.relatedMembers && event.relatedMembers.length > 0"
            :model-value="event.relatedMembers"
            :data-source="memberStore"
            display-expr="fullName"
            value-expr="id"
            image-expr="avatarUrl"
          />
        </v-timeline-item>
      </v-timeline>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event/event';
import { formatDate } from '@/utils/dateUtils';
import { useMemberStore } from '@/stores/member.store';
import ChipLookup from '@/components/common/ChipLookup.vue';

const { events } = defineProps<{
  events: Event[];
}>();

const { t } = useI18n();
const memberStore = useMemberStore();

</script>

<style scoped>
/* Add any specific styles for the timeline here */
</style>
