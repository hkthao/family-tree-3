<template>
  <div data-testid="relationship-list-view">
    <RelationshipSearch @update:filters="handleFilterUpdate" />

    <RelationshipList
      :search="currentFilters.searchQuery || ''"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      @create="openAddDrawer"
      @view-member="navigateToMemberDetailView"
    />

    <!-- Add Relationship Drawer -->
    <BaseCrudDrawer v-model="addDrawer" :title="t('relationship.form.addTitle')" icon="mdi-plus" @close="closeAddDrawer">
      <RelationshipAddView v-if="addDrawer" @close="closeAddDrawer" @saved="handleRelationshipSaved" />
    </BaseCrudDrawer>

    <!-- Edit Relationship Drawer -->
    <BaseCrudDrawer v-model="editDrawer" :title="t('relationship.form.editTitle')" icon="mdi-pencil" @close="closeEditDrawer">
      <RelationshipEditView v-if="selectedItemId && editDrawer" :relationship-id="selectedItemId as string" @close="closeEditDrawer"
        @saved="handleRelationshipSaved" />
    </BaseCrudDrawer>

    <!-- Detail Relationship Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" :title="t('relationship.detail.title')" icon="mdi-information-outline" @close="closeDetailDrawer">
      <RelationshipDetailView v-if="selectedItemId && detailDrawer" :relationship-id="selectedItemId as string" @close="closeDetailDrawer"
        @edit="openEditDrawer" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
// import { useRouter } from 'vue-router'; // Removed as no longer used for navigation directly
import { useRelationshipStore } from '@/stores/relationship.store';
import { RelationshipSearch, RelationshipList } from '@/components/relationship';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer } from '@/composables';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { RelationshipFilter, Relationship } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import RelationshipAddView from '@/views/relationship/RelationshipAddView.vue';
import RelationshipEditView from '@/views/relationship/RelationshipEditView.vue';
import RelationshipDetailView from '@/views/relationship/RelationshipDetailView.vue';

const { t } = useI18n();

// const router = useRouter(); // Removed

const emit = defineEmits(['view-member']); // Add emit



const relationshipStore = useRelationshipStore();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const currentFilters = ref<RelationshipFilter>({});
const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

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

const loadRelationships = async () => {
  relationshipStore.list.filters = {
    ...relationshipStore.list.filters,
    ...currentFilters.value,
  };
  relationshipStore.setItemsPerPage(itemsPerPage.value); // Ensure itemsPerPage is set
  await relationshipStore._loadItems();
};

const navigateToMemberDetailView = (memberId: string) => {
  emit('view-member', memberId);
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

const handleRelationshipSaved = () => {
  closeAllDrawers(); // Close whichever drawer was open
  loadRelationships(); // Reload list after save
};

onMounted(() => {
  loadRelationships();
});
</script>