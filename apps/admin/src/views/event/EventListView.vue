<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="events" :total-events="totalItems" :loading="loading"
    :search="eventListSearchQuery || ''" @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate" @view="openDetailDrawer"
    @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer" />

  <!-- Add Event Drawer -->
  <BaseCrudDrawer v-model="addDrawer" @close="closeAddDrawer">
    <EventAddView v-if="addDrawer" :family-id="filters.familyId || undefined" @close="closeAddDrawer"
      @saved="handleEventSaved" />
  </BaseCrudDrawer>

  <!-- Edit Event Drawer -->
  <BaseCrudDrawer v-model="editDrawer" @close="closeEditDrawer">
    <EventEditView v-if="selectedItemId && editDrawer" :event-id="selectedItemId" @close="closeEditDrawer"
      @saved="handleEventSaved" />
  </BaseCrudDrawer>

  <!-- Detail Event Drawer -->
  <BaseCrudDrawer v-model="detailDrawer" @close="closeDetailDrawer">
    <EventDetailView v-if="selectedItemId && detailDrawer" :event-id="selectedItemId" @close="closeDetailDrawer"
      @edit="openEditDrawer" />
  </BaseCrudDrawer>
</template>

<script setup lang="ts">
import { EventSearch, EventList } from '@/components/event';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import EventEditView from '@/views/event/EventEditView.vue';
import EventDetailView from '@/views/event/EventDetailView.vue';
import { useEventList } from '@/composables'; // Import the new composable

const props = defineProps<{
  familyId: string;
  readOnly?: boolean;
}>();

const emit = defineEmits(['close', 'saved']);

const {
  eventListSearchQuery,
  filters,
  events,
  totalItems,
  loading,
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  handleFilterUpdate,
  handleSearchUpdate,
  handleListOptionsUpdate,
  confirmDelete,
  handleEventSaved,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
} = useEventList(props, emit);
</script>
