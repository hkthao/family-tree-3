<template>
  <div data-testid="memory-item-list-view">
    <MemoryItemList
      :items="memoryItems"
      :total-items="currentTotalItems"
      :loading="isLoadingMemoryItems"
      :family-id="props.familyId"
      :page="paginationOptions.page"
      :items-per-page="paginationOptions.itemsPerPage"
      @update:options="handleListOptionsUpdate"
      @create="openAddDrawer()"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      :allow-add="true"
      :allow-edit="true"
      :allow-delete="true"
      :is-exporting="isExporting"
      :is-importing="isImporting"
      :can-perform-actions="true"
      :on-export="exportMemoryItems"
      :on-import-click="() => importDialog = true"
    />

    <!-- Add Memory Item Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleMemoryItemClosed">
      <MemoryItemAddView v-if="addDrawer" :family-id="props.familyId" @close="handleMemoryItemClosed"
        @saved="handleMemoryItemSaved" />
    </BaseCrudDrawer>

    <!-- Edit Memory Item Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleMemoryItemClosed">
      <MemoryItemEditView v-if="selectedItemId && editDrawer" :family-id="props.familyId"
        :memory-item-id="selectedItemId" @close="handleMemoryItemClosed" @saved="handleMemoryItemSaved" />
    </BaseCrudDrawer>

    <!-- Detail Memory Item Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleMemoryItemClosed">
      <MemoryItemDetailView v-if="selectedItemId && detailDrawer" :family-id="props.familyId"
        :memory-item-id="selectedItemId" @close="handleMemoryItemClosed" />
    </BaseCrudDrawer>

    <!-- Import Dialog -->
    <BaseImportDialog
      v-model="importDialog"
      :title="t('memoryItem.import.title')"
      :label="t('memoryItem.import.selectFile')"
      :loading="isImporting"
      :max-file-size="5 * 1024 * 1024"
      @update:model-value="importDialog = $event"
      @import="triggerImport"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useCrudDrawer, useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useMemoryItemDataManagement, useMemoryItemsQuery, useDeleteMemoryItemMutation } from '@/composables';
import { useQueryClient } from '@tanstack/vue-query';
import type { MemoryItem } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import MemoryItemList from '@/components/memory-item/MemoryItemList.vue';
import MemoryItemAddView from './MemoryItemAddView.vue';
import MemoryItemEditView from './MemoryItemEditView.vue';
import MemoryItemDetailView from './MemoryItemDetailView.vue';
import BaseImportDialog from '@/components/common/BaseImportDialog.vue';
import { useMemoryItemImportExport } from '@/composables/memory-item/useMemoryItemImportExport'; // New import

interface MemoryItemListViewProps {
  familyId: string;
}

const props = defineProps<MemoryItemListViewProps>();
const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const importDialog = ref(false);

const { isExporting, isImporting, exportMemoryItems, importMemoryItems } = useMemoryItemImportExport(computed(() => props.familyId));

const {
  state: { paginationOptions, filters },
  actions: { setPage, setItemsPerPage, setSortBy },
} = useMemoryItemDataManagement(computed(() => props.familyId));

const {
  state: { memoryItems: queryMemoryItems, totalItems, isLoading: isLoadingMemoryItems },
  actions: { refetch },
} = useMemoryItemsQuery(
  computed(() => props.familyId),
  paginationOptions,
  filters
);

const memoryItems = computed(() => queryMemoryItems.value || []);
const currentTotalItems = computed(() => totalItems.value || 0);

const { mutate: deleteMemoryItem } = useDeleteMemoryItemMutation();

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const triggerImport = async (file: File) => {
  if (!file) {
    showSnackbar(t('memoryItem.messages.noFileSelected'), 'warning');
    return;
  }

  const reader = new FileReader();
  reader.onload = async (e) => {
    try {
      const jsonContent = JSON.parse(e.target?.result as string);
      await importMemoryItems(jsonContent);
      importDialog.value = false;
      refetch();
    } catch (error: any) {
      console.error("Import operation failed:", error);
    }
  };
  reader.readAsText(file);
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  setPage(options.page);
  setItemsPerPage(options.itemsPerPage);
  setSortBy(options.sortBy as { key: string; order: 'asc' | 'desc' }[]);
};

const confirmDelete = async (id: string) => {
  const itemToDelete = memoryItems.value.find(item => item.id === id);
  if (!itemToDelete) {
    showSnackbar(t('memoryItem.messages.notFound'), 'error');
    return;
  }

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('memoryItem.list.confirmDelete', { title: itemToDelete.title }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    handleDeleteConfirm(itemToDelete.id);
  }
};

const handleDeleteConfirm = (id: string) => {
  deleteMemoryItem({ familyId: props.familyId, id }, {
    onSuccess: () => {
      showSnackbar(t('memoryItem.messages.deleteSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['memory-items'] });
    },
    onError: (error) => {
      showSnackbar(error.message || t('memoryItem.messages.deleteError'), 'error');
    },
  });
};

const handleMemoryItemSaved = () => {
  closeAllDrawers();
  queryClient.invalidateQueries({ queryKey: ['memory-items'] });
};

const handleMemoryItemClosed = () => {
  closeAllDrawers();
};

</script>

<style scoped></style>