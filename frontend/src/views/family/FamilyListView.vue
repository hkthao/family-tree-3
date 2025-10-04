<template>
  <FamilySearch @update:filters="handleFilterUpdate" />
  <FamilyList :items="items" :total-items="familyStore.totalItems" :loading="familyStore.loading"
    :items-per-page="itemsPerPage"  @update:options="handleListOptionsUpdate"
    @update:itemsPerPage="itemsPerPage = $event" @view="navigateToViewFamily" @edit="navigateToEditFamily"
    @delete="confirmDelete" @create="navigateToAddFamily" />
  <!-- Confirm Delete Dialog -->
  <ConfirmDeleteDialog :model-value="deleteConfirmDialog" :title="t('confirmDelete.title')"
    :message="t('confirmDelete.message', { name: familyToDelete?.name || '' })" @confirm="handleDeleteConfirm"
    @cancel="handleDeleteCancel" />
  <!-- Snackbar -->
  <v-snackbar v-model="notificationStore.snackbar.show" :color="notificationStore.snackbar.color" timeout="3000">
    {{ notificationStore.snackbar.message }}
  </v-snackbar>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { FamilySearch, FamilyList } from '@/components/family';
import { ConfirmDeleteDialog } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter, Family } from '@/types';

const { t } = useI18n();
const router = useRouter();

const familyStore = useFamilyStore();
const { items } = storeToRefs(familyStore);
const notificationStore = useNotificationStore();

const currentFilters = ref<FamilyFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const deleteConfirmDialog = ref(false);
const familyToDelete = ref<Family | undefined>(undefined);

const handleFilterUpdate = (filters: FamilyFilter) => {
  currentFilters.value = filters;
  familyStore.filter = currentFilters.value;
  familyStore._loadItems()
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

const navigateToEditFamily = (family: Family) => {
  router.push(`/family/edit/${family.id}`);
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
</script>
