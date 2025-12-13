<template>
  <v-timeline density="default" truncate-line="both">
    <v-timeline-item
      v-for="event in list.items"
      :key="event.id"
      :dot-color="event.color || 'primary'"
      size="small"
      @click="showEventDetails(event)"
      class="cursor-pointer"
    >
      <template v-slot:opposite>
        {{ formatDate(event.startDate) }}
      </template>
      <v-card :color="event.color || 'primary'" variant="tonal" class="pa-0">
        <v-card-title class="d-flex align-center">
          <span class="text-subtitle-1">{{ event.name }}</span>
          <v-spacer></v-spacer>
          <v-btn icon size="small" variant="text" :color="event.color || 'primary'">
            <v-icon>mdi-arrow-right</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text class="py-1">
          <div v-if="event.location" class="text-body-2 text-grey-darken-1">
            <v-icon size="small">mdi-map-marker</v-icon> {{ event.location }}
          </div>
          <div v-if="event.relatedMembers && event.relatedMembers.length > 0" class="mt-1">
            <MemberName
              v-for="member in event.relatedMembers"
              :key="member.id"
              :full-name="member.fullName"
              :avatar-url="member.avatarUrl"
              :gender="member.gender"
            />
          </div>
        </v-card-text>
      </v-card>
    </v-timeline-item>
  </v-timeline>
  <v-alert
    v-if="list.totalItems === 0 && !list.loading"
    type="info"
    class="mt-4"
    variant="tonal"
  >
    {{ t('event.timeline.noEvents') }}
  </v-alert>
  <v-pagination
    v-if="list.totalItems > 0"
    v-model="list.currentPage"
    :length="paginationLength"
    @update:model-value="handlePageChange"
    class="mt-4"
  ></v-pagination>

  <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
    <EventDetailView v-if="detailDrawer && selectedEventId" :event-id="selectedEventId" @close="handleDetailClosed" />
  </BaseCrudDrawer>
</template>

<script setup lang="ts">

import EventDetailView from '@/views/event/EventDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import MemberName from '@/components/member/MemberName.vue'; // Import MemberName
import { useEventTimeline } from '@/composables/event/useEventTimeline';

const props = defineProps<{
  familyId?: string;
  memberId?: string;
  readOnly?: boolean;
}>();

const {
  t,
  list,
  selectedEventId,
  detailDrawer,
  paginationLength,
  showEventDetails,
  handleDetailClosed,
  handlePageChange,
  formatDate,
} = useEventTimeline(props);
</script>

<style scoped>
/* Add any specific styles for the timeline here */
</style>
