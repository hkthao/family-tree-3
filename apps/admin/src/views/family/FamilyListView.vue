<template>
  <div data-testid="family-list-view">
    <FamilySearch id="tour-step-1" @update:filters="actions.handleFilterUpdate" />
    <FamilyList
      id="tour-step-2"
      :items="state.families.value"
      :total-items="state.totalItems.value"
      :loading="state.loading.value"
      :items-per-page="state.itemsPerPage.value!"
      :search="state.familyListSearchQuery.value!"
      :sortBy="state.sortBy.value!"
      @update:options="actions.handleListOptionsUpdate"
      @update:itemsPerPage="actions.setItemsPerPage"
      @update:search="actions.handleSearchUpdate"
      @view="actions.navigateToFamilyDetail"
      @delete="actions.confirmDelete"
      @create="actions.openAddDrawer"
      @onImportClick="importDialog = true"
    />

    <!-- Add Family Drawer -->
    <BaseCrudDrawer v-model="state.addDrawer.value" @close="actions.handleFamilyAddClosed">
      <FamilyAddView v-if="state.addDrawer" @close="actions.handleFamilyAddClosed" />
    </BaseCrudDrawer>

    <!-- Import Dialog -->
    <BaseImportDialog
      v-model="importDialog"
      :title="t('family.import.title')"
      :label="t('family.import.selectFile')"
      :loading="importComposableState.isImporting.value"
      :max-file-size="5 * 1024 * 1024"
      @update:model-value="importDialog = $event"
      @import="triggerImport"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { FamilySearch, FamilyList } from '@/components/family';
import { useFamilyList } from '@/composables/family/logic/useFamilyList';
import FamilyAddView from '@/views/family/FamilyAddView.vue';
import BaseImportDialog from '@/components/common/BaseImportDialog.vue'; // New import
import { useFamilyImportExport } from '@/composables/family/useFamilyImportExport'; // New import
import { useGlobalSnackbar } from '@/composables'; // New import

const { state, actions } = useFamilyList();
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const importDialog = ref(false);

const { state: importComposableState, actions: importComposableActions } = useFamilyImportExport('');

const triggerImport = async (file: File) => {
  if (!file) {
    showSnackbar(t('family.messages.noFileSelected'), 'warning');
    return;
  }

  importComposableState.importFile.value = file;

  try {
    await importComposableActions.importData();
    importDialog.value = false;
    actions.refetch();
  } catch (error: any) {
    console.error("Import operation failed:", error);
    showSnackbar(error.message || t('family.import.error'), 'error');
  }
};

</script>
