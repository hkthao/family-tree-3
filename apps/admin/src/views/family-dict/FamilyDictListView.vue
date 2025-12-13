<template>
  <div data-testid="family-dict-list-view">
    <FamilyDictSearch v-if="!computedHideSearch" @update:filters="handleFilterUpdate" />

    <FamilyDictList :items="familyDicts" :total-items="totalItems" :loading="loading"
      :items-per-page="itemsPerPage" :search="searchQuery" :sortBy="sortBy"
      @update:options="handleListOptionsUpdate" @update:search="handleSearchUpdate"
      @view="item => openDetailDrawer(item.id)" @edit="item => openEditDrawer(item.id)" @delete="confirmDelete" @create="openAddDrawer"
      @import="openImportDialog" :read-only="computedReadOnly">
    </FamilyDictList>

    <!-- Edit FamilyDict Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('familyDict.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <FamilyDictEditView v-if="selectedItemId && editDrawer" :family-dict-id="selectedItemId"
        @close="closeEditDrawer" @saved="handleFamilyDictSaved" />
    </BaseCrudDrawer>

    <!-- Add FamilyDict Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('familyDict.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
      <FamilyDictAddView v-if="addDrawer" @close="closeAddDrawer" @saved="handleFamilyDictSaved" />
    </BaseCrudDrawer>

    <!-- Detail FamilyDict Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('familyDict.detail.title')" icon="mdi-information-outline" @close="closeDetailDrawer">
      <FamilyDictDetailView v-if="selectedItemId && detailDrawer" :family-dict-id="selectedItemId"
        @close="closeDetailDrawer" @edit-family-dict="item => openEditDrawer(item.id)" />
    </BaseCrudDrawer>

    <!-- Import FamilyDict Dialog -->
    <FamilyDictImportDialog :show="importDialog" @update:show="importDialog = $event"
      @imported="handleFamilyDictSaved" />
  </div>
</template>

<script setup lang="ts">
import { FamilyDictSearch, FamilyDictList } from '@/components/family-dict';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import type { FamilyDictFilter, FamilyDict } from '@/types';
import { useI18n } from 'vue-i18n';
import { ref, computed } from 'vue';

// Component Imports
import FamilyDictEditView from '@/views/family-dict/FamilyDictEditView.vue';
import FamilyDictAddView from '@/views/family-dict/FamilyDictAddView.vue';
import FamilyDictDetailView from '@/views/family-dict/FamilyDictDetailView.vue';
import FamilyDictImportDialog from '@/components/family-dict/FamilyDictImportDialog.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useFamilyDictListFilters, useFamilyDictsQuery, useDeleteFamilyDictMutation } from '@/composables/family-dict';

interface FamilyDictListViewProps {
  readOnly?: boolean;
  hideSearch?: boolean;
}
const props = defineProps<FamilyDictListViewProps>();

const { t } = useI18n();
const importDialog = ref(false); // State for import dialog

const computedHideSearch = computed(() => props.hideSearch);
const computedReadOnly = computed(() => props.readOnly);

const {
  searchQuery,
  itemsPerPage,
  sortBy,
  filters,
  setPage,
  setItemsPerPage,
  setSortBy,
  setSearchQuery,
  setFilters,
} = useFamilyDictListFilters();

const { familyDicts, totalItems, loading, refetch } = useFamilyDictsQuery(filters);
const { mutate: deleteFamilyDict } = useDeleteFamilyDictMutation();

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

const handleFilterUpdate = (newFilters: FamilyDictFilter) => {
  setFilters(newFilters);
};

const handleSearchUpdate = (search: string) => {
  setSearchQuery(search);
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

const openImportDialog = () => {
  importDialog.value = true;
};

const confirmDelete = async (familyDict: FamilyDict) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyDict.list.confirmDelete', { name: familyDict.name || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    deleteFamilyDict(familyDict.id, {
      onSuccess: () => {
        showSnackbar(
          t('familyDict.messages.deleteSuccess'),
          'success',
        );
        refetch(); // Refetch the list after successful deletion
      },
      onError: (error) => {
        showSnackbar(
          error.message || t('familyDict.messages.deleteError'),
          'error',
        );
      },
    });
  }
};

const handleFamilyDictSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  refetch(); // Reload list after save
};
</script>
