<template>
  <div data-testid="family-list-view">
    <FamilySearch id="tour-step-1" @update:filters="handleFilterUpdate" />
    <FamilyList id="tour-step-2" :items="list.items" :total-items="familyStore.list.totalItems" :loading="familyStore.list.loading"
      :items-per-page="itemsPerPage" :search="currentFilters.searchQuery || ''"
      @update:options="handleListOptionsUpdate" @update:itemsPerPage="itemsPerPage = $event"
      @update:search="handleSearchUpdate" @view="openDetailDrawer"
      @delete="confirmDelete" @create="openAddDrawer" @edit="openEditDrawer" />

    <!-- Add Family Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('family.form.addTitle')" icon="mdi-plus" @close="handleFamilyClosed">
      <FamilyAddView v-if="addDrawer" @close="handleFamilyClosed" @saved="handleFamilySaved" />
    </BaseCrudDrawer>

    <!-- Edit Family Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('family.form.editTitle')" icon="mdi-pencil" @close="handleFamilyClosed">
      <FamilyEditView v-if="selectedItemId && editDrawer" :initial-family-id="selectedItemId as string" @close="handleFamilyClosed"
        @saved="handleFamilySaved" />
    </BaseCrudDrawer>

    <!-- Detail Family Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('family.detail.title')" icon="mdi-information-outline" @close="handleDetailClosed">
      <FamilyDetailView v-if="selectedItemId && detailDrawer" :family-id="selectedItemId as string" @close="handleDetailClosed"
        @edit-family="openEditDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useFamilyStore } from '@/stores/family.store';
import { FamilySearch, FamilyList } from '@/components/family';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter, Family } from '@/types';
import { useFamilyTour } from '@/composables';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue'; // New import
import { useCrudDrawer } from '@/composables/useCrudDrawer'; // New import
import FamilyAddView from '@/views/family/FamilyAddView.vue'; // New import
import FamilyEditView from '@/views/family/FamilyEditView.vue'; // New import
import FamilyDetailView from '@/views/family/FamilyDetailView.vue'; // New import

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
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

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

// No longer navigating to separate routes, open drawers directly
const navigateToAddFamily = () => {
  openAddDrawer();
};

const navigateToViewFamily = (family: Family) => {
  openDetailDrawer(family.id);
};

const openEditFamily = (family: Family) => {
  openEditDrawer(family.id);
}

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

const handleFamilySaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  familyStore._loadItems(); // Reload list after save
};

const handleFamilyClosed = () => {
  closeAllDrawers(); // Close whichever drawer was open
};

const handleDetailClosed = () => {
  closeAllDrawers(); // Close the detail drawer
};
</script>
