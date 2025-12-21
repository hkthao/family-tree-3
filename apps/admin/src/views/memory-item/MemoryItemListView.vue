<template>
  <div data-testid="memory-item-list-view">
    <MemoryItemList
      :items="memoryItems"
      :total-items="currentTotalItems"
      :loading="isLoadingMemoryItems"
      :family-id="props.familyId"
      @update:options="handleListOptionsUpdate"
      @create="openAddDrawer()"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      :allow-add="true"
      :allow-edit="true"
      :allow-delete="true"
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

interface MemoryItemListViewProps {
  familyId: string;
}

const props = defineProps<MemoryItemListViewProps>();
const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const {
  state: { paginationOptions, filters },
  actions: { setPage, setItemsPerPage, setSortBy },
} = useMemoryItemDataManagement(computed(() => props.familyId));

const { state: { memberFaces, totalItems, isLoading: isLoadingMemoryItems } } = useMemoryItemsQuery(
  computed(() => props.familyId),
  paginationOptions,
  filters
);

const memoryItems = ref<MemoryItem[]>(memberFaces.value || []);
const currentTotalItems = ref(totalItems.value || 0);

watch(memberFaces, (newData) => {
  memoryItems.value = newData || [];
});

watch(totalItems, (newTotal) => {
  currentTotalItems.value = newTotal || 0;
});

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
      queryClient.invalidateQueries({ queryKey: ['family', props.familyId, 'memory-items'] });
    },
    onError: (error) => {
      showSnackbar(error.message || t('memoryItem.messages.deleteError'), 'error');
    },
  });
};

const handleMemoryItemSaved = () => {
  closeAllDrawers();
  queryClient.invalidateQueries({ queryKey: ['family', props.familyId, 'memory-items'] });
};

const handleMemoryItemClosed = () => {
  closeAllDrawers();
};

</script>

<style scoped></style>