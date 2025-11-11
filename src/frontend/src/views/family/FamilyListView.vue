<template>
  <div data-testid="family-list-view">
    <FamilySearch id="tour-step-1" @update:filters="handleFilterUpdate" />
    <FamilyList id="tour-step-2" :items="list.items" :total-items="familyStore.list.totalItems" :loading="familyStore.list.loading"
      :items-per-page="itemsPerPage" :search="currentFilters.searchQuery || ''"
      @update:options="handleListOptionsUpdate" @update:itemsPerPage="itemsPerPage = $event"
      @update:search="handleSearchUpdate" @view="navigateToViewFamily"
      @delete="confirmDelete" @create="navigateToAddFamily" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { FamilySearch, FamilyList } from '@/components/family';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter, Family } from '@/types';
import { useFamilyTour } from '@/composables';

const { t } = useI18n();
const router = useRouter();

const familyStore = useFamilyStore();
const { list } = storeToRefs(familyStore);
const notificationStore = useNotificationStore();
const { showConfirmDialog } = useConfirmDialog();
useFamilyTour();

const currentFilters = ref<FamilyFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

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
  familyStore.setListOptions(options);
};

const navigateToAddFamily = () => {
  router.push('/family/add');
};

const navigateToViewFamily = (family: Family) => {
  router.push(`/family/detail/${family.id}`);
};

const confirmDelete = async (family: Family) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('confirmDelete.message', { name: family.name || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    try {
      await familyStore.deleteItem(family.id);
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
};
</script>
