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
          <ChipLookup
            v-if="event.relatedMembers && event.relatedMembers.length > 0"
            :model-value="event.relatedMembers"
            :data-source="memberLookupStore"
            display-expr="fullName"
            value-expr="id"
            image-expr="avatarUrl"
            class="mt-1"
          />
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
import { ref, computed, watch } from 'vue';
import { formatDate } from '@/utils/dateUtils';
import { useMemberLookupStore } from '@/stores/memberLookup.store';
import { useEventTimelineStore } from '@/stores/eventTimeline.store'; // Import new store
import ChipLookup from '@/components/common/ChipLookup.vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import EventDetailView from '@/views/event/EventDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // Import BaseCrudDrawer
import type { Event } from '@/types';

const props = defineProps<{
  familyId?: string;
  memberId?: string;
  readOnly?: boolean;
}>();

const memberLookupStore = useMemberLookupStore();
const eventTimelineStore = useEventTimelineStore(); // Use new store

const { list } = storeToRefs(eventTimelineStore); // Get list state from new store
const { t } = useI18n();

// const page = ref(1); // Removed
// const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE); // Removed
// const totalEvents = ref(0); // Removed
const selectedEventId = ref<string | null>(null);
const detailDrawer = ref(false);

// const paginatedEvents = computed(() => list.items);

const paginationLength = computed(() => {
  return Math.max(1, list.value.totalPages);
});

const showEventDetails = (event: Event) => {
  selectedEventId.value = event.id;
  detailDrawer.value = true;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedEventId.value = null;
};

const handlePageChange = (newPage: number) => {
  eventTimelineStore.setListOptions({
    page: newPage,
    itemsPerPage: list.value.itemsPerPage,
    sortBy: list.value.sortBy,
  });
};

watch(
  [() => props.familyId, () => props.memberId],
  ([newFamilyId, newMemberId]) => {
    eventTimelineStore.setFilters({ familyId: newFamilyId, memberId: newMemberId });
  },
  { immediate: true },
);
</script>

<style scoped>
/* Add any specific styles for the timeline here */
</style>
