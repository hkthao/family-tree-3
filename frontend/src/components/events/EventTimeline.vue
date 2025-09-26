<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.timeline.title') }}
      <v-spacer></v-spacer>
    </v-card-title>
    <v-card-text>
      <v-timeline density="compact" side="end" truncate-line="both">
        <v-timeline-item
          v-for="event in paginatedEvents"
          :key="event.id"
          :dot-color="event.color || 'primary'"
          size="small"
        >
          <div class="d-flex justify-space-between flex-wrap">
            <div class="text-h6">{{ event.name }}</div>
            <div class="text-caption text-grey">{{ formatDate(event.startDate) }}</div>
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
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event/event';
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

const { t } = useI18n();
const memberStore = useMemberStore();
const eventStore = useEventStore(); // Initialize event store

const page = ref(1);
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
const totalEvents = ref(0);

const paginatedEvents = computed(() => eventStore.items);

const loadEvents = async () => {
  if (!props.familyId && !props.memberId) {
    eventStore.items = [];
    eventStore.totalItems = 0;
    return;
  }

  eventStore.setPage(page.value);
  eventStore.setItemsPerPage(itemsPerPage.value);

  const filters: any = {};
  if (props.memberId) {
    filters.relatedMemberId = props.memberId;
  } else if (props.familyId) {
    filters.familyId = props.familyId;
  }

  await eventStore.searchItems(filters);
  totalEvents.value = eventStore.totalItems;
};

watch([page, itemsPerPage, () => props.familyId, () => props.memberId], () => {
  loadEvents();
}, { immediate: true });


</script>

<style scoped>
/* Add any specific styles for the timeline here */
</style>
