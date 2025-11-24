<template>
  <v-card flat>
    <v-card-title class="d-flex align-center">
      <v-btn icon @click="emit('close')" variant="text">
        <v-icon>mdi-close</v-icon>
      </v-btn>
      <span class="text-h6">{{ memory?.title || t('memory.detail.titleDefault') }}</span>
      <v-spacer></v-spacer>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn icon v-bind="props">
            <v-icon>mdi-dots-vertical</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item @click="editMemory" :disabled="loading">
            <v-list-item-title>{{ t('common.edit') }}</v-list-item-title>
          </v-list-item>
          <v-list-item @click="confirmDeleteMemory" :disabled="loading">
            <v-list-item-title>{{ t('common.delete') }}</v-list-item-title>
          </v-list-item>
          <v-list-item @click="exportPdf" :disabled="loading">
            <v-list-item-title>{{ t('memory.detail.exportPdf') }}</v-list-item-title>
          </v-list-item>
          <v-list-item @click="shareMemory" :disabled="loading">
            <v-list-item-title>{{ t('memory.detail.share') }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
    </v-card-title>

    <v-card-text v-if="loading" class="text-center">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-2">{{ t('memory.detail.loading') }}</p>
    </v-card-text>

    <v-card-text v-else-if="memory">
      <v-container>
        <v-row>
          <v-col cols="12">
            <div class="text-h5 mb-2">{{ memory.title }}</div>
            <div class="text-caption text-grey">{{ t('common.createdAt') }}: {{ formatDateTime(memory.createdAt) }}</div>
          </v-col>

          <v-col cols="12">
            <v-chip-group class="mb-2">
              <v-chip v-for="tag in memory.tags" :key="tag" color="info" size="small">{{ tag }}</v-chip>
            </v-chip-group>
            <v-chip-group class="mb-4">
              <v-chip v-for="keyword in memory.keywords" :key="keyword" color="secondary" size="small">{{ keyword }}</v-chip>
            </v-chip-group>
          </v-col>

          <v-col cols="12">
            <v-img v-if="memory.photoUrl" :src="memory.photoUrl" max-height="300" contain class="mb-4"></v-img>
          </v-col>

          <v-col cols="12">
            <p class="text-body-1">{{ memory.story }}</p>
          </v-col>

          <v-col cols="12" v-if="memory.photoAnalysisResult">
            <v-divider class="my-4"></v-divider>
            <h3 class="text-h6 mb-2">{{ t('memory.detail.photoAnalysisResult') }}</h3>
            <PhotoAnalyzerPreview :analysis-result="memory.photoAnalysisResult" :hide-actions="true" />
          </v-col>
        </v-row>
      </v-container>
    </v-card-text>
    <v-card-text v-else>
      <v-alert type="error">{{ t('memory.detail.notFound') }}</v-alert>
    </v-card-text>
  </v-card>

  <!-- Edit Memory Drawer -->
  <BaseCrudDrawer v-model="editMemoryDrawer" @close="closeEditMemory">
    <MemoryEdit v-if="editMemoryDrawer && memory" :memory-id="memory.id" @close="closeEditMemory" @saved="handleMemorySaved" />
  </BaseCrudDrawer>

  <!-- Delete Confirmation Dialog -->
  <ConfirmDialog
    v-model="deleteConfirmDialog"
    :title="t('memory.delete.confirmTitle')"
    :message="t('memory.delete.confirmMessage', { title: memory?.title })"
    @confirm="deleteMemory"
    @cancel="cancelDelete"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
// import { useSnackbarStore } from '@/stores/snackbar.store'; // Removed
import { formatDateTime } from '@/utils/formatters';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import ConfirmDialog from '@/components/common/ConfirmDialog.vue';
import PhotoAnalyzerPreview from './PhotoAnalyzerPreview.vue';
import MemoryEdit from './MemoryEdit.vue'; // Will create this component

interface Props {
  memoryId: string;
}
const props = defineProps<Props>();
const emit = defineEmits(['close', 'deleted', 'updated']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
// const snackbarStore = useSnackbarStore(); // Removed

const memory = ref<any | null>(null);
const loading = ref(false);
const editMemoryDrawer = ref(false);
const deleteConfirmDialog = ref(false);

  const loadMemory = async (id: string) => {
  loading.value = true;
  const result = await memoryStore.getById(id);
  if (result.isSuccess) {
    memory.value = result.value;
  } else {
    // snackbarStore.showSnackbar('Error loading memory: ' + result.error, 'error'); // Removed
    memory.value = null;
  }
  loading.value = false;
};

const editMemory = () => {
  editMemoryDrawer.value = true;
};

const closeEditMemory = () => {
  editMemoryDrawer.value = false;
};

const handleMemorySaved = () => {
  closeEditMemory();
  loadMemory(props.memoryId); // Reload memory details
  emit('updated');
};

const confirmDeleteMemory = () => {
  deleteConfirmDialog.value = true;
};

const deleteMemory = async () => {
  if (memory.value) {
    const result = await memoryStore.delete(memory.value.id);
    if (result.isSuccess) {
      // snackbarStore.showSnackbar(t('memory.delete.success'), 'success'); // Removed
      emit('deleted');
      emit('close'); // Close detail view after deletion
    } else {
      // snackbarStore.showSnackbar('Error deleting memory: ' + result.error, 'error'); // Removed
    }
  }
};

const cancelDelete = () => {
  deleteConfirmDialog.value = false;
};

const exportPdf = () => {
  // snackbarStore.showSnackbar('Export to PDF is not yet implemented.', 'info'); // Removed
};

const shareMemory = () => {
  // snackbarStore.showSnackbar('Share memory is not yet implemented.', 'info'); // Removed
};
onMounted(() => {
  if (props.memoryId) {
    loadMemory(props.memoryId);
  }
});

watch(
  () => props.memoryId,
  (newId) => {
    if (newId) {
      loadMemory(newId);
    } else {
      memory.value = null;
    }
  },
);
</script>

<style scoped>
/* Scoped styles for MemoryDetail */
</style>
