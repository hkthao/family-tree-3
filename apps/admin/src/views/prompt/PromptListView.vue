<template>
  <div data-testid="prompt-list-view">
    <PromptList :items="promptStore.list.items" :total-items="promptStore.list.totalItems" :loading="list.loading"
      :search="searchQuery" @update:search="handleSearchUpdate" @update:options="handleListOptionsUpdate"
      @view="openDetailDrawer" @edit="openEditDrawer" @delete="confirmDelete" @create="openAddDrawer">
    </PromptList>

    <!-- Edit Prompt Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('prompt.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <PromptEditView v-if="selectedItemId && editDrawer" :prompt-id="selectedItemId as string" @close="closeEditDrawer"
        @saved="handlePromptSaved" />
    </BaseCrudDrawer>

    <!-- Add Prompt Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('prompt.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
      <PromptAddView v-if="addDrawer" @close="closeAddDrawer" @saved="handlePromptSaved" />
    </BaseCrudDrawer>

    <!-- Detail Prompt Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('prompt.detail.title')" icon="mdi-information-outline"
      @close="closeDetailDrawer">
      <PromptDetailView v-if="selectedItemId && detailDrawer" :prompt-id="selectedItemId as string"
        @close="closeDetailDrawer" @edit-prompt="openEditDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { usePromptStore } from '@/stores/prompt.store';
import { PromptList } from '@/components/prompt';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import PromptEditView from '@/views/prompt/PromptEditView.vue';
import PromptAddView from '@/views/prompt/PromptAddView.vue';
import PromptDetailView from '@/views/prompt/PromptDetailView.vue';
import type { PromptFilter, Prompt } from '@/types';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useCrudDrawer } from '@/composables/useCrudDrawer';

const { t } = useI18n();
const promptStore = usePromptStore();
const { list } = storeToRefs(promptStore);
const searchQuery = ref('');

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

const handleFilterUpdate = async (filters: PromptFilter) => {
  promptStore.list.filters = { ...filters, searchQuery: searchQuery.value };
  await promptStore._loadItems()
};

const handleSearchUpdate = async (search: string) => {
  searchQuery.value = search;
  promptStore.list.filters = { ...promptStore.list.filters, searchQuery: searchQuery.value };
  await promptStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  promptStore.setListOptions(options);
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
    await promptStore.deleteItem(prompt.id);
    if (promptStore.error) {
      showSnackbar(
        t('prompt.messages.deleteError', { error: promptStore.error }),
        'error',
      );
    } else {
      showSnackbar(
        t('prompt.messages.deleteSuccess'),
        'success',
      );
    }
  }
  promptStore._loadItems();
};

const handlePromptSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  promptStore._loadItems(); // Reload list after save
};

onMounted(() => {
  promptStore._loadItems();
})
</script>
