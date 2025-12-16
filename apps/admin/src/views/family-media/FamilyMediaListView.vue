<template>
  <div data-testid="family-media-list-view">
    <FamilyMediaSearch @update:filters="handleFilterUpdate" />
    <FamilyMediaList :items="familyMediaList" :total-items="totalItems" :loading="isLoading || isDeleting"
      :search="filters.searchQuery"
      @update:options="handleListOptionsUpdate" @update:search="handleSearchUpdate" @view="openDetailDrawer"
      @delete="confirmDelete" @create="openAddDrawer()" :allow-add="allowAdd" :allow-edit="allowEdit" :allow-delete="allowDelete" />

    <!-- Add/Edit/Detail Drawers (similar to MemberListView) -->
    <!-- Detail Family Media Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <FamilyMediaDetailView v-if="selectedItemId && detailDrawer" :family-id="props.familyId"
        :family-media-id="selectedItemId" @close="handleDetailClosed" />
    </BaseCrudDrawer>

    <!-- Add Family Media Drawer -->
    <BaseCrudDrawer v-if="allowAdd" v-model="addDrawer" @close="handleMediaClosed">
      <FamilyMediaAddView v-if="addDrawer" :family-id="props.familyId" @close="handleMediaClosed"
        @saved="handleMediaSaved" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { nextTick, toRef, computed } from 'vue';
import { useCrudDrawer } from '@/composables';
import type { FamilyMediaFilter, ListOptions } from '@/types';
import { useFamilyMediaListQuery, useDeleteFamilyMediaMutation, useFamilyMediaListFilters, useFamilyMediaDeletion } from '@/composables/family-media';
import { useAuth } from '@/composables'; // Import useAuth
// Components
import FamilyMediaSearch from '@/components/family-media/FamilyMediaSearch.vue';
import FamilyMediaList from '@/components/family-media/FamilyMediaList.vue';
import FamilyMediaAddView from '@/views/family-media/FamilyMediaAddView.vue';

import FamilyMediaDetailView from '@/views/family-media/FamilyMediaDetailView.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';

const props = defineProps<{
  familyId: string;
}>();

const { isAdmin, isFamilyManager } = useAuth();
const allowAdd = computed(() => isAdmin.value || isFamilyManager.value(props.familyId));
const allowEdit = computed(() => isAdmin.value || isFamilyManager.value(props.familyId)); // Edit currently not implemented for media, but for future proofing
const allowDelete = computed(() => isAdmin.value || isFamilyManager.value(props.familyId));

const familyIdRef = toRef(props, 'familyId');
const familyMediaListFiltersComposable = useFamilyMediaListFilters(familyIdRef);
const { filters, listOptions, setItemsPerPage, setPage, setSortBy, setFilters } = familyMediaListFiltersComposable;
const { familyMediaList, totalItems, isLoading, refetch } = useFamilyMediaListQuery(filters, listOptions);
const { mutateAsync: deleteFamilyMediaMutation } = useDeleteFamilyMediaMutation();

const { isDeleting, confirmAndDelete } = useFamilyMediaDeletion({
  familyId: familyIdRef,
  deleteMutation: deleteFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.deleteSuccess',
  errorMessageKey: 'familyMedia.messages.deleteError',
  confirmationTitleKey: 'confirmDelete.title',
  confirmationMessageKey: 'familyMedia.list.confirmDelete',
  refetchList: refetch,
});

  const {
    addDrawer,
    detailDrawer,
    selectedItemId,
    openAddDrawer,
    openDetailDrawer,
    closeAllDrawers,
  } = useCrudDrawer<string>();
const handleFilterUpdate = (newFilters: FamilyMediaFilter) => {
  setFilters(newFilters);
};

const handleSearchUpdate = (newSearchQuery: string) => {
  setFilters({ ...filters.value, searchQuery: newSearchQuery });
};

const handleListOptionsUpdate = async (options: ListOptions) => {
  await nextTick();
  setPage(options.page || 1);
  setItemsPerPage(options.itemsPerPage || 10);
  setSortBy(options.sortBy);
};

const confirmDelete = async (familyMediaId: string) => {
  const media = familyMediaList.value.find(m => m.id === familyMediaId);
  if (!media) {
    // Optionally show a snackbar here that the media was not found
    return;
  }
  await confirmAndDelete(familyMediaId, media.fileName || '');
};

const handleMediaSaved = () => {
  closeAllDrawers();
  refetch(); // Refetch the list after add/edit/delete
};

const handleMediaClosed = () => {
  closeAllDrawers();
};

const handleDetailClosed = () => {
  closeAllDrawers();
};
</script>