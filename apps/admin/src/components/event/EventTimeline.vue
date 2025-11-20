<template>
  <v-timeline density="default" truncate-line="both">
    <v-timeline-item
      v-for="event in paginatedEvents"
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
    v-if="totalEvents === 0 && !list.loading"
    type="info"
    class="mt-4"
    variant="tonal"
  >
    {{ t('event.timeline.noEvents') }}
  </v-alert>
  <v-pagination
    v-if="totalEvents > 0"
    v-model="page"
    :length="paginationLength"
    @update:model-value="loadEvents"
    class="mt-4"
  ></v-pagination>

  <v-navigation-drawer v-model="detailDrawer" location="right" temporary width="650">
    <EventDetailView v-if="detailDrawer && selectedEventId" :event-id="selectedEventId" @close="handleDetailClosed" />
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { formatDate } from '@/utils/dateUtils';
import { useMemberLookupStore } from '@/stores/memberLookup.store';
import { useEventStore } from '@/stores/event.store'; // Import event store
import ChipLookup from '@/components/common/ChipLookup.vue';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import EventDetailView from '@/views/event/EventDetailView.vue'; // Import EventDetailView
import type { Event } from '@/types';

const props = defineProps<{
  familyId?: string; // Optional prop for family ID
  memberId?: string; // Optional prop for member ID
  readOnly?: boolean;
}>();

const memberLookupStore = useMemberLookupStore();
const eventStore = useEventStore(); // Initialize event store

const { list } = storeToRefs(eventStore);
const { t } = useI18n();

const page = ref(1);
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
const totalEvents = ref(0);
const selectedEventId = ref<string | null>(null); // Store the ID of the event being viewed
const detailDrawer = ref(false); // Control visibility of the detail drawer



const paginatedEvents = computed(() => eventStore.list.items);

const paginationLength = computed(() => {
  if (typeof totalEvents.value !== 'number' || typeof itemsPerPage.value !== 'number' || itemsPerPage.value <= 0) {
    return 1; // Default to 1 or handle error appropriately
  }
  return Math.max(1, Math.ceil(totalEvents.value / itemsPerPage.value));
});

const showEventDetails = (event: Event) => {
  selectedEventId.value = event.id;
  detailDrawer.value = true;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedEventId.value = null;
};

const loadEvents = async () => {
  if (!props.familyId && !props.memberId) {
    eventStore.list.items = [];
    eventStore.list.totalItems = 0;
    return;
  }

  const filters: any = {};
  if (props.memberId) {
    filters.relatedMemberId = props.memberId;
  } else if (props.familyId) {
    filters.familyId = props.familyId;
  }

  eventStore.list.filter = { ...eventStore.list.filter, ...filters }; // Directly update filter
  eventStore.setPage(page.value);
  eventStore.setItemsPerPage(itemsPerPage.value);
  await eventStore._loadItems(); // Call _loadItems directly
  totalEvents.value = eventStore.list.totalItems;
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
