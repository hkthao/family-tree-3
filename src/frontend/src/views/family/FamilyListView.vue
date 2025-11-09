<template>
  <div data-testid="family-list-view">
    <FamilySearch @update:filters="handleFilterUpdate" />
    <FamilyList :items="list.items" :total-items="familyStore.list.totalItems" :loading="familyStore.list.loading"
      :items-per-page="itemsPerPage" :search="currentFilters.searchQuery || ''"
      @update:options="handleListOptionsUpdate" @update:itemsPerPage="itemsPerPage = $event"
      @update:search="handleSearchUpdate" @view="navigateToViewFamily"
      @delete="confirmDelete" @create="navigateToAddFamily" @ai-create="openAiInputDialog" />
    <!-- Confirm Delete Dialog -->
    <ConfirmDeleteDialog :model-value="deleteConfirmDialog" :title="t('confirmDelete.title')"
      :message="t('confirmDelete.message', { name: familyToDelete?.name || '' })" @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel" />
    <!-- AI Input Dialog -->
    <NLFamilyPopup :model-value="aiInputDialog" @update:model-value="aiInputDialog = $event" @save="handleAiSave" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { FamilySearch, FamilyList, NLFamilyPopup } from '@/components/family';
import { ConfirmDeleteDialog } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter, Family, GeneratedDataResponse } from '@/types';

const { t } = useI18n();
const router = useRouter();

const familyStore = useFamilyStore();
const { list } = storeToRefs(familyStore);
const notificationStore = useNotificationStore();

const currentFilters = ref<FamilyFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const deleteConfirmDialog = ref(false);
const familyToDelete = ref<Family | undefined>(undefined);
const aiInputDialog = ref(false);

const handleFilterUpdate = async (filters: FamilyFilter) => {
  currentFilters.value = filters;
  familyStore.list.filter = currentFilters.value;
  await familyStore._loadItems()
};

const handleSearchUpdate = async (search: string) => {
  currentFilters.value.searchQuery = search;
  familyStore.list.filter = currentFilters.value;
  await familyStore._loadItems();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  familyStore.setPage(options.page);
  familyStore.setItemsPerPage(options.itemsPerPage);
  familyStore.setSortBy(options.sortBy);
};

const navigateToAddFamily = () => {
  router.push('/family/add');
};

const navigateToViewFamily = (family: Family) => {
  router.push(`/family/detail/${family.id}`);
};

const confirmDelete = (family: Family) => {
  familyToDelete.value = family;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (familyToDelete.value) {
    try {
      await familyStore.deleteItem(familyToDelete.value.id);
      notificationStore.showSnackbar(
        t('family.management.messages.deleteSuccess'),
        'success',
      );
    } catch (error) {
      notificationStore.showSnackbar(
        t('family.management.messages.deleteError'),
        'error',
      );
    }
  }
  deleteConfirmDialog.value = false;
  familyToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  familyToDelete.value = undefined;
};

const openAiInputDialog = () => {
  aiInputDialog.value = true;
};

const handleAiSave = async (generatedData: GeneratedDataResponse) => {
  try {
    if (generatedData.families.length > 0) {
      for (const family of generatedData.families) {
        await familyStore.addItem(family);
      }
    }
    notificationStore.showSnackbar(
      t('aiInput.saveSuccess'),
      'success',
    );
    console.log('handleAiSave success path reached');
  } catch (error) {
    console.error('Error saving generated data:', error);
    notificationStore.showSnackbar(
      t('aiInput.saveError'),
      'error',
    );
  } finally {
    await familyStore._loadItems(); // Refresh the family list after saving
  }
};
</script>
