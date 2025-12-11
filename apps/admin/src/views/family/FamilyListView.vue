<template>
  <div data-testid="family-list-view">
    <FamilySearch id="tour-step-1" @update:filters="handleFilterUpdate" />
    <FamilyList id="tour-step-2" :items="list.items" :total-items="familyStore.list.totalItems"
      :loading="familyStore.list.loading" :items-per-page="itemsPerPage" :search="currentFilters.searchQuery || ''"
      @update:options="handleListOptionsUpdate" @update:itemsPerPage="itemsPerPage = $event"
      @update:search="handleSearchUpdate" @view="navigateToFamilyDetail" @delete="confirmDelete"
      @create="openAddDrawer" />

    <!-- Add Family Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleFamilyAddClosed">
      <FamilyAddView v-if="addDrawer" @close="handleFamilyAddClosed" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useFamilyStore } from '@/stores/family.store';
import { FamilySearch, FamilyList } from '@/components/family'; // Removed FamilyForm
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer, useFamilyTour } from '@/composables';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter, Family } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useRouter } from 'vue-router';
import FamilyAddView from './FamilyAddView.vue';

const router = useRouter();
const { t } = useI18n();
const familyStore = useFamilyStore();
const { list } = storeToRefs(familyStore);
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();
useFamilyTour();

const currentFilters = ref<FamilyFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const {
  addDrawer,
  openAddDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const handleFilterUpdate = async (filters: FamilyFilter) => {
  currentFilters.value = filters;
  familyStore.list.filter = currentFilters.value;
  await familyStore._loadItems()
};

const navigateToFamilyDetail = (item: Family) => {
  router.push({ name: 'FamilyDetail', params: { id: item.id } });
}

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
      showSnackbar(
        t('family.management.messages.deleteSuccess'),
        'success',
      );
    } catch (error) {
      showSnackbar(
        t('family.management.messages.deleteError'),
        'error',
      );
    }
  }
};

const handleFamilyAddClosed = () => {
  closeAllDrawers(); // Close the drawer on cancel
};
</script>
