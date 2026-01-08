<template>
  <EventSearch @update:filters="handleFilterUpdate" />
  <EventList :events="events" :total-events="totalItems" :loading="loading"
    :search="eventListSearchQuery || ''" @update:options="handleListOptionsUpdate"
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
    @generateOccurrences="handleGenerateOccurrences" /> <!-- NEW -->

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
    @import="triggerImport"
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
import { useEventImportExport } from '@/composables/event/useEventImportExport';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { ref, computed } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { useEventService } from '@/services/event.service'; // NEW

const props = defineProps<{
  familyId: string;
  readOnly?: boolean;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const authStore = useAuthStore();

const importDialog = ref(false);

const { isExporting, isImporting, exportEvents, importEvents } = useEventImportExport(ref(props.familyId));
const eventService = useEventService(); // NEW

const {
  state,
  actions,
} = useEventList(props, emit);

const { refetchEvents } = actions;

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
  handleEventSaved: originalHandleEventSaved,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
} = actions;

const triggerImport = async (file: File) => {
  if (!file) {
    showSnackbar(t('event.messages.noFileSelected'), 'warning');
    return;
  }

  const reader = new FileReader();
  reader.onload = async (e) => {
    try {
      const jsonContent = JSON.parse(e.target?.result as string);
      await importEvents(jsonContent);
      importDialog.value = false;
      refetchEvents();
    } catch (error: any) {
      console.error("Import operation failed:", error);
    }
  };
  reader.readAsText(file);
};

const handleEventSaved = () => {
  originalHandleEventSaved();
  refetchEvents();
};

const isAdmin = computed(() => authStore.isAdmin);

const isGeneratingOccurrences = ref(false); // NEW

const handleGenerateOccurrences = async (year: number, familyId: string) => { // NEW
  isGeneratingOccurrences.value = true;
  const result = await eventService.generateEventOccurrences(year, familyId);
  if (result.ok) {
    showSnackbar(t('event.list.action.generateOccurrencesSuccess'), 'success');
    refetchEvents(); // Reload events
  } else {
    showSnackbar(result.error?.message || t('event.list.action.generateOccurrencesError'), 'error');
  }
  isGeneratingOccurrences.value = false;
};
</script>