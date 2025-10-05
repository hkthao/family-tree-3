<template>
  <v-timeline truncate-line="both">
    <v-timeline-item
      v-for="event in paginatedEvents"
      :key="event.id"
      :dot-color="event.color || 'primary'"
      size="small"
    >
      <template v-slot:opposite>
        <div
          :style="{ color: event.color || 'primary' }"
          class="text-subtitle-1"
        >
          {{ formatDate(event.startDate) }}
        </div>
      </template>
      <div class="d-flex justify-space-between flex-wrap">
        <div class="text-h6" :style="{ color: event.color || 'primary' }">
          {{ event.name }}
        </div>
      </div>
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
  <v-pagination
    v-if="totalEvents > 0"
    v-model="page"
    :length="Math.max(1, Math.ceil(totalEvents / itemsPerPage))"
    @update:model-value="loadEvents"
    class="mt-4"
  ></v-pagination>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { formatDate } from '@/utils/dateUtils';
import { useMemberStore } from '@/stores/member.store';
import { useEventStore } from '@/stores/event.store'; // Import event store
import ChipLookup from '@/components/common/ChipLookup.vue';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

const props = defineProps<{
  familyId?: string; // Optional prop for family ID
  memberId?: string; // Optional prop for member ID
  readOnly?: boolean;
}>();

const memberStore = useMemberStore();
const eventStore = useEventStore(); // Initialize event store

const page = ref(1);
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
const totalEvents = ref(0);

const paginationLength = computed(() => {
  if (typeof totalEvents.value !== 'number' || typeof itemsPerPage.value !== 'number' || itemsPerPage.value <= 0) {
    return 1; // Default to 1 or handle error appropriately
  }
  return Math.max(1, Math.ceil(totalEvents.value / itemsPerPage.value));
});

const loadEvents = async () => {
  if (!props.familyId && !props.memberId) {
    eventStore.items = [];
    eventStore.totalItems = 0;
    return;
  }

  const filters: any = {};
  if (props.memberId) {
    filters.relatedMemberId = props.memberId;
  } else if (props.familyId) {
    filters.familyId = props.familyId;
  }

  eventStore.filter = { ...eventStore.filter, ...filters }; // Directly update filter
  eventStore.setPage(page.value);
  eventStore.setItemsPerPage(itemsPerPage.value);
  await eventStore._loadItems(); // Call _loadItems directly
  totalEvents.value = eventStore.totalItems;
};

watch(
  [page, itemsPerPage, () => props.familyId, () => props.memberId],
  () => {
    loadEvents();
  },
  { immediate: true },
);
</script>

<style scoped>
/* Add any specific styles for the timeline here */
</style>
