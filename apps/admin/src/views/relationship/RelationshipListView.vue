<template>
  <div data-testid="relationship-list-view">
    <RelationshipSearch @update:filters="handleFilterUpdate" />

    <RelationshipList
      :search="currentFilters.searchQuery || ''"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @view="navigateToDetailView"
      @edit="navigateToEditRelationship"
      @delete="confirmDelete"
      @create="navigateToAddRelationship"
      @view-member="navigateToMemberDetailView"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useRelationshipStore } from '@/stores/relationship.store';
import { RelationshipSearch, RelationshipList } from '@/components/relationship';
import { useConfirmDialog } from '@/composables/useConfirmDialog';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { RelationshipFilter, Relationship } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

const { t } = useI18n();
const router = useRouter();

const relationshipStore = useRelationshipStore();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar

const currentFilters = ref<RelationshipFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const loadRelationships = async () => {
  relationshipStore.list.filters = {
    ...relationshipStore.list.filters,
    ...currentFilters.value,
  };
  relationshipStore.setItemsPerPage(itemsPerPage.value); // Ensure itemsPerPage is set
  await relationshipStore._loadItems();
};

const navigateToDetailView = (relationship: Relationship) => {
  // Assuming a detail view for relationship exists, similar to member
  router.push(`/relationship/detail/${relationship.id}`);
};

const navigateToAddRelationship = () => {
  router.push('/relationship/add');
};

const navigateToEditRelationship = (relationship: Relationship) => {
  router.push(`/relationship/edit/${relationship.id}`);
};

const navigateToMemberDetailView = (memberId: string) => {
  router.push(`/member/detail/${memberId}`);
};

const handleFilterUpdate = (filters: RelationshipFilter) => {
  currentFilters.value = { ...currentFilters.value, ...filters };
  loadRelationships();
};

const handleSearchUpdate = (searchQuery: string) => {
  currentFilters.value.searchQuery = searchQuery;
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

const confirmDelete = async (relationship: Relationship) => {
  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('relationship.list.confirmDelete', { name: relationship.sourceMember?.fullName || '' }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    const result = await relationshipStore.deleteItem(relationship.id!);
    if (result.ok) {
      showSnackbar(
        t('relationship.messages.deleteSuccess'),
        'success',
      );
      await loadRelationships(); // Reload relationships after deletion
    } else {
      showSnackbar(
        result.error?.message || t('relationship.messages.deleteError'),
        'error',
      );
    }
  }
};

onMounted(() => {
  loadRelationships();
});
</script>