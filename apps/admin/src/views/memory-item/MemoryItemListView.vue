<template>
  <div data-testid="memory-item-list-view">
    <ListToolbar :title="t('memoryItem.list.title')" :create-button-tooltip="t('memoryItem.list.add')"
      create-button-test-id="add-memory-item-button" :search-query="filters.searchTerm"
      :search-label="t('common.search')" @create="openAddDrawer()" @update:search="handleSearchUpdate" />
    <v-data-table-server :items="memoryItems" :items-length="totalItems" :loading="isLoadingMemoryItems"
      :items-per-page="paginationOptions.itemsPerPage" :page="paginationOptions.page" :headers="headers"
      @update:options="handleListOptionsUpdate">
      <template v-slot:item.happenedAt="{ item }">
        {{ item.happenedAt ? formatDate(item.happenedAt) : '' }}
      </template>
      <template v-slot:item.actions="{ item }">
        <v-icon small class="mr-2" @click="openDetailDrawer(item.id)">mdi-eye</v-icon>
        <v-icon small class="mr-2" @click="openEditDrawer(item.id)">mdi-pencil</v-icon>
        <v-icon small @click="confirmDelete(item.id)">mdi-delete</v-icon>
      </template>
    </v-data-table-server>

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
import { useMemoryItemDataManagement, useMemoryItemsQuery, useDeleteMemoryItemMutation } from '@/composables/memory-item';
import { useQueryClient } from '@tanstack/vue-query';
import type { MemoryItem } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import ListToolbar from '@/components/common/ListToolbar.vue';
import MemoryItemAddView from './MemoryItemAddView.vue';
import MemoryItemEditView from './MemoryItemEditView.vue';
import MemoryItemDetailView from './MemoryItemDetailView.vue';
import dayjs from 'dayjs';

interface MemoryItemListViewProps {
  familyId: string;
}

const props = defineProps<MemoryItemListViewProps>();
const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const {
  paginationOptions,
  filters,
  setPage,
  setItemsPerPage,
  setSortBy,
} = useMemoryItemDataManagement(computed(() => props.familyId));

const { data: memoryItemsData, isLoading: isLoadingMemoryItems } = useMemoryItemsQuery(
  computed(() => props.familyId),
  paginationOptions,
  filters
);

const memoryItems = ref<MemoryItem[]>(memoryItemsData.value?.items || []);
const totalItems = ref(memoryItemsData.value?.totalItems || 0);

watch(memoryItemsData, (newData) => {
  memoryItems.value = newData?.items || [];
  totalItems.value = newData?.totalItems || 0;
}, { deep: true });

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

const headers = ref([
  { title: t('memoryItem.list.col.title'), key: 'title' },
  { title: t('memoryItem.list.col.description'), key: 'description' },
  { title: t('memoryItem.list.col.happenedAt'), key: 'happenedAt' },
  { title: t('memoryItem.list.col.emotionalTag'), key: 'emotionalTag' },
  { title: t('common.actions'), key: 'actions', sortable: false },
]);

const handleSearchUpdate = (value: string | null) => {
  filters.value.searchTerm = value || '';
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

const formatDate = (dateString: string | Date) => {
  return dayjs(dateString).format('DD/MM/YYYY');
};
</script>

<style scoped></style>