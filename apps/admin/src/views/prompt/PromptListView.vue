<template>
  <div data-testid="prompt-list-view">
    <PromptList :items="items" :total-items="totalItems" :loading="loading"
      :items-per-page="listOptions.itemsPerPage" :page="listOptions.page" :search="searchQuery" @update:search="handleSearchUpdate"
      @update:options="handleListOptionsUpdate" @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete"
      @create="openAddDrawer">
      <template #top>
        <ListToolbar
          :title="t('prompt.list.title')"
          :create-button-tooltip="t('prompt.management.addPrompt')"
          create-button-test-id="add-new-prompt-button"
          :hide-create-button="!canPerformActions"
          :search-query="searchQuery"
          :search-label="t('common.search')"
          @create="openAddDrawer"
          @update:search="handleSearchUpdate"
        >
          <template #custom-buttons>
            <!-- Export Button -->
            <v-btn color="secondary" class="mr-2" :loading="isExporting" :disabled="!canPerformActions" @click="exportPrompts" icon>
              <v-tooltip :text="t('common.export')">
                <template v-slot:activator="{ props }">
                  <v-icon v-bind="props">mdi-export</v-icon>
                </template>
              </v-tooltip>
            </v-btn>
            <!-- Import Button -->
            <v-btn color="secondary" class="mr-2" :loading="isImporting" :disabled="!canPerformActions" @click="importDialog = true" icon>
              <v-tooltip :text="t('common.import')">
                <template v-slot:activator="{ props }">
                  <v-icon v-bind="props">mdi-import</v-icon>
                </template>
              </v-tooltip>
            </v-btn>
          </template>
        </ListToolbar>
      </template>
    </PromptList>

    <!-- Import Dialog -->
    <BaseImportDialog
      v-model="importDialog"
      :title="t('prompt.import.title')"
      :label="t('prompt.import.selectFile')"
      :loading="isImporting"
      :max-file-size="5 * 1024 * 1024"
      @update:model-value="importDialog = $event"
      @import="triggerImport"
    />

    <v-alert v-if="listError" type="error" dismissible class="mt-4">
      {{ listError?.message || t('prompt.list.loadError') }}
    </v-alert>

    <!-- Edit Prompt Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('prompt.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <PromptEditView v-if="selectedItemId && editDrawer" :prompt-id="selectedItemId" @close="closeEditDrawer"
        @saved="handlePromptSaved" />
    </BaseCrudDrawer>

    <!-- Add Prompt Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('prompt.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
      <PromptAddView v-if="addDrawer" @close="closeAddDrawer" @saved="handlePromptSaved" />
    </BaseCrudDrawer>

    <!-- Detail Prompt Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('prompt.detail.title')" icon="mdi-information-outline"
      @close="closeDetailDrawer">
      <PromptDetailView v-if="selectedItemId && detailDrawer" :prompt-id="selectedItemId" @close="closeDetailDrawer"
        @edit-prompt="openEditDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { PromptList } from '@/components/prompt';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import type { Prompt } from '@/types';
import { useI18n } from 'vue-i18n';
import { ref, reactive, computed } from 'vue';
import { usePromptsQuery, useDeletePromptMutation } from '@/composables';
import { usePromptImportExport } from '@/composables/prompt/usePromptImportExport';
import type { PromptListOptions } from '@/composables';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import PromptAddView from './PromptAddView.vue';
import PromptEditView from './PromptEditView.vue';
import PromptDetailView from './PromptDetailView.vue';
import ListToolbar from '@/components/common/ListToolbar.vue';
import BaseImportDialog from '@/components/common/BaseImportDialog.vue'; // Added

const { t } = useI18n();

const searchQuery = ref('');

const listOptions = reactive<PromptListOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  searchQuery: searchQuery.value,
});

const { state: { prompts: promptsData, totalItems, isLoading: isListLoading, error: listError }, actions: { refetch: refetchPrompts } } = usePromptsQuery(listOptions);
const { state: { isPending: isDeletingPrompt }, actions: { deletePrompt } } = useDeletePromptMutation();
const { isExporting, isImporting, exportPrompts, importPrompts } = usePromptImportExport();

const importDialog = ref(false);

const triggerImport = async (file: File) => { // Modified to accept file directly
  if (!file) {
    showSnackbar(t('prompt.messages.noFileSelected'), 'warning');
    return;
  }

  const reader = new FileReader();
  reader.onload = async (e) => {
    try {
      const jsonContent = JSON.parse(e.target?.result as string);
      const success = await importPrompts(jsonContent);
      if (success) {
        importDialog.value = false;
        refetchPrompts(); // Refetch the list after successful import
      }
    } catch (error: any) {
      showSnackbar(error.message || t('prompt.messages.invalidJson'), 'error');
    }
  };
  reader.readAsText(file); // Use the passed file
};

const items = computed(() => promptsData.value || []);

const loading = computed(() => isListLoading.value || isDeletingPrompt.value);

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAddDrawer,
  closeEditDrawer,
  closeDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

import { useAuth } from '@/composables';
const { state: authState } = useAuth();

const canPerformActions = computed(() => authState.isAdmin.value);

const handleSearchUpdate = (search: string) => {
  searchQuery.value = search;
  listOptions.searchQuery = search;
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: 'asc' | 'desc' }[];
}) => {
  listOptions.page = options.page;
  listOptions.itemsPerPage = options.itemsPerPage;
  listOptions.sortBy = options.sortBy;
};

const confirmDelete = async (prompt: Prompt) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('prompt.list.confirmDelete', { title: prompt.title || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    await handleDeleteConfirm(prompt);
  }
};

const handleDeleteConfirm = async (prompt: Prompt) => {
  if (prompt) {
    try {
      await deletePrompt(prompt.id);
      showSnackbar(t('prompt.messages.deleteSuccess'), 'success');
    } catch (error) {
      showSnackbar((error as Error).message || t('prompt.messages.deleteError'), 'error');
    }
  }
};

const handlePromptSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  // Query invalidation handles refetching
};
</script>
