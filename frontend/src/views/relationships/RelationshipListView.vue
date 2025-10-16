<template>
  <RelationshipSearch @update:filters="handleFilterUpdate" />

  <RelationshipList
    @update:options="handleListOptionsUpdate"
    @view="navigateToDetailView"
    @edit="navigateToEditRelationship"
    @delete="confirmDelete"
    @create="navigateToAddRelationship"
  />

  <!-- Confirm Delete Dialog -->
  <ConfirmDeleteDialog
    :model-value="deleteConfirmDialog"
    :title="t('confirmDelete.title')"
    :message="
      t('relationship.list.confirmDelete', { name: relationshipToDelete?.sourceMember?.fullName || '' })
    "
    @confirm="handleDeleteConfirm"
    @cancel="handleDeleteCancel"
  />

  <!-- Global Snackbar -->
  <v-snackbar
    v-if="notificationStore.snackbar"
    v-model="notificationStore.snackbar.show"
    :color="notificationStore.snackbar.color"
    timeout="3000"
  >
    {{ notificationStore.snackbar.message }}
  </v-snackbar>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useRelationshipStore } from '@/stores/relationship.store';
import { RelationshipSearch, RelationshipList } from '@/components/relationships'; // Grouped imports
import { ConfirmDeleteDialog } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { RelationshipFilter, Relationship } from '@/types';

const { t } = useI18n();
const router = useRouter();

const relationshipStore = useRelationshipStore();
const { items, loading } = storeToRefs(relationshipStore); // Removed currentPage as it's not directly used here
const notificationStore = useNotificationStore();

const currentFilters = ref<RelationshipFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const deleteConfirmDialog = ref(false);
const relationshipToDelete = ref<Relationship | undefined>(undefined);

const loadRelationships = async () => {
  relationshipStore.filter = {
    ...relationshipStore.filter,
    ...currentFilters.value,
  };
  relationshipStore.setItemsPerPage(itemsPerPage.value); // Ensure itemsPerPage is set
  await relationshipStore._loadItems();
};

const navigateToDetailView = (relationship: Relationship) => {
  // Assuming a detail view for relationship exists, similar to member
  router.push(`/relationships/detail/${relationship.id}`);
};

const navigateToAddRelationship = () => {
  router.push('/relationships/add');
};

const navigateToEditRelationship = (relationship: Relationship) => {
  router.push(`/relationships/edit/${relationship.id}`);
};

const handleFilterUpdate = (filters: RelationshipFilter) => {
  currentFilters.value = filters;
  loadRelationships();
};

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  relationshipStore.setPage(options.page);
  relationshipStore.setItemsPerPage(options.itemsPerPage);
  if (options.sortBy && options.sortBy.length > 0) {
    relationshipStore.setSortBy(options.sortBy);
  } else {
    relationshipStore.setSortBy([]);
  }
};

const confirmDelete = (relationship: Relationship) => {
  relationshipToDelete.value = relationship;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (relationshipToDelete.value) {
    const result = await relationshipStore.deleteItem(relationshipToDelete.value.id!);
    if (result.ok) {
      notificationStore.showSnackbar(
        t('relationship.messages.deleteSuccess'),
        'success',
      );
      await loadRelationships(); // Reload relationships after deletion
    } else {
      notificationStore.showSnackbar(
        result.error?.message || t('relationship.messages.deleteError'),
        'error',
      );
    }
  }
  deleteConfirmDialog.value = false;
  relationshipToDelete.value = undefined;
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  relationshipToDelete.value = undefined;
};

onMounted(() => {
  loadRelationships();
});
</script>