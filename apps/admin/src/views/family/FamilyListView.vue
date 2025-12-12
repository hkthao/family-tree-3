<template>
  <div data-testid="family-list-view">
    <FamilySearch id="tour-step-1" @update:filters="handleFilterUpdate" />
    <FamilyList
      id="tour-step-2"
      :items="families"
      :total-items="totalItems"
      :loading="loading"
      :items-per-page="itemsPerPage!"
      :search="familyListSearchQuery!"
      :sortBy="sortBy!"
      @update:options="handleListOptionsUpdate"
      @update:itemsPerPage="setItemsPerPage($event)"
      @update:search="handleSearchUpdate"
      @view="navigateToFamilyDetail"
      @delete="confirmDelete"
      @create="openAddDrawer"
    />

    <!-- Add Family Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleFamilyAddClosed">
      <FamilyAddView v-if="addDrawer" @close="handleFamilyAddClosed" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilySearch, FamilyList } from '@/components/family';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer, useFamilyTour } from '@/composables';
import type { FamilyFilter, Family } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import { useRouter } from 'vue-router';
import FamilyAddView from './FamilyAddView.vue';
import { useFamilyListFilters, useFamiliesQuery, useDeleteFamilyMutation } from '@/composables/family';

const router = useRouter();
const { t } = useI18n();

const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();
useFamilyTour();

const familyListFiltersComposables = useFamilyListFilters();
const {
  searchQuery: familyListSearchQuery,
  page,
  itemsPerPage,
  sortBy,
  filters,
} = toRefs(familyListFiltersComposables);

const {
  setPage,
  setItemsPerPage,
  setSortBy,
  setSearchQuery,
  setFilters,
} = familyListFiltersComposables;

const { families, totalItems, loading, refetch } = useFamiliesQuery(filters);
const { mutate: deleteFamily } = useDeleteFamilyMutation();

const {
  addDrawer,
  openAddDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const handleFilterUpdate = (newFilters: FamilyFilter) => {
  setFilters(newFilters);
};

const navigateToFamilyDetail = (item: Family) => {
  router.push({ name: 'FamilyDetail', params: { id: item.id } });
}

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

const confirmDelete = async (family: Family) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('confirmDelete.message', { name: family.name || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    deleteFamily(family.id, {
      onSuccess: () => {
        showSnackbar(
          t('family.management.messages.deleteSuccess'),
          'success',
        );
        refetch(); // Refetch the list after successful deletion
      },
      onError: () => {
        showSnackbar(
          t('family.management.messages.deleteError'),
          'error',
        );
      },
    });
  }
};

const handleFamilyAddClosed = () => {
  closeAllDrawers(); // Close the drawer on cancel
  refetch(); // Refetch the list in case a family was added
};
</script>
