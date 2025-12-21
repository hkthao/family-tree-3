<template>
  <div data-testid="prompt-list-view">
    <PromptList :items="items" :total-items="totalItems" :loading="loading"
      :items-per-page="(listOptions.itemsPerPage as number)" :search="searchQuery" @update:search="handleSearchUpdate" @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer">
    </PromptList>

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
import type { PromptListOptions } from '@/composables';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import PromptAddView from './PromptAddView.vue';
import PromptEditView from './PromptEditView.vue';
import PromptDetailView from './PromptDetailView.vue';

const { t } = useI18n();

const searchQuery = ref('');

const listOptions = reactive<PromptListOptions>({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
  searchQuery: searchQuery.value,
});

const { state: { prompts: promptsData, totalItems, isLoading: isListLoading, error: listError }, actions: { refetch } } = usePromptsQuery(listOptions);
const { state: { isPending: isDeletingPrompt }, actions: { deletePrompt } } = useDeletePromptMutation();

const items = computed(() => promptsData.value || []);
const listTotalItems = computed(() => totalItems.value || 0);
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
