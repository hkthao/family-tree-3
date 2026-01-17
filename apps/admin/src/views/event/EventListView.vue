<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="events" :total-events="totalItems" :loading="loading"
    :search="eventListSearchQuery || ''" :page="filters.page" :items-per-page="filters.itemsPerPage" @update:options="handleListOptionsUpdate"
    @update:search="handleSearchUpdate" @view="openDetailDrawer"
    @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer"
    :is-exporting="isExporting"
    :is-importing="isImporting"
    :can-perform-actions="true"
    :on-export="exportEvents"
    :on-import-click="() => importDialog = true"
    :is-admin="isAdmin"
    :family-id="props.familyId"
    :is-generating-occurrences="isGeneratingOccurrences"
    @generateOccurrences="handleGenerateOccurrences"
    @sendNotification="handleSendNotification"
    :is-sending-notification="isSendingNotification"/>

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

  <!-- Import Dialog -->
  <BaseImportDialog
    v-model="importDialog"
    :title="t('event.import.title')"
    :label="t('event.import.selectFile')"
    :loading="isImporting"
    :max-file-size="5 * 1024 * 1024"
    @update:model-value="importDialog = $event"
    @import="actionsTriggerImport"
  />
</template>

<script setup lang="ts">
import { EventSearch, EventList } from '@/components/event';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import EventEditView from '@/views/event/EventEditView.vue';
import EventDetailView from '@/views/event/EventDetailView.vue';
import BaseImportDialog from '@/components/common/BaseImportDialog.vue';
import { useEventList } from '@/composables';
import { useI18n } from 'vue-i18n';
import { useEventActions } from '@/composables/event/useEventActions'; // NEW

const props = defineProps<{
  familyId: string;
  readOnly?: boolean;
}>();

const emit = defineEmits(['close', 'saved']);
const { t } = useI18n();

const {
  state,
  actions,
} = useEventList(props, emit);

const { refetchEvents, handleEventSaved: originalHandleEventSaved } = actions;

const {
  importDialog,
  isExporting,
  isImporting,
  isGeneratingOccurrences,
  isSendingNotification,
  isAdmin,
  exportEvents,
  triggerImport: actionsTriggerImport, // Rename to avoid conflict with local triggerImport
  handleGenerateOccurrences,
  handleSendNotification,
} = useEventActions(props, emit, refetchEvents);

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
} = state;

const {
  handleFilterUpdate,
  handleSearchUpdate,
  handleListOptionsUpdate,
  confirmDelete,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
} = actions;

const handleEventSaved = () => {
  originalHandleEventSaved();
  refetchEvents();
};
</script>