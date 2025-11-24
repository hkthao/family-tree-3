<template>
  <div data-testid="family-dict-list-view">
    <FamilyDictSearch v-if="!props.hideSearch" @update:filters="handleFilterUpdate" />

    <FamilyDictList :items="familyDictStore.list.items" :total-items="familyDictStore.list.totalItems"
      :loading="list.loading" :search="searchQuery" @update:search="handleSearchUpdate"
      @update:options="handleListOptionsUpdate" @view="openDetailDrawer" @edit="openEditDrawer"
      @delete="confirmDelete" @create="openAddDrawer" @import="openImportDialog" :read-only="props.readOnly">
    </FamilyDictList>

    <!-- Edit FamilyDict Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('familyDict.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <FamilyDictEditView v-if="selectedItemId && editDrawer" :family-dict-id="selectedItemId as string"
        @close="closeEditDrawer" @saved="handleFamilyDictSaved" />
    </BaseCrudDrawer>

    <!-- Add FamilyDict Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('familyDict.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
      <FamilyDictAddView v-if="addDrawer" @close="closeAddDrawer" @saved="handleFamilyDictSaved" />
    </BaseCrudDrawer>

    <!-- Detail FamilyDict Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('familyDict.detail.title')" icon="mdi-information-outline" @close="closeDetailDrawer">
      <FamilyDictDetailView v-if="selectedItemId && detailDrawer" :family-dict-id="selectedItemId as string"
        @close="closeDetailDrawer" @edit-family-dict="openEditDrawer" />
    </BaseCrudDrawer>

    <!-- Import FamilyDict Dialog -->
    <FamilyDictImportDialog :show="importDialog" @update:show="importDialog = $event"
      @imported="handleFamilyDictSaved" />
  </div>
</template>

<script setup lang="ts">
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { FamilyDictSearch, FamilyDictList } from '@/components/family-dict';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import FamilyDictEditView from '@/views/family-dict/FamilyDictEditView.vue';
import FamilyDictAddView from '@/views/family-dict/FamilyDictAddView.vue';
import FamilyDictDetailView from '@/views/family-dict/FamilyDictDetailView.vue';
import FamilyDictImportDialog from '@/components/family-dict/FamilyDictImportDialog.vue'; // Import FamilyDictImportDialog
import type { FamilyDictFilter, FamilyDict } from '@/types';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import

interface FamilyDictListViewProps {
  readOnly?: boolean;
  hideSearch?: boolean;
}

const props = defineProps<FamilyDictListViewProps>();

const { t } = useI18n();
const familyDictStore = useFamilyDictStore();
const { list } = storeToRefs(familyDictStore);
const searchQuery = ref('');
const importDialog = ref(false); // State for import dialog

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

const handleFilterUpdate = async (filters: FamilyDictFilter) => {
  familyDictStore.list.filters = { ...filters, searchQuery: searchQuery.value };
  await familyDictStore._loadItems()
};

const handleSearchUpdate = async (search: string) => {
  searchQuery.value = search;
  familyDictStore.list.filters = { ...familyDictStore.list.filters, searchQuery: searchQuery.value };
  await familyDictStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  familyDictStore.setListOptions(options);
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
    await handleDeleteConfirm(familyDict);
  }
};

const handleDeleteConfirm = async (familyDict: FamilyDict) => {
  if (familyDict) {
    await familyDictStore.deleteItem(familyDict.id);
    if (familyDictStore.error) {
      showSnackbar(
        t('familyDict.messages.deleteError', { error: familyDictStore.error }),
        'error',
      );
    } else {
      showSnackbar(
        t('familyDict.messages.deleteSuccess'),
        'success',
      );
    }
  }
  familyDictStore._loadItems();
};

const handleFamilyDictSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  familyDictStore._loadItems(); // Reload list after save
};

onMounted(() => {
  familyDictStore._loadItems();
})

</script>
